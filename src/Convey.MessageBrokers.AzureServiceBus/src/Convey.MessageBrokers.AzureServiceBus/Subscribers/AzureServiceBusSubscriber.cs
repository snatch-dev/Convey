using Convey.MessageBrokers.AzureServiceBus.Internals;

namespace Convey.MessageBrokers.AzureServiceBus.Subscribers;

internal sealed class AzureServiceBusSubscriber : IBusSubscriber
{
    private readonly ISubscribersChannel _subscribersChannel;

    public AzureServiceBusSubscriber(ISubscribersChannel subscribersChannel) =>
        _subscribersChannel = subscribersChannel;

    public IBusSubscriber Subscribe<T>(Func<IServiceProvider, T, object, Task> handle) where T : class
    {
        var type = typeof(T);
        var subscriber = new MessageSubscriber(
            type,
            (serviceProvider, message, context) => handle(serviceProvider, (T) message, context));
        
        _subscribersChannel.WriteAsync(subscriber);
        return this;
    }

    public void Dispose()
    {
    }
}