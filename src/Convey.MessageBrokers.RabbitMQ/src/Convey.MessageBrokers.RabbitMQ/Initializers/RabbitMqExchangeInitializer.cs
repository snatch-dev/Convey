using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Convey.Types;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Convey.MessageBrokers.RabbitMQ.Initializers
{
    public class RabbitMqExchangeInitializer : IInitializer
    {
        private const string DefaultType = "topic";
        private readonly IConnection _connection;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqExchangeInitializer> _logger;
        private readonly bool _loggerEnabled;

        public RabbitMqExchangeInitializer(IConnection connection, RabbitMqOptions options,
            ILogger<RabbitMqExchangeInitializer> logger)
        {
            _connection = connection;
            _options = options;
            _logger = logger;
            _loggerEnabled = _options.Logger?.Enabled == true;
        }

        public Task InitializeAsync()
        {
            var exchanges = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsDefined(typeof(MessageAttribute), false))
                .Select(t => t.GetCustomAttribute<MessageAttribute>().Exchange)
                .Distinct()
                .ToList();

            using var channel = _connection.CreateModel();
            if (_options.Exchange?.Declare == true)
            {
                Log(_options.Exchange.Name, _options.Exchange.Type);
                channel.ExchangeDeclare(_options.Exchange.Name, _options.Exchange.Type, _options.Exchange.Durable,
                    _options.Exchange.AutoDelete);

                if (_options.DeadLetter?.Declare is true)
                {
                    channel.ExchangeDeclare($"{_options.DeadLetter.Prefix}{_options.Exchange.Name}",
                        ExchangeType.Direct, _options.Exchange.Durable, _options.Exchange.AutoDelete);
                }
            }

            foreach (var exchange in exchanges)
            {
                if (exchange.Equals(_options.Exchange?.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                Log(exchange, DefaultType);
                channel.ExchangeDeclare(exchange, DefaultType, true);
            }

            channel.Close();

            return Task.CompletedTask;
        }

        private void Log(string exchange, string type)
        {
            if (!_loggerEnabled)
            {
                return;
            }

            _logger.LogInformation($"Declaring an exchange: '{exchange}', type: '{type}'.");
        }
    }
}