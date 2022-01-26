using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.AzureServiceBus.Registries;

internal sealed class ExceptionHandlingRegistry : IExceptionHandlingRegistry
{
    private readonly HashSet<ExceptionHandlingRegistryEntry> _exceptionHandlers = new();

    public IReadOnlyList<ExceptionHandlingRegistryEntry> ExceptionHandlers =>
        _exceptionHandlers.ToList();

    public IExceptionHandlingRegistry DeadLetter<T>(
        Func<Exception, ILogger, (bool DeadLetter, string? Reason)>? shouldDeadLetter = null,
        string? deadLetterReason = null)
        where T : Exception
    {
        var entry = new ExceptionHandlingRegistryEntry(typeof(T), (exception, _, logger) =>
        {
            if (shouldDeadLetter is not null)
            {
                var (should, reason) = shouldDeadLetter(exception, logger);
                if (should)
                {
                    return new DeadLetterMessageExceptionHandlingOperation(reason);
                }
            }

            return null;
        }, ExceptionHandlingOperationPriority.DeadLetter);

        _exceptionHandlers.Add(entry);

        return this;
    }

    public IReadOnlyList<ExceptionHandlingRegistryEntry> GetOrderedEntries(Type type) =>
        ExceptionHandlers
            .Where(x => x.MessageType == type)
            .OrderBy(x => x.Priority)
            .ToList();
}