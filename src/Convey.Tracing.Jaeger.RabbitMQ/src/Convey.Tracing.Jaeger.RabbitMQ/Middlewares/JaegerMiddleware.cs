using System;
using System.Text;
using System.Threading.Tasks;
using Convey.MessageBrokers;
using Convey.MessageBrokers.RabbitMQ;
using Jaeger;
using OpenTracing;
using OpenTracing.Tag;
using RabbitMQ.Client.Events;

namespace Convey.Tracing.Jaeger.RabbitMQ.Middlewares
{
    internal sealed class JaegerMiddleware : IRabbitMqMiddleware
    {
        private readonly ITracer _tracer;

        public JaegerMiddleware(ITracer tracer) => _tracer = tracer;

        public async Task HandleAsync(Func<Task> next, object message, object correlationContext,
            BasicDeliverEventArgs args)
        {
            var messageName = message.GetType().Name;
            var messageId = args.BasicProperties.MessageId;
            var spanContext = string.Empty;
            if (args.BasicProperties.Headers.TryGetValue("span_context", out var spanContextHeader) &&
                spanContextHeader is byte[] spanContextBytes)
            {
                spanContext = Encoding.UTF8.GetString(spanContextBytes);
            }

            using (var scope = BuildScope(messageName, spanContext))
            {
                var span = scope.Span;
                span.Log($"Started processing: {messageName} [id: {messageId}]");
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    span.SetTag(Tags.Error, true);
                    span.Log(ex.Message);
                }

                span.Log($"Finished processing: {messageName} [id: {messageId}]");
            }
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