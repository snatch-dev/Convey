using System;
using System.Linq;
using System.Threading.Tasks;
using Convey.MessageBrokers.RabbitMQ.Clients;
using Convey.MessageBrokers.RabbitMQ.Contexts;
using Convey.MessageBrokers.RabbitMQ.Conventions;
using Convey.MessageBrokers.RabbitMQ.Initializers;
using Convey.MessageBrokers.RabbitMQ.Plugins;
using Convey.MessageBrokers.RabbitMQ.Processors;
using Convey.MessageBrokers.RabbitMQ.Publishers;
using Convey.MessageBrokers.RabbitMQ.Serializers;
using Convey.MessageBrokers.RabbitMQ.Subscribers;
using Convey.Types;
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
            Func<IRabbitMqPluginsRegistry, IRabbitMqPluginsRegistry> plugins = null)
        {
            var options = builder.GetOptions<RabbitMqOptions>(sectionName);
            builder.Services.AddSingleton(options);
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
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
            builder.AddInitializer<RabbitMqExchangeInitializer>();
            
            var pluginsRegistry = new RabbitMqPluginsRegistry();
            builder.Services.AddSingleton<IRabbitMqPluginsRegistryAccessor>(pluginsRegistry);
            builder.Services.AddSingleton<IRabbitMqPluginsExecutor, RabbitMqPluginsExecutor>();
            plugins?.Invoke(pluginsRegistry);
            
            if (options.MessageProcessor?.Enabled == true)
            {
                pluginsRegistry.Add<UniqueMessagesPlugin>();
                switch (options.MessageProcessor.Type?.ToLowerInvariant())
                {
                    default:
                        builder.Services.AddMemoryCache();
                        builder.Services.AddSingleton<IMessageProcessor, InMemoryMessageProcessor>();
                        break;
                }
            }
            else
            {
                builder.Services.AddSingleton<IMessageProcessor, EmptyMessageProcessor>();
            }

            builder.Services.AddSingleton(sp =>
            {
                var connectionFactory = new ConnectionFactory
                {
                    HostName = options.HostNames?.FirstOrDefault(),
                    Port = options.Port,
                    VirtualHost = options.VirtualHost,
                    UserName = options.Username,
                    Password = options.Password,
                    RequestedConnectionTimeout = options.RequestedConnectionTimeout,
                    SocketReadTimeout = options.SocketReadTimeout,
                    SocketWriteTimeout = options.SocketWriteTimeout,
                    RequestedChannelMax = options.RequestedChannelMax,
                    RequestedFrameMax = options.RequestedFrameMax,
                    RequestedHeartbeat = options.RequestedHeartbeat,
                    UseBackgroundThreadsForIO = options.UseBackgroundThreadsForIO,
                    DispatchConsumersAsync = true,
                    Ssl = options.Ssl is null
                        ? new SslOption()
                        : new SslOption(options.Ssl.ServerName, options.Ssl.CertificatePath, options.Ssl.Enabled)
                };

                var connection = connectionFactory.CreateConnection(options.ConnectionName);
                if (options.Exchange is null || !options.Exchange.Declare)
                {
                    return connection;
                }

                using (var channel = connection.CreateModel())
                {
                    if (options.Logger?.Enabled == true)
                    {
                        var logger = sp.GetService<ILogger<IConnection>>();
                        logger.LogInformation($"Declaring an exchange: '{options.Exchange.Name}', type: '{options.Exchange.Type}'.");
                    }

                    channel.ExchangeDeclare(options.Exchange.Name, options.Exchange.Type, options.Exchange.Durable,
                        options.Exchange.AutoDelete);
                    channel.Close();
                }

                return connection;
            });

            ((IRabbitMqPluginsRegistryAccessor)pluginsRegistry).Get().ToList().ForEach(p => 
                builder.Services.AddTransient(p.PluginType));
            
            return builder;
        }

        public static IConveyBuilder AddExceptionToMessageMapper<T>(this IConveyBuilder builder)
            where T : class, IExceptionToMessageMapper
        {
            builder.Services.AddSingleton<IExceptionToMessageMapper, T>();

            return builder;
        }
        
        public static IBusSubscriber UseRabbitMq(this IApplicationBuilder app) => new RabbitMqSubscriber(app);

        private class EmptyMessageProcessor : IMessageProcessor
        {
            public Task<bool> TryProcessAsync(string id) => Task.FromResult(true);

            public Task RemoveAsync(string id) => Task.CompletedTask;
        }
    }
}