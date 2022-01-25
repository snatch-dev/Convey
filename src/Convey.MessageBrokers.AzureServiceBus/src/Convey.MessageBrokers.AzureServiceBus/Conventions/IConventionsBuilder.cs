namespace Convey.MessageBrokers.AzureServiceBus.Conventions;

public interface IConventionsBuilder
{
    public string GetTopicName(Type type);

    public string GetSubscriberName(Type type);

    public (string Topic, string Subscriber) GetConventions(Type type);
}