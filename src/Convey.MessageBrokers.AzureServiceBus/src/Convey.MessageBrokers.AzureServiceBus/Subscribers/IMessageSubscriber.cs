namespace Convey.MessageBrokers.AzureServiceBus.Subscribers;

internal interface IMessageSubscriber
{
    Type Type { get; }
    Func<IServiceProvider, object, object, Task> Handle { get; }
    MessageSubscriberAction Action { get; }
}