using System.Threading.Channels;
using Convey.MessageBrokers.AzureServiceBus.Subscribers;

namespace Convey.MessageBrokers.AzureServiceBus.Internals;

internal class SubscribersChannel : ISubscribersChannel
{
    private readonly Channel<IMessageSubscriber> _channel =
        Channel.CreateUnbounded<IMessageSubscriber>();

    public IAsyncEnumerable<IMessageSubscriber> ReadAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);

    public ValueTask WriteAsync(IMessageSubscriber sub, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(sub, cancellationToken);
}