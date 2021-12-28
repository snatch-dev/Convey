using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Convey.CQRS.Queries;
using Conveyor.Services.Orders.Commands;
using Conveyor.Services.Orders.DTO;
using Conveyor.Services.Orders.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Conveyor.Services.Orders.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public OrdersController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderDto>> Get([FromRoute] GetOrder query)
    {
        var order = await _queryDispatcher.QueryAsync(query);
        if (order is null)
        {
            return NotFound();
        }

        return order;
    }

    [HttpPost]
    public async Task<ActionResult> Post(CreateOrder command)
    {
        await _commandDispatcher.SendAsync(command);
        return CreatedAtAction(nameof(Get), new {orderId = command.OrderId}, null);
    }
}