using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.AzureServiceBus.Registries;

public interface IExceptionHandlingRegistry
{
    IReadOnlyList<ExceptionHandlingRegistryEntry> ExceptionHandlers { get; }

    IExceptionHandlingRegistry DeadLetter<T>(
        Func<Exception, ILogger, (bool DeadLetter, string? Reason)>? shouldDeadLetter = null,
        string? deadLetterReason = null) where T : Exception;

    IReadOnlyList<ExceptionHandlingRegistryEntry> GetOrderedEntries(Type type);
}