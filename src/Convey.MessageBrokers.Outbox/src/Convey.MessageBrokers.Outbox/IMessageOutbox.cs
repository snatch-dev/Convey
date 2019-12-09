using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convey.MessageBrokers.Outbox
{
    public interface IMessageOutbox
    {
        bool Enabled { get; }

        Task SendAsync<T>(T message, string messageId = null, string correlationId = null, string spanContext = null,
            object messageContext = null, IDictionary<string, object> headers = null, string userId = null)
            where T : class;
    }
}