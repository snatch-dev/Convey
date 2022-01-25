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
        
    // -------------------- ERROR 16_500-16_599 --------------------
    
    public static void LogServiceBusMessageProcessingFailed(this ILogger logger, Type messageType, Exception e) =>
        LoggerMessageDefinitions.ServiceBusMessageProcessingFailedDef(logger, messageType.Name, e);
}