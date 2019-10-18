using System.Collections.Generic;

namespace Convey.MessageBrokers.RabbitMQ.Plugins
{
    internal interface IRabbitMqPluginsRegistryAccessor
    {
        LinkedList<RabbitMqPluginChain> Get();
    }
}