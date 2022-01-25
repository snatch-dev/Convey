namespace Convey.MessageBrokers.AzureServiceBus.Conventions;

public interface IConventionsBuilder
{
    public string GetTopicName(Type type);

    public string GetSubscriberName(Type type);

    public string? GetSubscriptionFilter(Type type);

    public string? GetToValue(Type type) => 
        null;

    public string? GetSubjectValue(Type type) =>
        type.Name;
    
    public (string? To, string? Subject) GetPublishConventions(Type type) =>
        (GetToValue(type), GetSubjectValue(type));

    public (string Topic, string Subscriber, string? SubscriptionFilter) GetSubscriptionConventions(Type type) =>
        (GetTopicName(type), GetSubscriberName(type), GetSubscriptionFilter(type));
}