namespace Convey.MessageBrokers.AzureServiceBus.Internals;

internal interface ISubscribersChannel
{
    IAsyncEnumerable<object> ReadAsync(CancellationToken cancellationToken = default);
    ValueTask WriteAsync(object sub, CancellationToken cancellationToken = default);
}