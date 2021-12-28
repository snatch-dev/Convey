using System;
using Convey.CQRS.Events;

namespace Conveyor.Services.Orders.Events;

public class OrderCreated : IEvent
{
    public Guid OrderId { get; }

    public OrderCreated(Guid orderId)
    {
        OrderId = orderId;
    }
}