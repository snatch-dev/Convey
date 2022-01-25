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
}