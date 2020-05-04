using System;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Convey.MessageBrokers.RabbitMQ.Clients;
using Convey.MessageBrokers.RabbitMQ.Contexts;
using Convey.MessageBrokers.RabbitMQ.Conventions;
using Convey.MessageBrokers.RabbitMQ.Initializers;
using Convey.MessageBrokers.RabbitMQ.Internals;
using Convey.MessageBrokers.RabbitMQ.Plugins;
using Convey.MessageBrokers.RabbitMQ.Publishers;
using Convey.MessageBrokers.RabbitMQ.Serializers;
using Convey.MessageBrokers.RabbitMQ.Subscribers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Convey.MessageBrokers.RabbitMQ
{
    public static class Extensions
    {
        private const string SectionName = "rabbitmq";
        private const string RegistryName = "messageBrokers.rabbitmq";

        public static IConveyBuilder AddRabbitMq(this IConveyBuilder builder, string sectionName = SectionName,
            Func<IRabbitMqPluginsRegistry, IRabbitMqPluginsRegistry> plugins = null,
            Action<ConnectionFactory> connectionFactoryConfigurator = null)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var options = builder.GetOptions<RabbitMqOptions>(sectionName);
            builder.Services.AddSingleton(options);
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            if (options.HostNames is null || !options.HostNames.Any())
            {
                throw new ArgumentException("RabbitMQ hostnames are not specified.", nameof(options.HostNames));
            }


            ILogger<IRabbitMqClient> logger;
            using (var serviceProvider = builder.Services.BuildServiceProvider())
            {
                logger = serviceProvider.GetService<ILogger<IRabbitMqClient>>();
            }

            builder.Services.AddSingleton<IContextProvider, ContextProvider>();
            builder.Services.AddSingleton<ICorrelationContextAccessor>(new CorrelationContextAccessor());
            builder.Services.AddSingleton<IMessagePropertiesAccessor>(new MessagePropertiesAccessor());
            builder.Services.AddSingleton<IConventionsBuilder, ConventionsBuilder>();
            builder.Services.AddSingleton<IConventionsProvider, ConventionsProvider>();
            builder.Services.AddSingleton<IConventionsRegistry, ConventionsRegistry>();
            builder.Services.AddSingleton<IRabbitMqSerializer, NewtonsoftJsonRabbitMqSerializer>();
            builder.Services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
            builder.Services.AddSingleton<IBusPublisher, RabbitMqPublisher>();
            builder.Services.AddSingleton<IBusSubscriber, RabbitMqSubscriber>();
            builder.Services.AddTransient<RabbitMqExchangeInitializer>();
            builder.Services.AddHostedService<RabbitMqHostedService>();
            builder.AddInitializer<RabbitMqExchangeInitializer>();

            var pluginsRegistry = new RabbitMqPluginsRegistry();
            builder.Services.AddSingleton<IRabbitMqPluginsRegistryAccessor>(pluginsRegistry);
            builder.Services.AddSingleton<IRabbitMqPluginsExecutor, RabbitMqPluginsExecutor>();
            plugins?.Invoke(pluginsRegistry);

            var connectionFactory = new ConnectionFactory
            {
                Port = options.Port,
                VirtualHost = options.VirtualHost,
                UserName = options.Username,
                Password = options.Password,
                RequestedHeartbeat = options.RequestedHeartbeat,
                RequestedConnectionTimeout = options.RequestedConnectionTimeout,
                SocketReadTimeout = options.SocketReadTimeout,
                SocketWriteTimeout = options.SocketWriteTimeout,
                RequestedChannelMax = options.RequestedChannelMax,
                RequestedFrameMax = options.RequestedFrameMax,
                UseBackgroundThreadsForIO = options.UseBackgroundThreadsForIO,
                DispatchConsumersAsync = true,
                ContinuationTimeout = options.ContinuationTimeout,
                HandshakeContinuationTimeout = options.HandshakeContinuationTimeout,
                NetworkRecoveryInterval = options.NetworkRecoveryInterval,
                Ssl = options.Ssl is null
                    ? new SslOption()
                    : new SslOption(options.Ssl.ServerName, options.Ssl.CertificatePath, options.Ssl.Enabled)
            };
            ConfigureSsl(connectionFactory, options, logger);
            connectionFactoryConfigurator?.Invoke(connectionFactory);

            logger.LogDebug($"Connecting to RabbitMQ: '{string.Join(", ", options.HostNames)}'...");
            var connection = connectionFactory.CreateConnection(options.HostNames.ToList(), options.ConnectionName);
            logger.LogDebug($"Connected to RabbitMQ: '{string.Join(", ", options.HostNames)}'.");
            builder.Services.AddSingleton(connection);

            ((IRabbitMqPluginsRegistryAccessor) pluginsRegistry).Get().ToList().ForEach(p =>
                builder.Services.AddTransient(p.PluginType));

            return builder;
        }

        private static void ConfigureSsl(ConnectionFactory connectionFactory, RabbitMqOptions options,
            ILogger<IRabbitMqClient> logger)
        {
            if (options.Ssl is null || string.IsNullOrWhiteSpace(options.Ssl.ServerName))
            {
                connectionFactory.Ssl = new SslOption();
                return;
            }

            connectionFactory.Ssl = new SslOption(options.Ssl.ServerName, options.Ssl.CertificatePath,
                options.Ssl.Enabled);

            logger.LogDebug($"RabbitMQ SSL is: {(options.Ssl.Enabled ? "enabled" : "disabled")}, " +
                            $"server: '{options.Ssl.ServerName}', client certificate: '{options.Ssl.CertificatePath}', " +
                            $"CA certificate: '{options.Ssl.CaCertificatePath}'.");

            if (string.IsNullOrWhiteSpace(options.Ssl.CaCertificatePath))
            {
                return;
            }

            connectionFactory.Ssl.CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;
                }

                if (chain is null)
                {
                    return false;
                }

                chain = new X509Chain();
                var certificate2 = new X509Certificate2(certificate);
                var signerCertificate2 = new X509Certificate2(options.Ssl.CaCertificatePath);
                chain.ChainPolicy.ExtraStore.Add(signerCertificate2);
                chain.Build(certificate2);
                var ignoredStatuses = Enumerable.Empty<X509ChainStatusFlags>();
                if (options.Ssl.X509IgnoredStatuses?.Any() is true)
                {
                    logger.LogDebug("Ignored X509 certificate chain statuses: " +
                                    $"{string.Join(", ", options.Ssl.X509IgnoredStatuses)}.");
                    ignoredStatuses = options.Ssl.X509IgnoredStatuses
                        .Select(s => Enum.Parse<X509ChainStatusFlags>(s, true));
                }

                var statuses = chain.ChainStatus.ToList();
                logger.LogDebug("Received X509 certificate chain statuses: " +
                                $"{string.Join(", ", statuses.Select(x => x.Status))}");

                var isValid = statuses.All(chainStatus => chainStatus.Status == X509ChainStatusFlags.NoError
                                                          || ignoredStatuses.Contains(chainStatus.Status));
                if (!isValid)
                {
                    logger.LogError(string.Join(Environment.NewLine,
                        statuses.Select(s => $"{s.Status} - {s.StatusInformation}")));
                }

                return isValid;
            };
        }

        public static IConveyBuilder AddExceptionToMessageMapper<T>(this IConveyBuilder builder)
            where T : class, IExceptionToMessageMapper
        {
            builder.Services.AddSingleton<IExceptionToMessageMapper, T>();

            return builder;
        }

        public static IBusSubscriber UseRabbitMq(this IApplicationBuilder app)
            => new RabbitMqSubscriber(app.ApplicationServices);
    }
}