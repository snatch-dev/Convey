using System.Collections.Generic;

namespace Convey.MessageBrokers.RabbitMQ.Plugins
{
    internal sealed class RabbitMqPluginsRegistry : IRabbitMqPluginsRegistry, IRabbitMqPluginsRegistryAccessor
    {
        private readonly LinkedList<RabbitMqPluginChain> _plugins;

        public RabbitMqPluginsRegistry()
            => _plugins = new LinkedList<RabbitMqPluginChain>();

        public IRabbitMqPluginsRegistry Add<TPlugin>() where TPlugin : class, IRabbitMqPlugin
        {
            _plugins.AddLast(new RabbitMqPluginChain { PluginType =  typeof(TPlugin)});
            return this;
        }

        LinkedList<RabbitMqPluginChain> IRabbitMqPluginsRegistryAccessor.Get()
            => _plugins;
    }
}