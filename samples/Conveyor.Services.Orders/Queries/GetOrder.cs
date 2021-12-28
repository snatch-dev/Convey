using System;
using Convey.CQRS.Queries;
using Conveyor.Services.Orders.DTO;

namespace Conveyor.Services.Orders.Queries;

public class GetOrder : IQuery<OrderDto>
{
    public Guid OrderId { get; set; }
}