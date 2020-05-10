using System;
using System.Text;
using System.Threading.Tasks;
using Convey.MessageBrokers.RabbitMQ;
using Jaeger;
using OpenTracing;
using OpenTracing.Tag;
using RabbitMQ.Client.Events;

namespace Convey.Tracing.Jaeger.RabbitMQ.Plugins
{
    internal sealed class JaegerPlugin : RabbitMqPlugin
    {
        private readonly ITracer _tracer;
        private readonly string _spanContextHeader;

        public JaegerPlugin(ITracer tracer, RabbitMqOptions options)
        {
            _tracer = tracer;
            _spanContextHeader = options.GetSpanContextHeader();
        }

        public override async Task HandleAsync(object message, object correlationContext,
            BasicDeliverEventArgs args)
        {
            var messageName = message.GetType().Name.Underscore();
            var messageId = args.BasicProperties.MessageId;
            var spanContext = string.Empty;
            if (args.BasicProperties.Headers.TryGetValue(_spanContextHeader, out var spanContextHeader) &&
                spanContextHeader is byte[] spanContextBytes)
            {
                spanContext = Encoding.UTF8.GetString(spanContextBytes);
            }

            using var scope = BuildScope(messageName, spanContext);
            var span = scope.Span;
            span.Log($"Started processing a message: '{messageName}' [id: '{messageId}'].");
            try
            {
                await Next(message, correlationContext, args);
            }
            catch (Exception ex)
            {
                span.SetTag(Tags.Error, true);
                span.Log(ex.Message);
            }

            span.Log($"Finished processing a message: '{messageName}' [id: '{messageId}'].");
        }
        
        private IScope BuildScope(string messageName, string serializedSpanContext)
        {
            var spanBuilder = _tracer
                .BuildSpan($"processing-{messageName}")
                .WithTag("message-type", messageName);

            if (string.IsNullOrEmpty(serializedSpanContext))
            {
                return spanBuilder.StartActive(true);
            }

            var spanContext = SpanContext.ContextFromString(serializedSpanContext);

            return spanBuilder
                .AddReference(References.FollowsFrom, spanContext)
                .StartActive(true);
        }
    }
}