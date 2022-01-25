namespace Convey.MessageBrokers.AzureServiceBus.Subscribers;

internal class MessageSubscriber : IMessageSubscriber
{
    public Type Type { get; }
    public Func<IServiceProvider, object, object, Task> Handle { get; }
    public MessageSubscriberAction Action { get; }

    public MessageSubscriber(
        Type type, 
        Func<IServiceProvider, object, object, Task> handle, 
        MessageSubscriberAction action = MessageSubscriberAction.Subscribe)
    {
        Type = type;
        Handle = handle;
        Action = action;
    }
}