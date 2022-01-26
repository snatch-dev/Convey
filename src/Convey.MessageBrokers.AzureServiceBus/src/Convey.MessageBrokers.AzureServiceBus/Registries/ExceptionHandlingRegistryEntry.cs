using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.AzureServiceBus.Registries;

public record ExceptionHandlingRegistryEntry(
    Type MessageType,
    Func<Exception, ServiceBusReceivedMessage, ILogger, IMessageExceptionHandlingOperation?> Execute,
    ExceptionHandlingOperationPriority Priority);