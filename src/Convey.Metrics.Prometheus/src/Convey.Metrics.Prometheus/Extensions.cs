using Convey.Metrics.Prometheus.Internals;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.SystemMetrics;

namespace Convey.Metrics.Prometheus
{
    public static class Extensions
    {
        public static IConveyBuilder AddPrometheus(this IConveyBuilder builder)
        {
            var prometheusOptions = builder.GetOptions<PrometheusOptions>("prometheus");
            builder.Services.AddSingleton(prometheusOptions);
            if (!prometheusOptions.Enabled)
            {
                return builder;
            }

            builder.Services.AddHostedService<PrometheusJob>();
            builder.Services.AddSingleton<PrometheusMiddleware>();
            builder.Services.AddSystemMetrics();

            return builder;
        }

        public static IApplicationBuilder UsePrometheus(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<PrometheusOptions>();
            if (!options.Enabled)
            {
                return app;
            }

            var endpoint = string.IsNullOrWhiteSpace(options.Endpoint) ? "/metrics" :
                options.Endpoint.StartsWith("/") ? options.Endpoint : $"/{options.Endpoint}";

            return app
                .UseMiddleware<PrometheusMiddleware>()
                .UseHttpMetrics()
                .UseGrpcMetrics()
                .UseMetricServer(endpoint);
        }
    }
}