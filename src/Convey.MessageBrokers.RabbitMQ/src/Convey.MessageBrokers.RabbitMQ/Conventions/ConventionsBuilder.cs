using System;
using System.Linq;
using System.Reflection;

namespace Convey.MessageBrokers.RabbitMQ.Conventions;

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
        var routingKey = type.Name;
        if (_options.Conventions?.MessageAttribute?.IgnoreRoutingKey is true)
        {
            return WithCasing(routingKey);;
        }
            
        var attribute = GeAttribute(type);
        routingKey = string.IsNullOrWhiteSpace(attribute?.RoutingKey) ? routingKey : attribute.RoutingKey;

        return WithCasing(routingKey);
    }

    public string GetExchange(Type type)
    {
        var exchange = string.IsNullOrWhiteSpace(_options.Exchange?.Name)
            ? type.Assembly.GetName().Name
            : _options.Exchange.Name;
        if (_options.Conventions?.MessageAttribute?.IgnoreExchange is true)
        {
            return WithCasing(exchange);;
        }

        var attribute = GeAttribute(type);
        exchange = string.IsNullOrWhiteSpace(attribute?.Exchange) ? exchange : attribute.Exchange;

        return WithCasing(exchange);
    }

    public string GetQueue(Type type)
    {
        var attribute = GeAttribute(type);
        var ignoreQueue = _options.Conventions?.MessageAttribute?.IgnoreQueue;
        if (ignoreQueue is null or false && !string.IsNullOrWhiteSpace(attribute?.Queue))
        {
            return WithCasing(attribute.Queue);
        }

        var ignoreExchange = _options.Conventions?.MessageAttribute?.IgnoreExchange;
        var assembly = type.Assembly.GetName().Name;
        var message = type.Name;
        var exchange = ignoreExchange is true
            ? _options.Exchange?.Name
            : string.IsNullOrWhiteSpace(attribute?.Exchange)
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