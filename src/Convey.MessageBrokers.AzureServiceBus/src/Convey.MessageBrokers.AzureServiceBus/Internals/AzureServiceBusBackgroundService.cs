using Convey.MessageBrokers.AzureServiceBus.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.AzureServiceBus.Internals;

internal class AzureServiceBusHostedService : BackgroundService
{
    private readonly ILogger<AzureServiceBusHostedService> _logger;
    private readonly ISubscribersChannel _subscribersChannel;

    public AzureServiceBusHostedService(
        ILogger<AzureServiceBusHostedService> logger,
        ISubscribersChannel subscribersChannel)
    {
        _logger = logger;
        _subscribersChannel = subscribersChannel;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var subscriber in _subscribersChannel.ReadAsync(stoppingToken))
        {
            
        }
    }
}