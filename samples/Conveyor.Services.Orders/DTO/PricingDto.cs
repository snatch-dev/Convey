using System;

namespace Conveyor.Services.Orders.DTO;

public class PricingDto
{
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
}