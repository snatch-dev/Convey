using System;

namespace Convey.MessageBrokers.RabbitMQ;

public class RabbitMqMessageProcessingTimeoutException : Exception
{
    public string MessageId { get; }
    public string CorrelationId { get; }

    public RabbitMqMessageProcessingTimeoutException(string messageId, string correlationId)
        : base($"There was a timeout error when handling the message with ID: '{messageId}', correlation ID: '{correlationId}'.")
    {
        MessageId = messageId;
        CorrelationId = correlationId;
    }
}