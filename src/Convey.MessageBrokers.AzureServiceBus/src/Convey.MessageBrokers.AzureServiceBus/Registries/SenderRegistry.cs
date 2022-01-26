using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Convey.MessageBrokers.AzureServiceBus.Client;

namespace Convey.MessageBrokers.AzureServiceBus.Registries;

internal class SenderRegistry : ISenderRegistry
{
    private readonly IAzureBusClient _azureBusClient;
    private readonly ConcurrentDictionary<Type, ServiceBusSender> _messageSenderMap = new();

    public SenderRegistry(IAzureBusClient azureBusClient) => 
        _azureBusClient = azureBusClient;

    public async ValueTask<ServiceBusSender> GetSender<T>()
    {
        var messageType = typeof(T);
        
        if (_messageSenderMap.ContainsKey(messageType))
        {
            return _messageSenderMap[messageType];
        }

        var sender = await _azureBusClient.GetSenderAsync(messageType);
        _messageSenderMap[messageType] = sender;
        return sender;
    }
}