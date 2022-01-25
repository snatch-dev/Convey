namespace Convey.MessageBrokers.AzureServiceBus.Options;

public class MessageOptions
{
    public MessageOptions(Type messageType)
    {
        MessageType = messageType;
    }

    public Type MessageType { get; }
}