using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convey.MessageBrokers.Outbox.Outbox;
using Microsoft.Extensions.Hosting;

namespace Convey.MessageBrokers.Outbox.Processors
{
    internal sealed class OutboxProcessor : IHostedService
    {
        private readonly IMessageOutboxAccessor _outbox;
        private readonly IBusPublisher _publisher;
        private Timer _timer;

        public OutboxProcessor(IMessageOutbox outbox, IBusPublisher publisher)
        {
            _outbox = outbox as IMessageOutboxAccessor;
            _publisher = publisher;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
           _timer = new Timer(SendOutboxMessages, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
           return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void SendOutboxMessages(object state)
        {
            var messages = await _outbox.GetUnsentAsync();
            var publishTasks = messages.Select(om => _publisher.PublishAsync(om.Message, om.MessageId, 
                om.CorrelationId, om.SpanContext, om.MessageContext, om.Headers));
            await Task.WhenAll(publishTasks);
            await _outbox.ProcessAsync(messages);
        }
    }
}