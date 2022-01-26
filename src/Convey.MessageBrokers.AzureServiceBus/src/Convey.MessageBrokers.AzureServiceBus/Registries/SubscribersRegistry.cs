using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Convey.MessageBrokers.AzureServiceBus.Client;
using Convey.MessageBrokers.AzureServiceBus.Subscribers;

namespace Convey.MessageBrokers.AzureServiceBus.Registries;

internal class SubscribersRegistry : ISubscribersRegistry
{
    private readonly IAzureBusClient _azureBusClient;
    private readonly IBrokerMessageProcessorHandler _brokerMessageProcessorHandler;
    private readonly ConcurrentDictionary<Type, ServiceBusProcessor> _messageProcessorsMap = new();

    public SubscribersRegistry(
        IAzureBusClient azureBusClient, 
        IBrokerMessageProcessorHandler brokerMessageProcessorHandler)
    {
        _azureBusClient = azureBusClient;
        _brokerMessageProcessorHandler = brokerMessageProcessorHandler;
    }

    public async ValueTask SubscribeAsync(IMessageSubscriber messageSubscriber)
    {
        if (_messageProcessorsMap.ContainsKey(messageSubscriber.Type))
        {
            throw new InvalidOperationException(
                $"A processor for the message type {messageSubscriber.Type.FullName} already exists");
        }
        
        _messageProcessorsMap[messageSubscriber.Type] = await _azureBusClient.GetProcessorAsync(messageSubscriber.Type);
        await _brokerMessageProcessorHandler.StartAsync(_messageProcessorsMap[messageSubscriber.Type], messageSubscriber);
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