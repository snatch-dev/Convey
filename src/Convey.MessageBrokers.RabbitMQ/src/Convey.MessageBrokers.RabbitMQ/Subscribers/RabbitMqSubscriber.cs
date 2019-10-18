using System;
using System.Text;
using System.Threading.Tasks;
using Convey.MessageBrokers.RabbitMQ.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Convey.MessageBrokers.RabbitMQ.Subscribers
{
    internal sealed class RabbitMqSubscriber : IBusSubscriber
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBusPublisher _publisher;
        private readonly IConnection _connection;
        private readonly IRabbitMqSerializer _rabbitMqSerializer;
        private readonly IConventionsProvider _conventionsProvider;
        private readonly IContextProvider _contextProvider;
        private readonly ILogger _logger;
        private readonly IRabbitMqPluginsExecutor _pluginsExecutor;
        private readonly IExceptionToMessageMapper _exceptionToMessageMapper;
        private readonly int _retries;
        private readonly int _retryInterval;
        private readonly IModel _channel;
        private readonly bool _loggerEnabled;
        private readonly RabbitMqOptions _options;

        public RabbitMqSubscriber(IApplicationBuilder app)
        {
            _serviceProvider = app.ApplicationServices.GetRequiredService<IServiceProvider>();
            _connection = app.ApplicationServices.GetRequiredService<IConnection>();
            _channel = _connection.CreateModel();
            _publisher = app.ApplicationServices.GetRequiredService<IBusPublisher>();
            _rabbitMqSerializer = app.ApplicationServices.GetRequiredService<IRabbitMqSerializer>();;
            _conventionsProvider = app.ApplicationServices.GetRequiredService<IConventionsProvider>();;
            _contextProvider = app.ApplicationServices.GetRequiredService<IContextProvider>();
            _logger = app.ApplicationServices.GetService<ILogger<RabbitMqSubscriber>>();
            _exceptionToMessageMapper = _serviceProvider.GetService<IExceptionToMessageMapper>() ??
                                        new EmptyExceptionToMessageMapper();
            _pluginsExecutor = _serviceProvider.GetService<IRabbitMqPluginsExecutor>();
            _options = _serviceProvider.GetService<RabbitMqOptions>();
            _loggerEnabled = _options.Logger?.Enabled ?? false;
            _retries = _options.Retries >= 0 ? _options.Retries : 3;
            _retryInterval = _options.RetryInterval > 0 ? _options.RetryInterval : 2;
        }

        public IBusSubscriber Subscribe<T>(Func<IServiceProvider, T, object, Task> handle)
            where T : class
        {
            var conventions = _conventionsProvider.Get<T>();
            var declare = _options.Queue?.Declare ?? true;
            var durable = _options.Queue?.Durable ?? true;
            var exclusive = _options.Queue?.Exclusive ?? false;
            var autoDelete = _options.Queue?.AutoDelete ?? false;
            var info = string.Empty;
            
            if (_loggerEnabled)
            {
                info = $"[queue: '{conventions.Queue}', routing key: '{conventions.RoutingKey}', " +
                       $"exchange: '{conventions.Exchange}']";

                if (declare)
                {
                    _logger.LogInformation($"Declaring a queue: '{conventions.Queue}' with routing key: " +
                                           $"'{conventions.RoutingKey}' for exchange: '{conventions.Exchange}'.");
                    _channel.QueueDeclare(conventions.Queue, durable, exclusive, autoDelete);
                }
            }
            
            _channel.QueueBind(conventions.Queue, conventions.Exchange, conventions.RoutingKey);
            _channel.BasicQos(0, 1, false);
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.Received += async (model, args) =>
            {
                try
                {
                    var messageId = args.BasicProperties.MessageId;
                    var correlationId = args.BasicProperties.CorrelationId;
                    var timestamp = args.BasicProperties.Timestamp.UnixTime;
                    if (_loggerEnabled)
                    {
                        _logger.LogInformation($"Received a message with id: '{messageId}', " +
                                               $"correlation id: '{correlationId}', timestamp: {timestamp} {info}.");
                    }

                    var body = args.Body;
                    var payload = Encoding.UTF8.GetString(body);
                    var messagePropertiesAccessor = _serviceProvider.GetService<IMessagePropertiesAccessor>();
                    messagePropertiesAccessor.MessageProperties = new MessageProperties
                    {
                        MessageId = messageId,
                        CorrelationId = correlationId,
                        Timestamp = timestamp
                    };
                    var correlationContextAccessor = _serviceProvider.GetService<ICorrelationContextAccessor>();
                    var correlationContext = _contextProvider.Get(args.BasicProperties.Headers);
                    correlationContextAccessor.CorrelationContext = correlationContext;
                    var message = _rabbitMqSerializer.Deserialize<T>(payload);

                    Task Next(object m, object ctx, BasicDeliverEventArgs a)
                        => TryHandleAsync((T)m, messageId, correlationId, ctx, a, handle);

                    await _pluginsExecutor.ExecuteAsync(Next, message, correlationContext, args);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            };
            _channel.BasicConsume(conventions.Queue, false, consumer);

            return this;
        }

        private async Task TryHandleAsync<TMessage>(TMessage message, string messageId, string correlationId, 
            object correlationContext, BasicDeliverEventArgs args,Func<IServiceProvider, TMessage, object, Task> handle)
        {
            var currentRetry = 0;
            var messageName = message.GetType().Name;
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retries, i => TimeSpan.FromSeconds(_retryInterval));

            var exception = await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var retryMessage = string.Empty;
                    if (_loggerEnabled)
                    {
                        retryMessage = currentRetry == 0
                            ? string.Empty
                            : $"Retry: {currentRetry}'.";
                        var preLogMessage = $"Handling a message: '{messageName}' [id: '{messageId}'] " +
                                            $"with correlation id: '{correlationId}'. {retryMessage}";
                        _logger.LogInformation(preLogMessage);
                    }

                    await handle(_serviceProvider, message, correlationContext);

                    if (_loggerEnabled)
                    {
                        var postLogMessage = $"Handled a message: '{messageName}' [id: '{messageId}'] " +
                                             $"with correlation id: '{correlationId}'. {retryMessage}";
                        _logger.LogInformation(postLogMessage);
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    currentRetry++;
                    _logger.LogError(ex, ex.Message);
                    var rejectedEvent = _exceptionToMessageMapper.Map(ex, message);
                    if (rejectedEvent is null)
                    {
                        throw new Exception($"Unable to handle a message: '{messageName}' [id: '{messageId}'] " +
                                            $"with correlation id: '{correlationId}', " +
                                            $"retry {currentRetry - 1}/{_retries}...", ex);
                    }

                    var rejectedEventName = rejectedEvent.GetType().Name;
                    await _publisher.PublishAsync(rejectedEvent, correlationId, null, correlationContext);
                    if (_loggerEnabled)
                    {
                        _logger.LogWarning($"Published a rejected event: '{rejectedEventName}' " +
                                           $"for the message: '{messageName}' [id: '{messageId}'] with correlation id: '{correlationId}'.");
                    }

                    return new Exception($"Handling a message: '{messageName}' [id: '{messageId}'] " +
                                         $"with correlation id: '{correlationId}' failed and rejected event: " +
                                         $"'{rejectedEventName}' was published.", ex);
                }
            });
            
            if (exception is null)
            {
                _channel.BasicAck(args.DeliveryTag, false);
                return;
            }

            throw exception;
        }

        private class EmptyExceptionToMessageMapper : IExceptionToMessageMapper
        {
            public object Map(Exception exception, object message) => null;
        }
    }
}