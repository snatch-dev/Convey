using Convey.MessageBrokers.RabbitMQ;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Convey.Tracing.Jaeger.RabbitMQ.Plugins;

internal sealed class JaegerPlugin : RabbitMqPlugin
{
    private static readonly ActivitySource _activitySource = new ActivitySource("Jaeger.RabbitMqPlugin");

    private readonly ILogger<JaegerPlugin> _logger;
    private readonly string _spanContextHeader;

    public JaegerPlugin(ILogger<JaegerPlugin> logger, RabbitMqOptions options)
    {
        _logger = logger;
        _spanContextHeader = options.GetSpanContextHeader();
    }

    public override async Task HandleAsync(object message, object correlationContext, BasicDeliverEventArgs args)
    {
        var messageName = message.GetType().Name.Underscore();

        using (var activity = _activitySource.StartActivity($"processing-{messageName}"))
        {
            var messageId = args.BasicProperties.MessageId;
            var spanContext = string.Empty;

            if (args.BasicProperties.Headers is { } &&
                args.BasicProperties.Headers.TryGetValue(_spanContextHeader, out var spanContextHeader) &&
                spanContextHeader is byte[] spanContextBytes)
            {
                spanContext = Encoding.UTF8.GetString(spanContextBytes);
            }

            activity?.SetTag("MessageType", messageName);
            activity?.SetTag("SpanContext", spanContext);

            _logger?.LogInformation("Started processing a message: '{MessageName}' [id: '{MessageId}'].", messageName, messageId);

            try
            {
                await Next(message, correlationContext, args);

                activity?.SetStatus(ActivityStatusCode.Ok);
                activity?.SetTag("Success", "true");
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("Error", "true");

                _logger?.LogError(ex, ex.Message);
            }

            _logger?.LogInformation("Finished processing a message: '{MessageName}' [id: '{MessageId}'].", messageName, messageId);
        }
    }
}