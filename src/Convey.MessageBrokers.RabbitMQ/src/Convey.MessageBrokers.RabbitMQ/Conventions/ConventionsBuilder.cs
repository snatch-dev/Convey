using System;
using System.Linq;
using System.Reflection;

namespace Convey.MessageBrokers.RabbitMQ.Conventions
{
    public class ConventionsBuilder : IConventionsBuilder
    {
        private readonly RabbitMqOptions _options;
        private readonly bool _snakeCase;
        private readonly string _queueTemplate;

        public ConventionsBuilder(RabbitMqOptions options)
        {
            _options = options;
            _queueTemplate = string.IsNullOrWhiteSpace(_options.Queue?.Template)
                ? "{{assembly}}/{{exchange}}.{{message}}"
                : options.Queue.Template;
            _snakeCase = options.ConventionsCasing?.Equals("snakeCase",
                StringComparison.InvariantCultureIgnoreCase) == true;
        }

        public string GetRoutingKey(Type type)
        {
            var attribute = GeAttribute(type);
            var routingKey = string.IsNullOrWhiteSpace(attribute?.RoutingKey) ? type.Name : attribute.RoutingKey;

            return WithCasing(routingKey);
        }

        public string GetExchange(Type type)
        {
            var attribute = GeAttribute(type);
            var exchange = string.IsNullOrWhiteSpace(attribute?.Exchange)
                ? string.IsNullOrWhiteSpace(_options.Exchange?.Name) ? type.Assembly.GetName().Name :
                _options.Exchange.Name
                : attribute.Exchange;

            return WithCasing(exchange);
        }

        public string GetQueue(Type type)
        {
            var attribute = GeAttribute(type);
            if (!string.IsNullOrWhiteSpace(attribute?.Queue))
            {
                return WithCasing(attribute.Queue);
            }

            var assembly = type.Assembly.GetName().Name;
            var message = type.Name;
            var exchange = string.IsNullOrWhiteSpace(attribute?.Exchange)
                ? _options.Exchange?.Name
                : attribute.Exchange;

            var queue = _queueTemplate.Replace("{{assembly}}", assembly)
                .Replace("{{exchange}}", exchange)
                .Replace("{{message}}", message);

            return WithCasing(queue);
        }

        private string WithCasing(string value) => _snakeCase ? SnakeCase(value) : value;

        private static string SnakeCase(string value)
            => string.Concat(value.Select((x, i) =>
                    i > 0 && value[i - 1] != '.' && value[i - 1] != '/' && char.IsUpper(x) ? "_" + x : x.ToString()))
                .ToLowerInvariant();

        private static MessageAttribute GeAttribute(MemberInfo type) => type.GetCustomAttribute<MessageAttribute>();
    }
}