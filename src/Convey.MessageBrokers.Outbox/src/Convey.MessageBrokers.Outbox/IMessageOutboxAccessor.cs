using System.Collections.Generic;
using System.Threading.Tasks;
using Convey.MessageBrokers.Outbox.Messages;

namespace Convey.MessageBrokers.Outbox
{
    public interface IMessageOutboxAccessor
    {
        Task<IReadOnlyList<OutboxMessage>> GetUnsentAsync();
        Task ProcessAsync(OutboxMessage message);
        Task ProcessAsync(IEnumerable<OutboxMessage> outboxMessages);
    }
}