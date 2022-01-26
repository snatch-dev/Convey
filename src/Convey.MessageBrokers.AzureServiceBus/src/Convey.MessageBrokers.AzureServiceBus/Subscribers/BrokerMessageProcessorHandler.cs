using Azure.Messaging.ServiceBus;
using Convey.MessageBrokers.AzureServiceBus.Logging;
using Convey.MessageBrokers.AzureServiceBus.Registries;
using Convey.MessageBrokers.AzureServiceBus.Serializers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Convey.MessageBrokers.AzureServiceBus.Subscribers;

internal class BrokerMessageProcessorHandler : IBrokerMessageProcessorHandler
{
    private readonly ILogger<BrokerMessageProcessorHandler> _logger;
    private readonly IAzureServiceBusSerializer _serializer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<AzureServiceBusOptions> _serviceBusOptions;
    private readonly IExceptionHandlingRegistry _exceptionHandlingRegistry;

    public BrokerMessageProcessorHandler(
        ILogger<BrokerMessageProcessorHandler> logger,
        IAzureServiceBusSerializer serializer,
        IServiceProvider serviceProvider,
        IOptionsMonitor<AzureServiceBusOptions> serviceBusOptions,
        IExceptionHandlingRegistry exceptionHandlingRegistry)
    {
        _logger = logger;
        _serializer = serializer;
        _serviceProvider = serviceProvider;
        _serviceBusOptions = serviceBusOptions;
        _exceptionHandlingRegistry = exceptionHandlingRegistry;
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

            var operation = GetExceptionHandlingOperation(e, arg);

            switch (operation)
            {
                case DeadLetterMessageExceptionHandlingOperation deadLetter:
                    await arg.DeadLetterMessageAsync(arg.Message, deadLetter.Reason ?? e.Message);
                    return;
                case DeferMessageExceptionHandlingOperation:
                    await arg.DeferMessageAsync(arg.Message);
                    return;
            }

            throw;
        }

        _logger.LogServiceBusMessageProcessed(subscriber.Type);
    }

    private IMessageExceptionHandlingOperation? GetExceptionHandlingOperation(Exception e, ProcessMessageEventArgs args)
    {
        var entries =
            _exceptionHandlingRegistry.GetOrderedEntries(e.GetType());

        foreach (var handler in entries)
        {
            var result = handler.Execute(e, args.Message, _logger);

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }
}