using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.AzureServiceBus.Logging;

public static class LoggerMessageDefinitions
{
    // -------------------- DEBUG 16_000-16_199 --------------------
    
    public static readonly Action<ILogger, string, Exception> SubscriberBackgroundServiceStartedDef = LoggerMessage.Define<string>(
        LogLevel.Debug,
        EventIds.SubscriberBackgroundServiceStarted,
        "Message broker listener started for service {ServiceName}");
    
    public static readonly Action<ILogger, string, Exception> SubscriberBackgroundServiceStoppedDef = LoggerMessage.Define<string>(
        LogLevel.Debug,
        EventIds.SubscriberBackgroundServiceStopped,
        "Message broker background service stopped for service {ServiceName}");
    
    public static readonly Action<ILogger, string, Exception> ServiceBusMessageReceivedDef = LoggerMessage.Define<string>(
        LogLevel.Debug,
        EventIds.ServiceBusMessageReceived,
        "Service bus message of type {MessageType} received");
    
    public static readonly Action<ILogger, string, Exception> ServiceBusMessageProcessedDef = LoggerMessage.Define<string>(
        LogLevel.Debug,
        EventIds.ServiceBusMessageProcessed,
        "Service bus message of type {MessageType} processed");
    
    // -------------------- ERROR 16_500-16_599 --------------------
    
    public static readonly Action<ILogger, string, Exception> ServiceBusMessageProcessingFailedDef = LoggerMessage.Define<string>(
        LogLevel.Error,
        EventIds.ServiceBusMessageProcessed,
        "Service bus message of type {MessageType} processing failed");
}