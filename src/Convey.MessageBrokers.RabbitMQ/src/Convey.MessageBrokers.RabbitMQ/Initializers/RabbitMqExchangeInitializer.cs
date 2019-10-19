using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Convey.Types;
using RabbitMQ.Client;

namespace Convey.MessageBrokers.RabbitMQ.Initializers
{
    public class RabbitMqExchangeInitializer : IInitializer
    {
        private readonly IConnection _connection;

        public RabbitMqExchangeInitializer(IConnection connection)
            => _connection = connection;

        public Task InitializeAsync()
        {
            var exchanges = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsDefined(typeof(MessageAttribute), false))
                .Select(t => t.GetCustomAttribute<MessageAttribute>().Exchange)
                .ToList();

            using (var channel = _connection.CreateModel())
            {
                foreach (var exchange in exchanges)
                {
                    channel.ExchangeDeclare(exchange, "topic", true);
                }
            }

            return Task.CompletedTask;
        }
    }
}