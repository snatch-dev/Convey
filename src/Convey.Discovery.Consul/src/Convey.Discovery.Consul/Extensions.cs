using System;
using System.Threading.Tasks;
using Consul;
using Convey.Discovery.Consul.Builders;
using Convey.Discovery.Consul.Http;
using Convey.Discovery.Consul.MessageHandlers;
using Convey.Discovery.Consul.Registries;
using Convey.HTTP;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.Discovery.Consul
{
    public static class Extensions
    {
        private const string SectionName = "consul";
        private const string RegistryName = "discovery.consul";

        public static IConveyBuilder AddConsul(this IConveyBuilder builder, string sectionName = SectionName,
            string httpClientSectionName = "httpClient")
        {
            var consulOptions = builder.GetOptions<ConsulOptions>(sectionName);
            var httpClientOptions = builder.GetOptions<HttpClientOptions>(httpClientSectionName);
            return builder.AddConsul(consulOptions, httpClientOptions);
        }

        public static IConveyBuilder AddConsul(this IConveyBuilder builder,
            Func<IConsulOptionsBuilder, IConsulOptionsBuilder> buildOptions, HttpClientOptions httpClientOptions)
        {
            var options = buildOptions(new ConsulOptionsBuilder()).Build();
            return builder.AddConsul(options, httpClientOptions);
        }

        public static IConveyBuilder AddConsul(this IConveyBuilder builder, ConsulOptions options,
            HttpClientOptions httpClientOptions)
        {
            builder.Services.AddSingleton(options);
            if (!options.Enabled || !builder.TryRegister(RegistryName))
            {
                return builder;
            }

            if (httpClientOptions.Type?.ToLowerInvariant() == "consul")
            {
                builder.Services.AddTransient<ConsulServiceDiscoveryMessageHandler>();
                builder.Services.AddHttpClient<IConsulHttpClient, ConsulHttpClient>()
                    .AddHttpMessageHandler<ConsulServiceDiscoveryMessageHandler>();
                builder.Services.AddHttpClient<IHttpClient, ConsulHttpClient>()
                    .AddHttpMessageHandler<ConsulServiceDiscoveryMessageHandler>();
            }

            builder.Services.AddTransient<IConsulServicesRegistry, ConsulServicesRegistry>();
            builder.Services.AddSingleton<IConsulClient>(c => new ConsulClient(cfg =>
            {
                if (!string.IsNullOrEmpty(options.Url))
                {
                    cfg.Address = new Uri(options.Url);
                }
            }));

            var registration = builder.CreateConsulAgentRegistration(options);
            if (registration is null)
            {
                return builder;
            }

            builder.Services.AddSingleton(registration);
            builder.AddBuildAction(sp =>
            {
                var consulRegistration = sp.GetService<AgentServiceRegistration>();
                var client = sp.GetService<IConsulClient>();

                client.Agent.ServiceRegister(consulRegistration);
            });

            return builder;
        }

        public static void AddConsulHttpClient(this IConveyBuilder builder, string clientName, string serviceName)
            => builder.Services.AddHttpClient<IHttpClient, ConsulHttpClient>(clientName)
                .AddHttpMessageHandler(c => new ConsulServiceDiscoveryMessageHandler(
                    c.GetService<IConsulServicesRegistry>(),
                    c.GetService<ConsulOptions>(), serviceName, true));

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<ConsulOptions>();
            if (options.PingEnabled)
            {
                var pingEndpoint = string.IsNullOrWhiteSpace(options.PingEndpoint) ? string.Empty :
                    options.PingEndpoint.StartsWith("/") ? options.PingEndpoint : $"/{options.PingEndpoint}";
                if (pingEndpoint.EndsWith("/"))
                {
                    pingEndpoint = pingEndpoint.Substring(0, pingEndpoint.Length - 1);
                }

                app.Map(pingEndpoint, ab => ab.Run(ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    return Task.CompletedTask;
                }));
            }

            app.DeregisterConsulServiceOnShutdown();
            return app;
        }

        private static void DeregisterConsulServiceOnShutdown(this IApplicationBuilder app)
        {
            var applicationLifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                var registration = app.ApplicationServices.GetService<AgentServiceRegistration>();
                if (registration is null)
                {
                    return;
                }

                var client = app.ApplicationServices.GetService<IConsulClient>();
                client.Agent.ServiceDeregister(registration.ID);
            });
        }

        private static AgentServiceRegistration CreateConsulAgentRegistration(this IConveyBuilder builder,
            ConsulOptions options)
        {
            var enabled = options.Enabled;
            var consulEnabled = Environment.GetEnvironmentVariable("CONSUL_ENABLED")?.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(consulEnabled))
            {
                enabled = consulEnabled == "true" || consulEnabled == "1";
            }

            if (!enabled)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(options.Address))
            {
                throw new ArgumentException("Consul address can not be empty.",
                    nameof(options.PingEndpoint));
            }

            var uniqueId = string.Empty;
            using (var serviceProvider = builder.Services.BuildServiceProvider())
            {
                uniqueId = serviceProvider.GetRequiredService<IServiceId>().Id;
            }

            var pingInterval = options.PingInterval <= 0 ? 5 : options.PingInterval;
            var removeAfterInterval = options.RemoveAfterInterval <= 0 ? 10 : options.RemoveAfterInterval;

            var registration = new AgentServiceRegistration
            {
                Name = options.Service,
                ID = $"{options.Service}:{uniqueId}",
                Address = options.Address,
                Port = options.Port
            };

            if (!options.PingEnabled)
            {
                return registration;
            }


            var scheme = options.Address.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)
                ? string.Empty
                : "http://";
            var check = new AgentServiceCheck
            {
                Interval = TimeSpan.FromSeconds(pingInterval),
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(removeAfterInterval),
                HTTP = $"{scheme}{options.Address}{(options.Port > 0 ? $":{options.Port}" : string.Empty)}/" +
                       $"{options.PingEndpoint}"
            };
            registration.Checks = new[] {check};

            return registration;
        }
    }
}