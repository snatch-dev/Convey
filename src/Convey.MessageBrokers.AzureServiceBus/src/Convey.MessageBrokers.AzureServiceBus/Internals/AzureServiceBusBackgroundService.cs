using System.Runtime.InteropServices.ComTypes;
using Azure.Messaging.ServiceBus;
using Convey.MessageBrokers.AzureServiceBus.Logging;
using Convey.MessageBrokers.AzureServiceBus.Registries;
using Convey.MessageBrokers.AzureServiceBus.Subscribers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Convey.MessageBrokers.AzureServiceBus.Internals;

internal class AzureServiceBusHostedService : BackgroundService
{
    private readonly ILogger<AzureServiceBusHostedService> _logger;
    private readonly IOptionsMonitor<AzureServiceBusOptions> _serviceBusOptions;
    private readonly ISubscribersChannel _subscribersChannel;
    private readonly ISubscribersRegistry _subscribersRegistry;

    public AzureServiceBusHostedService(
        ILogger<AzureServiceBusHostedService> logger,
        IOptionsMonitor<AzureServiceBusOptions> serviceBusOptions,
        ISubscribersChannel subscribersChannel,
        ISubscribersRegistry subscribersRegistry)
    {
        _logger = logger;
        _serviceBusOptions = serviceBusOptions;
        _subscribersChannel = subscribersChannel;
        _subscribersRegistry = subscribersRegistry;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (_logger.BeginSubscriberBackgroundServiceScope(_serviceBusOptions.CurrentValue))
        {
            _logger.LogBackgroundServiceStarted(_serviceBusOptions.CurrentValue.ServiceName);
            
            await foreach (var subscriber in _subscribersChannel.ReadAsync(stoppingToken))
            {
                try
                {
                    var messageSubscriberActionTask = subscriber.Action is MessageSubscriberAction.Subscribe
                        ? _subscribersRegistry.SubscribeAsync(subscriber)
                        : _subscribersRegistry.UnSubscribeAsync(subscriber);

                    await messageSubscriberActionTask;
                }
                catch (UnauthorizedAccessException e)
                {
                    _logger.LogServiceBusAdminClientPermissionsError(_serviceBusOptions.CurrentValue.ServiceName, e);
                    throw;
                }
                catch (Exception e)
                {
                    _logger.LogServiceBusSubscriberError(subscriber.Type, e);
                    throw;
                }
            }
            
            _logger.LogBackgroundServiceStopped(_serviceBusOptions.CurrentValue.ServiceName);
        }
    }
}