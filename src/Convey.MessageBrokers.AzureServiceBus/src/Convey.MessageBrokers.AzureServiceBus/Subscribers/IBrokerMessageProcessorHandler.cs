using Azure.Messaging.ServiceBus;

namespace Convey.MessageBrokers.AzureServiceBus.Subscribers;

internal interface IBrokerMessageProcessorHandler
{
    Task StartAsync(ServiceBusProcessor processor, IMessageSubscriber subscriber);
}