using Azure.Messaging.ServiceBus;

namespace Convey.MessageBrokers.AzureServiceBus.Registries;

public interface ISenderRegistry
{
    ValueTask<ServiceBusSender> GetSender<T>();
}