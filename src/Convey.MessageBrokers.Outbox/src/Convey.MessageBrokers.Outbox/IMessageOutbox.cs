using System.Threading.Tasks;

namespace Convey.MessageBrokers.Outbox
{
    public interface IMessageOutbox
    {
        Task SendAsync<T>(T message);
    }
}