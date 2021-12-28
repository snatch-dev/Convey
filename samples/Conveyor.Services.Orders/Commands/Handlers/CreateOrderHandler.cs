using System;
using System.Threading;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Convey.MessageBrokers;
using Convey.MessageBrokers.Outbox;
using Convey.Persistence.MongoDB;
using Conveyor.Services.Orders.Domain;
using Conveyor.Services.Orders.Events;
using Conveyor.Services.Orders.Services;
using Microsoft.Extensions.Logging;
using OpenTracing;

namespace Conveyor.Services.Orders.Commands.Handlers;

public class CreateOrderHandler : ICommandHandler<CreateOrder>
{
    private readonly IMongoRepository<Order, Guid> _repository;
    private readonly IBusPublisher _publisher;
    private readonly IMessageOutbox _outbox;
    private readonly IPricingServiceClient _pricingServiceClient;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly ITracer _tracer;

    public CreateOrderHandler(IMongoRepository<Order, Guid> repository, IBusPublisher publisher,
        IMessageOutbox outbox, IPricingServiceClient pricingServiceClient, ITracer tracer,
        ILogger<CreateOrderHandler> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _outbox = outbox;
        _pricingServiceClient = pricingServiceClient;
        _tracer = tracer;
        _logger = logger;
    }

    public async Task HandleAsync(CreateOrder command, CancellationToken cancellationToken = default)
    {
        var exists = await _repository.ExistsAsync(o => o.Id == command.OrderId);
        if (exists)
        {
            throw new InvalidOperationException($"Order with given id: {command.OrderId} already exists!");
        }

        _logger.LogInformation($"Fetching a price for order with id: {command.OrderId}...");
        var pricingDto = await _pricingServiceClient.GetAsync(command.OrderId);
        if (pricingDto is null)
        {
            throw new InvalidOperationException($"Pricing was not found for order: {command.OrderId}");
        }

        _logger.LogInformation($"Order with id: {command.OrderId} will cost: {pricingDto.TotalAmount}$.");
        var order = new Order(command.OrderId, command.CustomerId, pricingDto.TotalAmount);
        await _repository.AddAsync(order);
        _logger.LogInformation($"Created an order with id: {command.OrderId}, customer: {command.CustomerId}.");
        var spanContext = _tracer.ActiveSpan?.Context.ToString();
        var @event = new OrderCreated(order.Id);
        if (_outbox.Enabled)
        {
            await _outbox.SendAsync(@event, spanContext: spanContext);
            return;
        }

        await _publisher.PublishAsync(@event, spanContext: spanContext);
    }
}