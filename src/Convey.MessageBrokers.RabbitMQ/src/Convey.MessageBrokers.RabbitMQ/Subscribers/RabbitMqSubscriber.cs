using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Convey.MessageBrokers.RabbitMQ.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Convey.MessageBrokers.RabbitMQ.Subscribers
{
    internal sealed class RabbitMqSubscriber : IBusSubscriber
    {
        private static readonly ConcurrentDictionary<string, ChannelInfo> Channels = new ConcurrentDictionary<string, ChannelInfo>();
        private static readonly EmptyExceptionToMessageMapper ExceptionMapper = new EmptyExceptionToMessageMapper();
        private readonly IServiceProvider _serviceProvider;
        private readonly IBusPublisher _publisher;
        private readonly IRabbitMqSerializer _rabbitMqSerializer;
        private readonly IConventionsProvider _conventionsProvider;
        private readonly IContextProvider _contextProvider;
        private readonly ILogger _logger;
        private readonly IRabbitMqPluginsExecutor _pluginsExecutor;
        private readonly IExceptionToMessageMapper _exceptionToMessageMapper;
        private readonly int _retries;
        private readonly int _retryInterval;
        private readonly bool _loggerEnabled;
        private readonly RabbitMqOptions _options;
        private readonly RabbitMqOptions.QosOptions _qosOptions;
        private readonly IConnection _connection;
        private readonly bool _requeueFailedMessages;

        public RabbitMqSubscriber(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _connection = _serviceProvider.GetRequiredService<IConnection>();
            _publisher = _serviceProvider.GetRequiredService<IBusPublisher>();
            _rabbitMqSerializer = _serviceProvider.GetRequiredService<IRabbitMqSerializer>();
            _conventionsProvider = _serviceProvider.GetRequiredService<IConventionsProvider>();
            _contextProvider = _serviceProvider.GetRequiredService<IContextProvider>();
            _logger = _serviceProvider.GetRequiredService<ILogger<RabbitMqSubscriber>>();
            _exceptionToMessageMapper = _serviceProvider.GetService<IExceptionToMessageMapper>() ?? ExceptionMapper;
            _pluginsExecutor = _serviceProvider.GetRequiredService<IRabbitMqPluginsExecutor>();
            _options = _serviceProvider.GetRequiredService<RabbitMqOptions>();
            _loggerEnabled = _options.Logger?.Enabled ?? false;
            _retries = _options.Retries >= 0 ? _options.Retries : 3;
            _retryInterval = _options.RetryInterval > 0 ? _options.RetryInterval : 2;
            _qosOptions = _options.Qos ?? new RabbitMqOptions.QosOptions();
            _requeueFailedMessages = _options.RequeueFailedMessages;
            if (_qosOptions.PrefetchCount < 1)
            {
                _qosOptions.PrefetchCount = 1;
            }
        }

        public IBusSubscriber Subscribe<T>(Func<IServiceProvider, T, object, Task> handle)
            where T : class
        {
            var conventions = _conventionsProvider.Get<T>();
            var channelKey = $"{conventions.Exchange}:{conventions.Queue}:{conventions.RoutingKey}";
            if (Channels.ContainsKey(channelKey))
            {
                return this;
            }
            
            var channel = _connection.CreateModel();
            if (!Channels.TryAdd(channelKey, new ChannelInfo(channel, conventions)))
            {
                return this;
            }
            
            _logger.LogTrace($"Created a channel: {channel.ChannelNumber}");
            var declare = _options.Queue?.Declare ?? true;
            var durable = _options.Queue?.Durable ?? true;
            var exclusive = _options.Queue?.Exclusive ?? false;
            var autoDelete = _options.Queue?.AutoDelete ?? false;
            var info = string.Empty;
            if (_loggerEnabled)
            {
                info = $" [queue: '{conventions.Queue}', routing key: '{conventions.RoutingKey}', " +
                       $"exchange: '{conventions.Exchange}']";
            }

            var deadLetterEnabled = _options.DeadLetter?.Enabled is true;
            var deadLetterExchange = deadLetterEnabled?  $"{_options.DeadLetter.Prefix}{_options.Exchange.Name}": string.Empty;
            var deadLetterQueue = deadLetterEnabled ? $"{_options.DeadLetter.Prefix}{conventions.Queue}" : string.Empty;
            if (declare)
            {
                if (_loggerEnabled)
                {
                    _logger.LogInformation($"Declaring a queue: '{conventions.Queue}' with routing key: " +
                                           $"'{conventions.RoutingKey}' for an exchange: '{conventions.Exchange}'.");
                }

                var queueArguments = deadLetterEnabled
                    ? new Dictionary<string, object>()
                    {
                        {"x-dead-letter-exchange", deadLetterExchange},
                        {"x-dead-letter-routing-key", deadLetterQueue},
                    }
                    : new Dictionary<string, object>();
                channel.QueueDeclare(conventions.Queue, durable, exclusive, autoDelete, queueArguments);
            }

            channel.QueueBind(conventions.Queue, conventions.Exchange, conventions.RoutingKey);
            channel.BasicQos(_qosOptions.PrefetchSize, _qosOptions.PrefetchCount, _qosOptions.Global);
            if (_options.DeadLetter?.Enabled is true)
            {
                if (_options.DeadLetter.Declare)
                {
                    var ttl = _options.DeadLetter.Ttl <= 0 ? 86400 : _options.DeadLetter.Ttl;
                    _logger.LogInformation($"Declaring a dead letter queue: '{deadLetterQueue}' " +
                                           $"for an exchange: '{deadLetterExchange}', message TTL: {ttl} seconds.");
                    channel.QueueDeclare(deadLetterQueue, _options.DeadLetter.Durable, _options.DeadLetter.Exclusive,
                        _options.DeadLetter.AutoDelete, new Dictionary<string, object>
                        {
                            {"x-dead-letter-exchange", conventions.Exchange},
                            {"x-dead-letter-routing-key", conventions.Queue},
                            {"x-message-ttl", ttl},
                        });
                }
                
                channel.QueueBind(deadLetterQueue, deadLetterExchange, deadLetterQueue);
            }

            var consumer = new AsyncEventingBasicConsumer(channel);
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
                                               $"correlation id: '{correlationId}', timestamp: {timestamp}{info}.");
                    }

                    var payload = Encoding.UTF8.GetString(args.Body.Span);
                    var message = _rabbitMqSerializer.Deserialize<T>(payload);
                    var correlationContext = BuildCorrelationContext(args);

                    Task Next(object m, object ctx, BasicDeliverEventArgs a)
                        => TryHandleAsync(channel, (T) m, messageId, correlationId, ctx, a, handle);

                    await _pluginsExecutor.ExecuteAsync(Next, message, correlationContext, args);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    channel.BasicReject(args.DeliveryTag, false);
                    throw;
                }
            };

            channel.BasicConsume(conventions.Queue, false, consumer);

            return this;
        }

        private object BuildCorrelationContext(BasicDeliverEventArgs args)
        {
            using var scope = _serviceProvider.CreateScope();
            var messagePropertiesAccessor = scope.ServiceProvider.GetService<IMessagePropertiesAccessor>();
            messagePropertiesAccessor.MessageProperties = new MessageProperties
            {
                MessageId = args.BasicProperties.MessageId,
                CorrelationId = args.BasicProperties.CorrelationId,
                Timestamp = args.BasicProperties.Timestamp.UnixTime,
                Headers = args.BasicProperties.Headers
            };
            var correlationContextAccessor = scope.ServiceProvider.GetService<ICorrelationContextAccessor>();
            var correlationContext = _contextProvider.Get(args.BasicProperties.Headers);
            correlationContextAccessor.CorrelationContext = correlationContext;

            return correlationContext;
        }

        private async Task TryHandleAsync<TMessage>(IModel channel, TMessage message, string messageId, string correlationId,
            object messageContext, BasicDeliverEventArgs args, Func<IServiceProvider, TMessage, object, Task> handle)
        {
            var currentRetry = 0;
            var messageName = message.GetType().Name.Underscore();
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retries, i => TimeSpan.FromSeconds(_retryInterval));

            await retryPolicy.ExecuteAsync(async () =>
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

                    await handle(_serviceProvider, message, messageContext);
                    channel.BasicAck(args.DeliveryTag, false);

                    if (_loggerEnabled)
                    {
                        var postLogMessage = $"Handled a message: '{messageName}' [id: '{messageId}'] " +
                                             $"with correlation id: '{correlationId}'. {retryMessage}";
                        _logger.LogInformation(postLogMessage);
                    }
                }
                catch (Exception ex)
                {
                    currentRetry++;
                    _logger.LogError(ex, ex.Message);
                    var rejectedEvent = _exceptionToMessageMapper.Map(ex, message);
                    if (rejectedEvent is null)
                    {
                        var errorMessage = $"Unable to handle a message: '{messageName}' [id: '{messageId}'] " +
                                           $"with correlation id: '{correlationId}', " +
                                           $"retry {currentRetry - 1}/{_retries}...";

                        if (currentRetry > 1)
                        {
                            _logger.LogError(errorMessage);
                        }
                        
                        if (currentRetry - 1 < _retries)
                        {
                            throw new Exception(errorMessage, ex);
                        }

                        _logger.LogError($"Handling a message: '{messageName}' [id: '{messageId}'] " +
                                         $"with correlation id: '{correlationId}' failed.");
                        channel.BasicNack(args.DeliveryTag, false, _requeueFailedMessages);
                        return;
                    }

                    var rejectedEventName = rejectedEvent.GetType().Name.Underscore();
                    await _publisher.PublishAsync(rejectedEvent, correlationId: correlationId,
                        messageContext: messageContext);
                    if (_loggerEnabled)
                    {
                        _logger.LogWarning($"Published a rejected event: '{rejectedEventName}' " +
                                           $"for the message: '{messageName}' [id: '{messageId}'] with correlation id: '{correlationId}'.");
                    }

                    _logger.LogError($"Handling a message: '{messageName}' [id: '{messageId}'] " +
                                     $"with correlation id: '{correlationId}' failed and rejected event: " +
                                     $"'{rejectedEventName}' was published.", ex);

                    channel.BasicAck(args.DeliveryTag, false);
                }
            });
        }

        private class EmptyExceptionToMessageMapper : IExceptionToMessageMapper
        {
            public object Map(Exception exception, object message) => null;
        }

        public void Dispose()
        {
            foreach (var (key, channel) in Channels)
            {
                channel?.Dispose();
                Channels.TryRemove(key, out _);
            }
            
            _connection?.Dispose();
        }

        private class ChannelInfo : IDisposable
        {
            public IModel Channel { get; }
            public IConventions Conventions { get; }

            public ChannelInfo(IModel channel, IConventions conventions)
            {
                Channel = channel;
                Conventions = conventions;
            }

            public void Dispose()
            {
                Channel?.Dispose();
            }
        }
    }
}