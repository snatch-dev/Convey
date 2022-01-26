namespace Convey.MessageBrokers.AzureServiceBus.Registries;

public record ExceptionToDeadLetterQueueRegistryEntry(
    Type MessageType,
    string? DeadLetterReason = null,
    Func<Exception, bool>? ShouldDeadLetter = null);