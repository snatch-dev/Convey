using Convey.MessageBrokers.AzureServiceBus.Subscribers;

namespace Convey.MessageBrokers.AzureServiceBus.Registries;

internal interface ISubscribersRegistry
{
    ValueTask SubscribeAsync(IMessageSubscriber messageSubscriber);

    ValueTask UnSubscribeAsync(IMessageSubscriber messageSubscriber);
}