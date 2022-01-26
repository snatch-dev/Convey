using Azure.Messaging.ServiceBus;
using Convey.MessageBrokers.AzureServiceBus.Conventions;
using Convey.MessageBrokers.AzureServiceBus.Registries;
using Convey.MessageBrokers.AzureServiceBus.Serializers;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.AzureServiceBus.Publishers;

public class AzureServiceBusPublisher : IBusPublisher
{
    private readonly ILogger<AzureServiceBusPublisher> _logger;
    private readonly ISenderRegistry _senderRegistry;
    private readonly IAzureServiceBusSerializer _azureServiceBusSerializer;
    private readonly IConventionsBuilder _conventionsBuilder;

    public AzureServiceBusPublisher(
        ILogger<AzureServiceBusPublisher> logger,
        ISenderRegistry senderRegistry,
        IAzureServiceBusSerializer azureServiceBusSerializer,
        IConventionsBuilder conventionsBuilder)
    {
        _logger = logger;
        _senderRegistry = senderRegistry;
        _azureServiceBusSerializer = azureServiceBusSerializer;
        _conventionsBuilder = conventionsBuilder;
    }

    public async Task PublishAsync<T>(
        T message,
        string? messageId = null,
        string? correlationId = null,
        string? spanContext = null,
        object? messageContext = null,
        IDictionary<string, object>? headers = null)
        where T : class
    {
        var sender = await _senderRegistry.GetSender<T>();
        
        //TODO: log debug creating message.
        
        var serviceBusMessage = BuildMessage(
            message,
            messageId,
            correlationId,
            headers);

        await sender.SendMessageAsync(serviceBusMessage);
        
        //TODO: log info sent message with correlation ID.
    }

    private ServiceBusMessage BuildMessage<T>(
        T message,
        string? messageId = null,
        string? correlationId = null,
        IDictionary<string, object>? headers = null)
        where T : class
    {
        var messageType = typeof(T);
        var serializedMessage = _azureServiceBusSerializer.Serialize(message);

        var serviceBusMessage = new ServiceBusMessage(serializedMessage.ToArray())
        {
            To = _conventionsBuilder.GetToValue(messageType),
            Subject = _conventionsBuilder.GetSubjectValue(messageType),
            CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
            ContentType = "application/json",
            MessageId = messageId ?? Guid.NewGuid().ToString(),
        };

        //TODO: handle span context, what is this?
        //TODO: handle messageContext via a builder of some sort. Need more context on this.

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                serviceBusMessage.ApplicationProperties.Add(header.Key, header.Value);
            }
        }

        return serviceBusMessage;
    }
}