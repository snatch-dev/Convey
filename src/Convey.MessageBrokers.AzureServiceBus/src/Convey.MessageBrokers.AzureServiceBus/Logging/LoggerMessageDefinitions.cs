using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.AzureServiceBus.Logging;

public static class LoggerMessageDefinitions
{
    // -------------------- DEBUG 16_000-16_199 --------------------

    public static readonly Action<ILogger, string, Exception> SubscriberBackgroundServiceStartedDef =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            EventIds.SubscriberBackgroundServiceStarted,
            "Message broker listener started for service {ServiceName}");

    public static readonly Action<ILogger, string, Exception> SubscriberBackgroundServiceStoppedDef =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            EventIds.SubscriberBackgroundServiceStopped,
            "Message broker background service stopped for service {ServiceName}");

    public static readonly Action<ILogger, string, Exception> ServiceBusMessageReceivedDef =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            EventIds.ServiceBusMessageReceived,
            "Service bus message of type {MessageType} received");

    public static readonly Action<ILogger, string, Exception> ServiceBusMessageProcessedDef =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            EventIds.ServiceBusMessageProcessed,
            "Service bus message of type {MessageType} processed");
    
    // -------------------- INFO 16_200-16_399 --------------------
    
    public static readonly Action<ILogger, string, string, Exception> ServiceBusTopicCreatedDef =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            EventIds.ServiceBusTopicCreated,
            "Service bus message topic created {TopicName} for message {MessageType}");
    
    public static readonly Action<ILogger, string, string, string, Exception> ServiceBusSubscriptionCreatedDef =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Information,
            EventIds.ServiceBusSubscriptionCreated,
            "Service bus message subscription created {SubscriptionName} for topic {TopicName} for message {MessageType}");

    // -------------------- ERROR 16_500-16_599 --------------------

    public static readonly Action<ILogger, string, Exception> ServiceBusMessageProcessingFailedDef =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            EventIds.ServiceBusMessageProcessed,
            "Service bus message of type {MessageType} processing failed");

    public static readonly Action<ILogger, string, Exception> ServiceBusAdminClientPermissionsErrorDef =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            EventIds.ServiceBusAdminClientPermissionsError,
            "The service bus admin client does not have sufficient permissions to create resources, please check the connection strings permissions for {ServiceName} or disable AutomaticMessageEntityCreation");
    
    public static readonly Action<ILogger, string, Exception> ServiceBusSubscriberErrorDef =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            EventIds.ServiceBusAdminClientPermissionsError,
            "Failed to start listening to events for message {MessageType}");
    
    
}