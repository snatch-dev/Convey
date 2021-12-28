using RabbitMQ.Client;

namespace Convey.MessageBrokers.RabbitMQ;

public sealed class ProducerConnection
{
    public IConnection Connection { get; }

    public ProducerConnection(IConnection connection)
    {
        Connection = connection;
    }
}