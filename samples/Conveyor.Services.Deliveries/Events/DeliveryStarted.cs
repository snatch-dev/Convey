using System;
using Convey.CQRS.Events;

namespace Conveyor.Services.Deliveries.Events;

public class DeliveryStarted : IEvent
{
    public Guid DeliveryId { get; }

    public DeliveryStarted(Guid deliveryId)
    {
        DeliveryId = deliveryId;
    }
}