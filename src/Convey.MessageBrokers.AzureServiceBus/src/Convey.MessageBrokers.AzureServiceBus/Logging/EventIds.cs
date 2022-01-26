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
    
    public static EventId ServiceBusMessageReceived = new(
        16_003,
        nameof(ServiceBusMessageReceived));
    
    public static EventId ServiceBusMessageProcessed = new(
        16_004,
        nameof(ServiceBusMessageProcessed));

    // -------------------- INFO 16_200-16_399 --------------------
    
    public static EventId ServiceBusTopicCreated = new(
        16_200,
        nameof(ServiceBusTopicCreated));
    
    public static EventId ServiceBusSubscriptionCreated = new(
        16_201,
        nameof(ServiceBusTopicCreated));

    // -------------------- WARNING 16_400-16_499 --------------------

    // -------------------- ERROR 16_500-16_599 --------------------
    
    public static EventId ServiceBusMessageProcessingFailed = new(
        16_500,
        nameof(ServiceBusMessageProcessingFailed));
    
    public static EventId ServiceBusAdminClientPermissionsError = new(
        16_501,
        nameof(ServiceBusAdminClientPermissionsError));

    public static EventId ServiceBusSubscriberError = new(
        16_502,
        nameof(ServiceBusSubscriberError));

    // -------------------- CRITICAL 16_600 - 16_650 --------------------
}