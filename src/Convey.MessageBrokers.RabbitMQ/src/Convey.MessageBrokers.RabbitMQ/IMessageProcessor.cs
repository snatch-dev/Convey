using System.Threading.Tasks;

namespace Convey.MessageBrokers.RabbitMQ
{
    public interface IMessageProcessor
    {
        Task<bool> TryProcessAsync(string id);
        Task RemoveAsync(string id);
    }
}