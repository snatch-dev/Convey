using Convey.MessageBrokers.RabbitMQ;
using Convey.Tracing.Jaeger.RabbitMQ.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.Tracing.Jaeger.RabbitMQ
{
    public static class Extensions
    {

        public static IConveyBuilder AddJaegerRabbitMqMiddleware(this IConveyBuilder builder)
        {
            builder.Services.AddTransient<IRabbitMqMiddleware, JaegerMiddleware>();

            return builder;
        }
    }
}