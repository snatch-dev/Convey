using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convey.MessageBrokers.Outbox.Outbox;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.Outbox.Processors
{
    internal sealed class OutboxProcessor : IHostedService
    {
        private readonly IMessageOutboxAccessor _outbox;
        private readonly IBusPublisher _publisher;
        private readonly OutboxOptions _options;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly TimeSpan _interval;
        private Timer _timer;

        public OutboxProcessor(IMessageOutbox outbox, IBusPublisher publisher, OutboxOptions options,
            ILogger<OutboxProcessor> logger)
        {
            if (options.Enabled && options.IntervalMilliseconds <= 0)
            {
                throw new Exception($"Invalid outbox interval: {options.IntervalMilliseconds} ms.");
            }

            _outbox = outbox as IMessageOutboxAccessor;
            _publisher = publisher;
            _options = options;
            _logger = logger;
            _interval = TimeSpan.FromMilliseconds(options.IntervalMilliseconds);
            if (options.Enabled)
            {
                _logger.LogInformation($"Outbox is enabled, message processing every {options.IntervalMilliseconds} ms.");
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
            var messages = await _outbox.GetUnsentAsync();
            _logger.LogTrace($"Found {messages.Count} unsent messages in outbox [job id: '{jobId}'].");
            if (!messages.Any())
            {
                _logger.LogTrace($"No messages to be processed in outbox [job id: '{jobId}'].");                
                return;
            }

            var publishTasks = messages.Select(om => _publisher.PublishAsync(om.Message, om.MessageId,
                om.CorrelationId, om.SpanContext, om.MessageContext, om.Headers, om.UserId));
            await Task.WhenAll(publishTasks);
            await _outbox.ProcessAsync(messages);
            stopwatch.Stop();
            _logger.LogTrace($"Processed {messages.Count} outbox messages in {stopwatch.ElapsedMilliseconds} ms [job id: '{jobId}'].");
        }
    }
}