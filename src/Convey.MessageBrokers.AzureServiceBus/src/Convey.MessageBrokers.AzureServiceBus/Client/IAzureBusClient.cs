using Azure.Messaging.ServiceBus;

namespace Convey.MessageBrokers.AzureServiceBus.Client;

public interface IAzureBusClient
{
    Task<ServiceBusProcessor> GetProcessorAsync(Type type);

    Task<ServiceBusSender> GetSenderAsync(Type type);
}