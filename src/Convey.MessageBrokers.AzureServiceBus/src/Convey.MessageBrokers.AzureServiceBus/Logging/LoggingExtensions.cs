using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.AzureServiceBus.Logging;

public static class LoggingExtensions
{
    // -------------------- SCOPES --------------------
    public static IDisposable BeginSubscriberBackgroundServiceScope(this ILogger logger, AzureServiceBusOptions options) =>
        logger.BeginScope(options.LoggingDetails());
    
    
    // -------------------- DEBUG 16_000-16_199 --------------------

    public static void LogBackgroundServiceStarted(this ILogger logger, string serviceName) =>
        LoggerMessageDefinitions.SubscriberBackgroundServiceStartedDef(logger, serviceName, null!);
    
    public static void LogBackgroundServiceStopped(this ILogger logger, string serviceName) =>
        LoggerMessageDefinitions.SubscriberBackgroundServiceStoppedDef(logger, serviceName, null!);
    
    public static void LogServiceBusMessageReceived(this ILogger logger, Type messageType) =>
        LoggerMessageDefinitions.ServiceBusMessageReceivedDef(logger, messageType.Name, null!);
    
    public static void LogServiceBusMessageProcessed(this ILogger logger, Type messageType) =>
        LoggerMessageDefinitions.ServiceBusMessageProcessedDef(logger, messageType.Name, null!);
    
    // -------------------- INFO 16_200-16_399 --------------------
    
    public static void LogServiceBusTopicCreated(this ILogger logger, Type messageType, string topicName) =>
        LoggerMessageDefinitions.ServiceBusTopicCreatedDef(logger, topicName, messageType.Name, null!);
    
    public static void LogServiceBusSubscriptionCreated(this ILogger logger, Type messageType, string topicName, string subscriptionName) =>
        LoggerMessageDefinitions.ServiceBusSubscriptionCreatedDef(logger, subscriptionName, topicName, messageType.Name, null!);
        
    // -------------------- ERROR 16_500-16_599 --------------------
    
    public static void LogServiceBusMessageProcessingFailed(this ILogger logger, Type messageType, Exception e) =>
        LoggerMessageDefinitions.ServiceBusMessageProcessingFailedDef(logger, messageType.Name, e);
    
    public static void LogServiceBusAdminClientPermissionsError(this ILogger logger, string serviceName, Exception e) =>
        LoggerMessageDefinitions.ServiceBusAdminClientPermissionsErrorDef(logger, serviceName, e);
    
    public static void LogServiceBusSubscriberError(this ILogger logger, Type messageType, Exception e) =>
        LoggerMessageDefinitions.ServiceBusAdminClientPermissionsErrorDef(logger, messageType.Name, e);
}