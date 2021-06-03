using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Convey.MessageBrokers.RabbitMQ.Internals
{
    internal sealed class RabbitMqHostedService : IHostedService
    {
        private readonly IConnection _consumerConnection;
        private readonly IConnection _producerConnection;

        public RabbitMqHostedService(ConsumerConnection consumerConnection, ProducerConnection producerConnection)
        {
            _consumerConnection = consumerConnection.Connection;
            _producerConnection = producerConnection.Connection;
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _consumerConnection.Close();
                _producerConnection.Close();
            }
            catch
            {
                // ignored
            }

            return Task.CompletedTask;
        }
    }
}