using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Convey.MessageBrokers.RabbitMQ.Clients
{
    internal sealed class RabbitMqClient : IRabbitMqClient
    {
        private readonly object _lockObject = new object();
        private const string EmptyContext = "{}";
        private readonly IConnection _connection;
        private readonly IContextProvider _contextProvider;
        private readonly IRabbitMqSerializer _serializer;
        private readonly ILogger<RabbitMqClient> _logger;
        private readonly bool _contextEnabled;
        private readonly bool _loggerEnabled;
        private readonly string _spanContextHeader;
        private readonly bool _persistMessages;
        private int _channelsCount;
        private readonly ConcurrentDictionary<int, IModel> _channels = new ConcurrentDictionary<int, IModel>();
        private int _maxChannels;

        public RabbitMqClient(IConnection connection, IContextProvider contextProvider, IRabbitMqSerializer serializer,
            RabbitMqOptions options, ILogger<RabbitMqClient> logger)
        {
            _connection = connection;
            _contextProvider = contextProvider;
            _serializer = serializer;
            _logger = logger;
            _contextEnabled = options.Context?.Enabled == true;
            _loggerEnabled = options.Logger?.Enabled ?? false;
            _spanContextHeader = options.GetSpanContextHeader();
            _persistMessages = options?.MessagesPersisted ?? false;
            _maxChannels = options.MaxProducerChannels <= 0 ? 1000 : options.MaxProducerChannels;
        }

        public void Send(object message, IConventions conventions, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            if (!_channels.TryGetValue(threadId, out var channel))
            {
                lock (_lockObject)
                {
                    if (_channelsCount >= _maxChannels)
                    {
                        throw new InvalidOperationException($"Cannot create RabbitMQ producer channel for thread: {threadId} " +
                                                            $"(reached the limit of {_maxChannels} channels). " +
                                                            "Modify `MaxProducerChannels` setting to allow more channels.");
                    }
                    
                    channel = _connection.CreateModel();
                    _channels.TryAdd(threadId, channel);
                    _logger.LogTrace($"Created a channel for thread: {threadId}, total channels: {_channelsCount}/{_maxChannels}");
                    _channelsCount++;
                }
            }
            else
            {
                _logger.LogTrace($"Reused a channel for thread: {threadId}, total channels: {_channelsCount}/{_maxChannels}");
            }
            
            var payload = _serializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(payload);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = _persistMessages;
            properties.MessageId = string.IsNullOrWhiteSpace(messageId)
                ? Guid.NewGuid().ToString("N")
                : messageId;
            properties.CorrelationId = string.IsNullOrWhiteSpace(correlationId)
                ? Guid.NewGuid().ToString("N")
                : correlationId;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>();

            if (_contextEnabled)
            {
                IncludeMessageContext(messageContext, properties);
            }

            if (!string.IsNullOrWhiteSpace(spanContext))
            {
                properties.Headers.Add(_spanContextHeader, spanContext);
            }

            if (headers is {})
            {
                foreach (var (key, value) in headers)
                {
                    if (string.IsNullOrWhiteSpace(key) || value is null)
                    {
                        continue;
                    }

                    properties.Headers.TryAdd(key, value);
                }
            }

            if (_loggerEnabled)
            {
                _logger.LogTrace($"Publishing a message with routing key: '{conventions.RoutingKey}' " +
                                 $"to exchange: '{conventions.Exchange}' " +
                                 $"[id: '{properties.MessageId}', correlation id: '{properties.CorrelationId}']");
            }

            channel.BasicPublish(conventions.Exchange, conventions.RoutingKey, properties, body);
        }

        private void IncludeMessageContext(object context, IBasicProperties properties)
        {
            if (context is {})
            {
                properties.Headers.Add(_contextProvider.HeaderName, _serializer.Serialize(context));
                return;
            }

            properties.Headers.Add(_contextProvider.HeaderName, EmptyContext);
        }
    }
}