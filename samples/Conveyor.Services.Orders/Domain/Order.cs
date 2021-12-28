using System;
using Convey.Types;

namespace Conveyor.Services.Orders.Domain;

public class Order : IIdentifiable<Guid>
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }

    public Order(Guid id, Guid customerId, decimal totalAmount)
    {
        Id = id;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}