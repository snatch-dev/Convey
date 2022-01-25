using Azure.Messaging.ServiceBus;
using Convey.MessageBrokers.AzureServiceBus.Logging;
using Convey.MessageBrokers.AzureServiceBus.Serializers;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.AzureServiceBus.Subscribers;

internal class BrokerMessageProcessorHandler : IBrokerMessageProcessorHandler
{
    private readonly ILogger<BrokerMessageProcessorHandler> _logger;
    private readonly IAzureServiceBusSerializer _serializer;
    private readonly IServiceProvider _serviceProvider;

    public BrokerMessageProcessorHandler(
        ILogger<BrokerMessageProcessorHandler> logger,
        IAzureServiceBusSerializer serializer,
        IServiceProvider serviceProvider
    )
    {
        _logger = logger;
        _serializer = serializer;
        _serviceProvider = serviceProvider;
    }
    
    public Task StartAsync(ServiceBusProcessor processor, IMessageSubscriber subscriber)
    {
        processor.ProcessMessageAsync += args => OnProcessMessageAsync(args, subscriber);
        processor.ProcessErrorAsync += args => OnProcessErrorAsync(args, subscriber);

        return processor.StartProcessingAsync();
    }

    private Task OnProcessErrorAsync(ProcessErrorEventArgs arg, IMessageSubscriber messageSubscriber)
    {
        _logger.LogError(arg.Exception, "Processing message of type {MessageType} failed", messageSubscriber.Type);

        return Task.CompletedTask;
    }

    private async Task OnProcessMessageAsync(ProcessMessageEventArgs arg, IMessageSubscriber subscriber)
    {
        _logger.LogServiceBusMessageReceived(subscriber.Type);
        
        //TODO: consider a message plugin pipeline here. Before processing the message.

        try
        {
            var message = _serializer.Deserialize(arg.Message.Body, subscriber.Type);

            //TODO: extract properties properly.
            object properties = "todo";
            
            await subscriber.Handle(_serviceProvider, message, properties);
        }
        catch (Exception e)
        {
            _logger.LogServiceBusMessageProcessingFailed(subscriber.Type, e);
            
            //TODO: this should be optional
            await arg.DeadLetterMessageAsync(arg.Message);
        }
        
        _logger.LogServiceBusMessageProcessed(subscriber.Type);
    }
}