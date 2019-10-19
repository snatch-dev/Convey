using System;
using System.Threading.Tasks;
using Convey.CQRS.Events;
using Convey.MessageBrokers;
using Conveyor.Services.Deliveries.RabbitMQ;
using Microsoft.Extensions.Logging;

namespace Conveyor.Services.Deliveries.Events.External.Handlers
{
    public class OrderCreatedHandler : IEventHandler<OrderCreated>
    {
        private readonly IBusPublisher _publisher;
        private readonly ILogger<OrderCreatedHandler> _logger;

        public OrderCreatedHandler(IBusPublisher publisher, ILogger<OrderCreatedHandler> logger)
        {
            _publisher = publisher;
            _logger = logger;
        }

        public Task HandleAsync(OrderCreated @event)
        {
            _logger.LogInformation($"Received 'order created' event with order id: {@event.OrderId}");
            var deliveryId = Guid.NewGuid();
            _logger.LogInformation($"Starting a delivery with id: {deliveryId}");
            return _publisher.PublishAsync(new DeliveryStarted(deliveryId), messageContext: new CorrelationContext());
        }
    }
}
