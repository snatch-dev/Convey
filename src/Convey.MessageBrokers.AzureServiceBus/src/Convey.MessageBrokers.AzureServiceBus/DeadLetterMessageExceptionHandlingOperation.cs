namespace Convey.MessageBrokers.AzureServiceBus;

public record DeadLetterMessageExceptionHandlingOperation(string? Reason = null) 
    : IMessageExceptionHandlingOperation;