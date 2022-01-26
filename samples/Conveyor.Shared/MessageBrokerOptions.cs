namespace Conveyor.Shared;

public class MessageBrokerOptions
{
    public MessageBrokers Provider { get; set; } = MessageBrokers.RabbitMQ;
}
