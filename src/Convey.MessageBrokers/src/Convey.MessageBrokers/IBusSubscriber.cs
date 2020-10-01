using System;
using System.Threading.Tasks;

namespace Convey.MessageBrokers
{
    public interface IBusSubscriber : IDisposable
    {
        IBusSubscriber Subscribe<T>(Func<IServiceProvider, T, object, Task> handle) where T : class;
    }
}
