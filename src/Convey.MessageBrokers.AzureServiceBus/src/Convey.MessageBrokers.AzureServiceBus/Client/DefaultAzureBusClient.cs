using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Convey.MessageBrokers.AzureServiceBus.Conventions;
using Convey.MessageBrokers.AzureServiceBus.Providers;
using Microsoft.Extensions.Options;

namespace Convey.MessageBrokers.AzureServiceBus.Client;

internal class DefaultAzureBusClient : IAzureBusClient
{
    private readonly INativeClientProvider _nativeClientProvider;
    private readonly IOptionsMonitor<AzureServiceBusOptions> _serviceBusOptions;
    private readonly IConventionsBuilder _conventionsBuilder;

    public DefaultAzureBusClient(
        INativeClientProvider nativeClientProvider,
        IOptionsMonitor<AzureServiceBusOptions> serviceBusOptions,
        IConventionsBuilder conventionsBuilder)
    {
        _nativeClientProvider = nativeClientProvider;
        _serviceBusOptions = serviceBusOptions;
        _conventionsBuilder = conventionsBuilder;
    }

    public async Task<ServiceBusProcessor> GetProcessorAsync(Type type)
    {
        var (topic, subscriber) = _conventionsBuilder.GetConventions(type);

        if (_serviceBusOptions.CurrentValue.AutomaticTopologyCreation)
        {
            var createdTopic = await _nativeClientProvider.UseAdminClientAsync(x =>
                x.TryCreateTopicAsync(new CreateTopicOptions(topic)));

            var createdSubscription = await _nativeClientProvider.UseAdminClientAsync(x =>
                x.TryCreateSubscriberAsync(new CreateSubscriptionOptions(topic, subscriber)));

            if (createdTopic)
            {
                //TODO: Log
            }

            if (createdSubscription)
            {
                //TODO: Log
            }
        }
        
        return _nativeClientProvider.UseClient(x => x.CreateProcessor(topic, subscriber));
    }
}