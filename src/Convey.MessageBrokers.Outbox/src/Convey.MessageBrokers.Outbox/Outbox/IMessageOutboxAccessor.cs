using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convey.MessageBrokers.Outbox.Outbox
{
    internal interface IMessageOutboxAccessor
    {
        Task<IReadOnlyList<OutboxMessage>> GetUnsentAsync();
        Task ProcessAsync(IEnumerable<OutboxMessage> outboxMessages);
    }
}