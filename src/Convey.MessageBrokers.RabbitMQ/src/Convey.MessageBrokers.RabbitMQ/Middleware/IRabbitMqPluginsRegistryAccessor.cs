using System.Collections.Generic;

namespace Convey.MessageBrokers.RabbitMQ.Middleware
{
    internal interface IRabbitMqPluginsRegistryAccessor
    {
        LinkedList<RabbitMqPluginChain> Get();
    }
}