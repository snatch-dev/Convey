using System;
using System.Threading.Tasks;
using Conveyor.Services.Orders.Commands;
using Conveyor.Services.Orders.DTO;
using Conveyor.Services.Orders.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Conveyor.Services.Orders.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderDto>> Get([FromRoute] GetOrder query)
        {
            // Fetch na order
            await Task.CompletedTask;
            var order = new OrderDto
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                TotalAmount = 50
            };

            return order;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CreateOrder command)
        {
            // Create an order
            await Task.CompletedTask;
            return CreatedAtAction(nameof(Get), new {orderId = command.OrderId}, null);
        }
    }
}
