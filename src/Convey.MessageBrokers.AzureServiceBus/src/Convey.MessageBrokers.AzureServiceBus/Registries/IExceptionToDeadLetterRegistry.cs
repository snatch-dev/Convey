namespace Convey.MessageBrokers.AzureServiceBus.Registries;

public interface IExceptionToDeadLetterRegistry
{
    IReadOnlyList<ExceptionToDeadLetterQueueRegistryEntry> ExceptionsToCauseDeadLettering { get; }

    IExceptionToDeadLetterRegistry DeadLetter<T>(
        Func<Exception, bool>? shouldDeadLetter = null,
        string? deadLetterReason = null) where T : Exception;

    IReadOnlyList<ExceptionToDeadLetterQueueRegistryEntry> GetEntries(Type type);
}