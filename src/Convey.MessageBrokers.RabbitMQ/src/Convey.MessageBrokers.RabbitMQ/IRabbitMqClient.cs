namespace Convey.MessageBrokers.RabbitMQ
{
    public interface IRabbitMqClient
    {
        void Send(object message, IConventions conventions, string messageId = null, string correlationId = null,
            object correlationContext = null);
    }
}