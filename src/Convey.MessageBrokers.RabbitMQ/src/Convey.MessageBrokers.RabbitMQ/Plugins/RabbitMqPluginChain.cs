using System;

namespace Convey.MessageBrokers.RabbitMQ.Plugins;

internal sealed class RabbitMqPluginChain
{
    public Type PluginType { get; set; }
}