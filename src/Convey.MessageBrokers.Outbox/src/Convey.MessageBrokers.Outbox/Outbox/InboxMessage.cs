using System;
using Convey.Types;

namespace Convey.MessageBrokers.Outbox.Outbox
{
    internal sealed class InboxMessage : IIdentifiable<string>
    {
        public string Id { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}