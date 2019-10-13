using System;
using Convey.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Convey.MessageBrokers.Outbox.Outbox
{
    internal sealed class OutboxMessage : IIdentifiable<Guid>
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
        public string Type { get; set; }
        [BsonIgnore]
        public object Message { get; set; }
        public string SerializedMessage { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}