using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.Outbox.Processors;

internal sealed class OutboxProcessor : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBusPublisher _publisher;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _interval;
    private readonly OutboxType _type;
    private Timer _timer;

    public OutboxProcessor(IServiceProvider serviceProvider, IBusPublisher publisher, OutboxOptions options,
        ILogger<OutboxProcessor> logger)
    {
        if (options.Enabled && options.IntervalMilliseconds <= 0)
        {
            throw new Exception($"Invalid outbox interval: {options.IntervalMilliseconds} ms.");
        }

        _type = OutboxType.Sequential;
        if (!string.IsNullOrWhiteSpace(options.Type))
        {
            if (!Enum.TryParse<OutboxType>(options.Type, true, out var outboxType))
            {
                throw new ArgumentException($"Invalid outbox type: '{_type}', " +
                                            $"valid types: '{OutboxType.Sequential}', '{OutboxType.Parallel}'.");
            }

            _type = outboxType;
        }

        _serviceProvider = serviceProvider;
        _publisher = publisher;
        _options = options;
        _logger = logger;            
        _interval = TimeSpan.FromMilliseconds(options.IntervalMilliseconds);
        if (options.Enabled)
        {
            _logger.LogInformation($"Outbox is enabled, type: '{_type}', message processing every {options.IntervalMilliseconds} ms.");
            return;
        }

        _logger.LogInformation("Outbox is disabled.");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return Task.CompletedTask;
        }

        _timer = new Timer(SendOutboxMessages, null, TimeSpan.Zero, _interval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return Task.CompletedTask;
        }

        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void SendOutboxMessages(object state)
    {
        _ = SendOutboxMessagesAsync();
    }

    private async Task SendOutboxMessagesAsync()
    {
        var jobId = Guid.NewGuid().ToString("N");
        _logger.LogTrace($"Started processing outbox messages... [job id: '{jobId}']");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using var scope = _serviceProvider.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IMessageOutboxAccessor>();
        var messages = await outbox.GetUnsentAsync();
        _logger.LogTrace($"Found {messages.Count} unsent messages in outbox [job ID: '{jobId}'].");
        if (!messages.Any())
        {
            _logger.LogTrace($"No messages to be processed in outbox [job ID: '{jobId}'].");
            return;
        }

        foreach (var message in messages.OrderBy(m => m.SentAt))
        {
            await _publisher.PublishAsync(message.Message, message.Id, message.CorrelationId,
                message.SpanContext, message.MessageContext, message.Headers);
            if (_type == OutboxType.Sequential)
            {
                await outbox.ProcessAsync(message);
            }
        }

        if (_type == OutboxType.Parallel)
        {
            await outbox.ProcessAsync(messages);
        }
            
        stopwatch.Stop();
        _logger.LogTrace($"Processed {messages.Count} outbox messages in {stopwatch.ElapsedMilliseconds} ms [job ID: '{jobId}'].");
    }

    private enum OutboxType
    {
        Sequential,
        Parallel
    }
}