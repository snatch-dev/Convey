using System.Threading.Tasks;

namespace Convey.MessageBrokers.RawRabbit
{
    public interface IMessageProcessor
    {
        Task<bool> TryProcessAsync(string id);
        Task RemoveAsync(string id);
    }
}