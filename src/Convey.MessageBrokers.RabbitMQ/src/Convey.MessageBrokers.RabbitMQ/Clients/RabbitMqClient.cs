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

        public RabbitMqClient(IConnection connection, IContextProvider contextProvider, IRabbitMqSerializer serializer,
            RabbitMqOptions options, ILogger<RabbitMqClient> logger)
        {
            _contextProvider = contextProvider;
            _serializer = serializer;
            _logger = logger;
            _contextEnabled = options.Context?.Enabled == true;
            _channel = connection.CreateModel();
            _loggerEnabled = options.Logger?.Enabled ?? false;
        }

        public void Send(object message, IConventions conventions, string messageId = null, string correlationId = null,
            object correlationContext = null)
        {
            var json = _serializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
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
                IncludeCorrelationContext(correlationContext, properties);
            }

            if (_loggerEnabled)
            {
                _logger.LogInformation($"Publishing a message with routing key: '{conventions.RoutingKey}' " +
                                       $"to exchange: '{conventions.Exchange}' " +
                                       $"[id: '{properties.MessageId}', correlation id: '{properties.CorrelationId}']");
            }

            _channel.BasicPublish(conventions.Exchange, conventions.RoutingKey, properties, body);
        }

        private void IncludeCorrelationContext(object context, IBasicProperties properties)
        {
            if (!(context is null))
            {
                properties.Headers.Add(_contextProvider.HeaderName, _serializer.Serialize(context));
                return;
            }

            properties.Headers.Add(_contextProvider.HeaderName, EmptyContext);
        }
    }
}