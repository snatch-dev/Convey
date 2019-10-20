using Convey.MessageBrokers.RabbitMQ;
using Convey.Tracing.Jaeger.RabbitMQ.Plugins;

namespace Convey.Tracing.Jaeger.RabbitMQ
{
    public static class Extensions
    {
        public static IRabbitMqPluginsRegistry AddJaegerRabbitMqPlugin(this IRabbitMqPluginsRegistry registry)
        {
            registry.Add<JaegerPlugin>();
            return registry;
        }
    }
}