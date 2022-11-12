using Convey.Tracing.Jaeger.Builders;
using Convey.Tracing.Jaeger.Tracers;
using Convey.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Extensions.Docker.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Shims.OpenTracing;
using OpenTelemetry.Trace;
using OpenTracing;
using OpenTracing.Util;
using System;
using System.Linq;
using System.Threading;

namespace Convey.Tracing.Jaeger;

public static class Extensions
{
    private const string SectionName = "jaeger";
    private const string RegistryName = "tracing.jaeger";

    private static int _initialized;

    public static IConveyBuilder AddJaeger(this IConveyBuilder builder, string sectionName = SectionName)
    {
        var options = builder.GetOptions<JaegerOptions>(string.IsNullOrWhiteSpace(sectionName) ? SectionName : sectionName);
        return builder.AddJaeger(options);
    }

    public static IConveyBuilder AddJaeger(this IConveyBuilder builder, Func<IJaegerOptionsBuilder, IJaegerOptionsBuilder> buildOptions)
    {
        var options = buildOptions(new JaegerOptionsBuilder()).Build();
        return builder.AddJaeger(options);
    }

    public static IConveyBuilder AddJaeger(this IConveyBuilder builder, JaegerOptions options)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return builder;
        }

        builder.Services.AddSingleton(options);

        if (!options.Enabled)
        {
            var defaultTracer = ConveyDefaultTracer.Create();
            builder.Services.AddSingleton(defaultTracer);
            return builder;
        }

        if (!builder.TryRegister(RegistryName))
        {
            return builder;
        }

        builder.Services.AddOpenTelemetryTracing(telemetryBuilder =>
            telemetryBuilder
                .SetResourceBuilder(
                    ResourceBuilder
                        .CreateDefault()
                        .AddDetector(new DockerResourceDetector()))
                .AddAspNetCoreInstrumentation(string.Empty, aspCoreOptions =>
                {
                    aspCoreOptions.Filter = context =>
                    {
                        return options.ExcludePaths?.Contains(context.Request.Path.ToString()) != true;
                    };
                })
                .AddHttpClientInstrumentation()
                .AddJaegerExporter(jaegerOptions =>
                {
                    jaegerOptions.AgentHost = options?.UdpHost ?? "localhost";
                    jaegerOptions.AgentPort = options?.UdpPort ?? 6831;

                    var senderType = string.IsNullOrWhiteSpace(options.Sender) ? "udp" : options.Sender?.ToLowerInvariant();

                    jaegerOptions.Protocol = senderType switch
                    {
                        "http" => JaegerExportProtocol.HttpBinaryThrift,
                        "udp" => JaegerExportProtocol.UdpCompactThrift,
                        _ => throw new InvalidOperationException($"Invalid Jaeger sender type: '{senderType}'.")
                    };

                    jaegerOptions.MaxPayloadSizeInBytes = options.MaxPacketSize <= 0 ? 4096 : options.MaxPacketSize;

                    if (senderType == "http" && options.HttpSender is not null)
                    {
                        jaegerOptions.Endpoint = new Uri(options.HttpSender.Endpoint ?? "http://localhost:14268/api/traces");
                        jaegerOptions.MaxPayloadSizeInBytes = options.HttpSender.MaxPacketSize <= 0 ? 4096 : options.HttpSender.MaxPacketSize;
                    }
                })
        );

        builder.Services.AddSingleton<ITracer>(serviceProvider =>
        {
            var appOptions = serviceProvider.GetRequiredService<AppOptions>();
            var traceProvider = serviceProvider.GetRequiredService<TracerProvider>();
            var tracer = new TracerShim(traceProvider.GetTracer(appOptions.Name), Propagators.DefaultTextMapPropagator);
            GlobalTracer.RegisterIfAbsent(tracer);
            return tracer;
        });

        return builder;
    }

    public static IApplicationBuilder UseJaeger(this IApplicationBuilder app)
    {
        // Could be extended with some additional middleware
        using var scope = app.ApplicationServices.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<JaegerOptions>();
        return app;
    }
}