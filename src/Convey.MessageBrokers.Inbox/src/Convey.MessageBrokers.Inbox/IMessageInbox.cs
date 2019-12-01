using System;
using System.Threading.Tasks;

namespace Convey.MessageBrokers.Inbox
{
    public interface IMessageInbox
    {
        Task<bool> TryProcessAsync(string messageId, Func<Task> handle);
    }
}