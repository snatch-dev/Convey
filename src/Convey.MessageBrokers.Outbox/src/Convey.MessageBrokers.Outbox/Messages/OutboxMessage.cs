using System;
using System.Collections.Generic;
using Convey.Types;

namespace Convey.MessageBrokers.Outbox.Messages
{
    public sealed class OutboxMessage : IIdentifiable<string>
    {
        public string Id { get; set; }
        public string OriginatedMessageId { get; set; }
        public string CorrelationId { get; set; }
        public string SpanContext { get; set; }
        public Dictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
        public string MessageType { get; set; }
        public string MessageContextType { get; set; }
        public object Message { get; set; }
        public object MessageContext { get; set; }
        public string SerializedMessage { get; set; }
        public string SerializedMessageContext { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}