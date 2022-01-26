namespace Convey.MessageBrokers.AzureServiceBus.Registries;

internal sealed class ExceptionToDeadLetterRegistry : IExceptionToDeadLetterRegistry
{
    private readonly HashSet<ExceptionToDeadLetterQueueRegistryEntry> _exceptionsToCauseDeadLettering = new();

    public IReadOnlyList<ExceptionToDeadLetterQueueRegistryEntry> ExceptionsToCauseDeadLettering =>
        _exceptionsToCauseDeadLettering.ToList();

    public IExceptionToDeadLetterRegistry DeadLetter<T>(
        Func<Exception, bool>? shouldDeadLetter = null,
        string? deadLetterReason = null)
        where T : Exception
    {
        var entry = new ExceptionToDeadLetterQueueRegistryEntry(
            typeof(T),
            deadLetterReason,
            shouldDeadLetter);

        _exceptionsToCauseDeadLettering.Add(entry);
        
        return this;
    }

    public IReadOnlyList<ExceptionToDeadLetterQueueRegistryEntry> GetEntries(Type type) =>
        ExceptionsToCauseDeadLettering.Where(x => x.MessageType == type).ToList();
}