using Convey.MessageBrokers.AzureServiceBus.Internals;

namespace Convey.MessageBrokers.AzureServiceBus.Subscribers;

internal sealed class AzureServiceBusSubscriber : IBusSubscriber
{
    private readonly ISubscribersChannel _subscribersChannel;

    public AzureServiceBusSubscriber(ISubscribersChannel subscribersChannel) => 
        _subscribersChannel = subscribersChannel;

    public IBusSubscriber Subscribe<T>(Func<IServiceProvider, T, object, Task> handle) where T : class
    {
        throw new NotImplementedException();
    }
    
    public void Dispose()
    {
    }
}