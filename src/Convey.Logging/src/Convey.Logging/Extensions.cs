using System;
using System.Linq;
using Convey.Logging.Options;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;

namespace Convey.Logging
{
    public static class Extensions
    {
        public static IWebHostBuilder UseLogging(this IWebHostBuilder webHostBuilder, string applicationName = null,
            string serviceId = null)
            => webHostBuilder.UseSerilog((context, loggerConfiguration) =>
            {
                var options = context.Configuration.GetOptions<LoggerOptions>("logger");
                if (!Enum.TryParse<LogEventLevel>(options.Level, true, out var level))
                {
                    level = LogEventLevel.Information;
                }

                if (!string.IsNullOrWhiteSpace(options.ApplicationName))
                {
                    applicationName = options.ApplicationName;
                }

                var applicationNameEnv = Environment.GetEnvironmentVariable("APPLICATION_NAME");
                if (!string.IsNullOrWhiteSpace(applicationNameEnv))
                {
                    applicationName = applicationNameEnv;
                }
                
                if (!string.IsNullOrWhiteSpace(options.ServiceId))
                {
                    serviceId = options.ServiceId;
                }

                var serviceIdEnv = Environment.GetEnvironmentVariable("SERVICE_ID");
                if (!string.IsNullOrWhiteSpace(serviceIdEnv))
                {
                    serviceId = serviceIdEnv;
                }

                loggerConfiguration.Enrich.FromLogContext()
                    .MinimumLevel.Is(level)
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("ApplicationName", applicationName)
                    .Enrich.WithProperty("ServiceId", serviceId);

                options.ExcludePaths?.ToList().ForEach(p => loggerConfiguration.Filter
                    .ByExcluding(Matching.WithProperty<string>("RequestPath", n => n.EndsWith(p))));

                Configure(loggerConfiguration, level, options);
            });

        private static void Configure(LoggerConfiguration loggerConfiguration, LogEventLevel level,
            LoggerOptions options)
        {
            var consoleOptions = options.Console ?? new ConsoleOptions();
            var fileOptions = options.File ?? new FileOptions();
            var elkOptions = options.Elk ?? new ElkOptions();
            var seqOptions = options.Seq ?? new SeqOptions();
            if (consoleOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Console();
            }

            if (fileOptions.Enabled)
            {
                var path = string.IsNullOrWhiteSpace(fileOptions.Path) ? "logs/logs.txt" : fileOptions.Path;
                if (!Enum.TryParse<RollingInterval>(fileOptions.Interval, true, out var interval))
                {
                    interval = RollingInterval.Day;
                }

                loggerConfiguration.WriteTo.File(path, rollingInterval: interval);
            }

            if (elkOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elkOptions.Url))
                {
                    MinimumLogEventLevel = level,
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    IndexFormat = string.IsNullOrWhiteSpace(elkOptions.IndexFormat)
                        ? "logstash-{0:yyyy.MM.dd}"
                        : elkOptions.IndexFormat,
                    ModifyConnectionSettings = connectionConfiguration =>
                        elkOptions.BasicAuthEnabled
                            ? connectionConfiguration.BasicAuthentication(elkOptions.Username, elkOptions.Password)
                            : connectionConfiguration
                });
            }

            if (seqOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Seq(seqOptions.Url, apiKey: seqOptions.ApiKey);
            }
        }
    }
}