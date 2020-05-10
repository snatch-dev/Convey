using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Convey.MessageBrokers.RabbitMQ.Clients
{
    internal sealed class RabbitMqClient : IRabbitMqClient
    {
        private const string EmptyContext = "{}";
        private readonly IContextProvider _contextProvider;
        private readonly IRabbitMqSerializer _serializer;
        private readonly ILogger<RabbitMqClient> _logger;
        private readonly bool _contextEnabled;
        private readonly IModel _channel;
        private readonly bool _loggerEnabled;
        private readonly string _spanContextHeader;

        public RabbitMqClient(IConnection connection, IContextProvider contextProvider, IRabbitMqSerializer serializer,
            RabbitMqOptions options, ILogger<RabbitMqClient> logger)
        {
            _contextProvider = contextProvider;
            _serializer = serializer;
            _logger = logger;
            _contextEnabled = options.Context?.Enabled == true;
            _channel = connection.CreateModel();
            _loggerEnabled = options.Logger?.Enabled ?? false;
            _spanContextHeader = options.GetSpanContextHeader();
        }

        public void Send(object message, IConventions conventions, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null)
        {
            var payload = _serializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(payload);
            var properties = _channel.CreateBasicProperties();
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

            _channel.BasicPublish(conventions.Exchange, conventions.RoutingKey, properties, body);
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