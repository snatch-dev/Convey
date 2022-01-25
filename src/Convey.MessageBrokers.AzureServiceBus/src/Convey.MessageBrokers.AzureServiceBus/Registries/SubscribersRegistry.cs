using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Convey.MessageBrokers.AzureServiceBus.Client;
using Convey.MessageBrokers.AzureServiceBus.Conventions;
using Convey.MessageBrokers.AzureServiceBus.Subscribers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Convey.MessageBrokers.AzureServiceBus.Registries;

internal class SubscribersRegistry : ISubscribersRegistry
{
    private readonly IAzureBusClient _azureBusClient;
    private readonly ConcurrentDictionary<Type, ServiceBusProcessor> _messageProcessorsMap = new();

    public SubscribersRegistry(IAzureBusClient azureBusClient)
    {
        _azureBusClient = azureBusClient;
    }

    public async ValueTask SubscribeAsync(IMessageSubscriber messageSubscriber)
    {
        if (_messageProcessorsMap.ContainsKey(messageSubscriber.Type))
        {
            throw new InvalidOperationException(
                $"A processor for the message type {messageSubscriber.Type.FullName} already exists");
        }
        
        _messageProcessorsMap[messageSubscriber.Type] = await _azureBusClient.GetProcessorAsync(messageSubscriber.Type);
        await _messageProcessorsMap[messageSubscriber.Type].StartProcessingAsync();
    }

    public async ValueTask UnSubscribeAsync(IMessageSubscriber messageSubscriber)
    {
        if (_messageProcessorsMap.TryRemove(messageSubscriber.Type, out var processor))
        {
            //TODO: log
            await processor.StopProcessingAsync();
        }
        else
        {
            //TODO: log info
        }
    }
}