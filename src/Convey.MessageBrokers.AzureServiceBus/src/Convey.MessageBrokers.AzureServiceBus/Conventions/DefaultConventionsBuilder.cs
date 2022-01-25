using Convey.MessageBrokers.AzureServiceBus.Options;
using Microsoft.Extensions.Options;

namespace Convey.MessageBrokers.AzureServiceBus.Conventions;

public class DefaultConventionsBuilder : IConventionsBuilder
{
    private readonly IOptionsMonitor<AzureServiceBusOptions> _serviceBusOptions;
    private readonly IEnumerable<MessageOptions> _messageOptions;

    public DefaultConventionsBuilder(
        IEnumerable<MessageOptions> messageOptions,
        IOptionsMonitor<AzureServiceBusOptions> serviceBusOptions)
    {
        _serviceBusOptions = serviceBusOptions;
        _messageOptions = messageOptions.ToList();
    }

    public string GetTopicName(Type type) =>
        type.Name.ToSnakeCase();

    public string GetSubscriberName(Type type) =>
        _serviceBusOptions.CurrentValue.ServiceName;

    public string? GetSubscriptionFilter(Type type) =>
        null;

}