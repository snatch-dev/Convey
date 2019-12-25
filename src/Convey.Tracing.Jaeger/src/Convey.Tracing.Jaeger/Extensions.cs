using System;
using System.Threading;
using Convey.Tracing.Jaeger.Builders;
using Convey.Tracing.Jaeger.Middlewares;
using Convey.Tracing.Jaeger.Tracers;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Contrib.NetCore.Configuration;
using OpenTracing.Util;

namespace Convey.Tracing.Jaeger
{
    public static class Extensions
    {
        private static int _initialized;
        private const string SectionName = "jaeger";
        private const string RegistryName = "tracing.jaeger";

        public static IConveyBuilder AddJaeger(this IConveyBuilder builder, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }
            
            var options = builder.GetOptions<JaegerOptions>(sectionName);
            return builder.AddJaeger(options);
        }

        public static IConveyBuilder AddJaeger(this IConveyBuilder builder,
            Func<IJaegerOptionsBuilder, IJaegerOptionsBuilder> buildOptions)
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

            if (options.ExcludePaths is {})
            {
                builder.Services.Configure<AspNetCoreDiagnosticOptions>(o =>
                {
                    foreach (var path in options.ExcludePaths)
                    {
                        o.Hosting.IgnorePatterns.Add(x => x.Request.Path == path);
                    }
                });
            }

            builder.Services.AddOpenTracing();
            builder.Services.AddSingleton<ITracer>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var reporter = new RemoteReporter.Builder()
                    .WithSender(new UdpSender(options.UdpHost, options.UdpPort, options.MaxPacketSize))
                    .WithLoggerFactory(loggerFactory)
                    .Build();

                var sampler = GetSampler(options);

                var tracer = new Tracer.Builder(options.ServiceName)
                    .WithLoggerFactory(loggerFactory)
                    .WithReporter(reporter)
                    .WithSampler(sampler)
                    .Build();

                GlobalTracer.Register(tracer);

                return tracer;
            });

            return builder;
        }

        public static IApplicationBuilder UseJaeger(this IApplicationBuilder app)
        {
            JaegerOptions options;
            using (var scope = app.ApplicationServices.CreateScope())
            {
                options = scope.ServiceProvider.GetService<JaegerOptions>();
            }

            return options.Enabled ? app.UseMiddleware<JaegerHttpMiddleware>() : app;
        }

        private static ISampler GetSampler(JaegerOptions options)
        {
            switch (options.Sampler)
            {
                case "const": return new ConstSampler(true);
                case "rate": return new RateLimitingSampler(options.MaxTracesPerSecond);
                case "probabilistic": return new ProbabilisticSampler(options.SamplingRate);
                default: return new ConstSampler(true);
            }
        }
    }
}