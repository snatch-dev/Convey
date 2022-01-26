using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Convey.MessageBrokers.AzureServiceBus.Conventions;
using Convey.MessageBrokers.AzureServiceBus.Logging;
using Convey.MessageBrokers.AzureServiceBus.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Convey.MessageBrokers.AzureServiceBus.Client;

internal class DefaultAzureBusClient : IAzureBusClient
{
    private readonly INativeClientProvider _nativeClientProvider;
    private readonly IOptionsMonitor<AzureServiceBusOptions> _serviceBusOptions;
    private readonly IConventionsBuilder _conventionsBuilder;
    private readonly ILogger<DefaultAzureBusClient> _logger;

    public DefaultAzureBusClient(
        INativeClientProvider nativeClientProvider,
        IOptionsMonitor<AzureServiceBusOptions> serviceBusOptions,
        IConventionsBuilder conventionsBuilder,
        ILogger<DefaultAzureBusClient> logger)
    {
        _nativeClientProvider = nativeClientProvider;
        _serviceBusOptions = serviceBusOptions;
        _conventionsBuilder = conventionsBuilder;
        _logger = logger;
    }

    public async Task<ServiceBusProcessor> GetProcessorAsync(Type type)
    {
        //TODO: look at applying a subscription filter.
        var (topic, subscriber, _) = _conventionsBuilder.GetSubscriptionConventions(type);

        if (_serviceBusOptions.CurrentValue.AutomaticMessageEntityCreation)
        {
            var createdTopic = await _nativeClientProvider.UseAdminClientAsync(x =>
                x.TryCreateTopicAsync(new CreateTopicOptions(topic)));

            var createdSubscription = await _nativeClientProvider.UseAdminClientAsync(x =>
                x.TryCreateSubscriberAsync(new CreateSubscriptionOptions(topic, subscriber)));

            if (createdTopic)
            {
                _logger.LogServiceBusTopicCreated(type, topic);
            }

            if (createdSubscription)
            {
                _logger.LogServiceBusSubscriptionCreated(type, topic, subscriber);
            }
        }
        
        return _nativeClientProvider.UseClient(x => x.CreateProcessor(topic, subscriber));
    }
}