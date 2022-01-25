using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.AzureServiceBus.Logging;

public static class LoggingExtensions
{
    // -------------------- DEBUG 16_000-16_199 --------------------

    public static void LogBackgroundServiceStarted(this ILogger logger, string serviceName) =>
        LoggerMessageDefinitions.SubscriberBackgroundServiceStartedDef(logger, serviceName, null!);
    
    public static void LogBackgroundServiceStopped(this ILogger logger, string serviceName) =>
        LoggerMessageDefinitions.SubscriberBackgroundServiceStoppedDef(logger, serviceName, null!);
}