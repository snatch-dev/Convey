using System;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.AspNetCore.Endpoints;
using App.Metrics.AspNetCore.Health.Endpoints;
using App.Metrics.AspNetCore.Tracking;
using App.Metrics.Formatters.Prometheus;
using Convey.Metrics.AppMetrics.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.Metrics.AppMetrics
{
    public static class Extensions
    {
        private static bool _initialized;
        private const string SectionName = "metrics";
        private const string RegistryName = "metrics.metrics";

        public static IConveyBuilder AddMetrics(this IConveyBuilder builder, string sectionName = SectionName)
        {
            var options = builder.GetOptions<MetricsOptions>(sectionName);
            return builder.AddMetrics(options);
        }

        public static IConveyBuilder AddMetrics(this IConveyBuilder builder,
            Func<IMetricsOptionsBuilder, IMetricsOptionsBuilder> buildOptions)
        {
            var options = buildOptions(new MetricsOptionsBuilder()).Build();
            return builder.AddMetrics(options);
        }

        public static IConveyBuilder AddMetrics(this IConveyBuilder builder, MetricsOptions options)
        {
            builder.Services.AddSingleton(options);
            if (!builder.TryRegister(RegistryName) || !options.Enabled || _initialized)
            {
                return builder;
            }

            _initialized = true;
            var metricsBuilder = new MetricsBuilder().Configuration.Configure(cfg =>
            {
                var tags = options.Tags;
                if (tags == null)
                {
                    return;
                }

                tags.TryGetValue("app", out var app);
                tags.TryGetValue("env", out var env);
                tags.TryGetValue("server", out var server);
                cfg.AddAppTag(string.IsNullOrWhiteSpace(app) ? null : app);
                cfg.AddEnvTag(string.IsNullOrWhiteSpace(env) ? null : env);
                cfg.AddServerTag(string.IsNullOrWhiteSpace(server) ? null : server);
                foreach (var tag in tags)
                {
                    if (!cfg.GlobalTags.ContainsKey(tag.Key))
                    {
                        cfg.GlobalTags.Add(tag.Key, tag.Value);
                    }
                }
            });

            if (options.InfluxEnabled)
            {
                metricsBuilder.Report.ToInfluxDb(o =>
                {
                    o.InfluxDb.Database = options.Database;
                    o.InfluxDb.BaseUri = new Uri(options.InfluxUrl);
                    o.InfluxDb.CreateDataBaseIfNotExists = true;
                    o.FlushInterval = TimeSpan.FromSeconds(options.Interval);
                });
            }

            var metrics = metricsBuilder.Build();
            var metricsWebHostOptions = GetMetricsWebHostOptions(options);

            using (var serviceProvider = builder.Services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                builder.Services.AddHealth();
                builder.Services.AddHealthEndpoints(configuration);
                builder.Services.AddMetricsTrackingMiddleware(configuration);
                builder.Services.AddMetricsEndpoints(configuration);
                builder.Services.AddSingleton<IStartupFilter>(new DefaultMetricsEndpointsStartupFilter());
                builder.Services.AddSingleton<IStartupFilter>(new DefaultHealthEndpointsStartupFilter());
                builder.Services.AddSingleton<IStartupFilter>(new DefaultMetricsTrackingStartupFilter());
                builder.Services.AddMetricsReportingHostedService(metricsWebHostOptions.UnobservedTaskExceptionHandler);
                builder.Services.AddMetricsEndpoints(metricsWebHostOptions.EndpointOptions, configuration);
                builder.Services.AddMetricsTrackingMiddleware(metricsWebHostOptions.TrackingMiddlewareOptions,
                    configuration);
                builder.Services.AddMetrics(metrics);
            }

            return builder;
        }

        private static MetricsWebHostOptions GetMetricsWebHostOptions(MetricsOptions metricsOptions)
        {
            var options = new MetricsWebHostOptions();

            if (!metricsOptions.Enabled)
            {
                return options;
            }

            if (!metricsOptions.PrometheusEnabled)
            {
                return options;
            }

            options.EndpointOptions = endpointOptions =>
            {
                switch (metricsOptions.PrometheusFormatter?.ToLowerInvariant() ?? string.Empty)
                {
                    case "protobuf":
                        endpointOptions.MetricsEndpointOutputFormatter =
                            new MetricsPrometheusProtobufOutputFormatter();
                        break;
                    default:
                        endpointOptions.MetricsEndpointOutputFormatter =
                            new MetricsPrometheusTextOutputFormatter();
                        break;
                }
            };

            return options;
        }

        public static IApplicationBuilder UseMetrics(this IApplicationBuilder app)
        {
            MetricsOptions options;
            using (var scope = app.ApplicationServices.CreateScope())
            {
                options = scope.ServiceProvider.GetService<MetricsOptions>();
            }

            return !options.Enabled
                ? app
                : app
                    .UseHealthAllEndpoints()
                    .UseMetricsAllEndpoints()
                    .UseMetricsAllMiddleware();
        }
    }
}