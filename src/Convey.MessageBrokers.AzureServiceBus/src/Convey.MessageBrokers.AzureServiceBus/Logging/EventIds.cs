using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.AzureServiceBus.Logging;

public static class EventIds
{
    
    // -------------------- DEBUG 16_000-16_199 --------------------

    public static EventId SubscriberBackgroundServiceStarted = new(
        16_001,
        nameof(SubscriberBackgroundServiceStarted));
    
    public static EventId SubscriberBackgroundServiceStopped = new(
        16_002,
        nameof(SubscriberBackgroundServiceStopped));

    // -------------------- INFO 16_200-16_399 --------------------

    // -------------------- WARNING 16_400-16_499 --------------------

    // -------------------- ERROR 16_500-16_599 --------------------

    // -------------------- CRITICAL 16_600 - 16_650 --------------------
}