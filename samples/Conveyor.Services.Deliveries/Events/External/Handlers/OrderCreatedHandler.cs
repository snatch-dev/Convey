using System;
using System.Threading.Tasks;
using Convey.CQRS.Events;
using Convey.MessageBrokers;
using Microsoft.Extensions.Logging;

namespace Conveyor.Services.Deliveries.Events.External.Handlers
{
    public class OrderCreatedHandler : IEventHandler<OrderCreated>
    {
        private readonly IBusPublisher _publisher;
        private readonly IMessagePropertiesAccessor _messagePropertiesAccessor;
        private readonly ILogger<OrderCreatedHandler> _logger;

        public OrderCreatedHandler(IBusPublisher publisher, IMessagePropertiesAccessor messagePropertiesAccessor,
            ILogger<OrderCreatedHandler> logger)
        {
            _publisher = publisher;
            _messagePropertiesAccessor = messagePropertiesAccessor;
            _logger = logger;
        }

        public Task HandleAsync(OrderCreated @event)
        {
            _logger.LogInformation($"Received 'order created' event with order id: {@event.OrderId}");
            var deliveryId = Guid.NewGuid();
            _logger.LogInformation($"Starting a delivery with id: {deliveryId}");
            var correlationId = _messagePropertiesAccessor.MessageProperties.CorrelationId;
            return _publisher.PublishAsync(new DeliveryStarted(deliveryId), correlationId: correlationId);
        }
    }
}
