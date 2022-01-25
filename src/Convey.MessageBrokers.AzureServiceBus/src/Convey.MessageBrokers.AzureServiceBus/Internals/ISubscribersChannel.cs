using Convey.MessageBrokers.AzureServiceBus.Subscribers;

namespace Convey.MessageBrokers.AzureServiceBus.Internals;

internal interface ISubscribersChannel
{
    IAsyncEnumerable<IMessageSubscriber> ReadAsync(CancellationToken cancellationToken = default);
    ValueTask WriteAsync(IMessageSubscriber sub, CancellationToken cancellationToken = default);
}