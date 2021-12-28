using System;
using System.Threading.Tasks;
using Conveyor.Services.Orders.DTO;

namespace Conveyor.Services.Orders.Services;

public interface IPricingServiceClient
{
    Task<PricingDto> GetAsync(Guid orderId);
}