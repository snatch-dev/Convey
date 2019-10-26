using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Microsoft.Extensions.Logging;

namespace Conveyor.Services.Orders.Commands.Handlers
{
    public class CreateOrderHandler : ICommandHandler<CreateOrder>
    {
        private readonly ILogger<CreateOrderHandler> _logger;

        public CreateOrderHandler(ILogger<CreateOrderHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(CreateOrder command)
        {
            await Task.CompletedTask;
            _logger.LogInformation($"Created an order with id: {command.OrderId}, customer: {command.CustomerId}.");
        }
    }
}
