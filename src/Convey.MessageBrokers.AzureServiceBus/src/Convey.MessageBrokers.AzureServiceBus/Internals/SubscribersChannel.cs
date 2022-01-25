using System.Threading.Channels;

namespace Convey.MessageBrokers.AzureServiceBus.Internals;

internal class SubscribersChannel : ISubscribersChannel
{
    private readonly Channel<object> _channel = Channel.CreateUnbounded<object>();

    public IAsyncEnumerable<object> ReadAsync(CancellationToken cancellationToken) => 
        _channel.Reader.ReadAllAsync(cancellationToken);

    public ValueTask WriteAsync(object sub, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(sub, cancellationToken);
}