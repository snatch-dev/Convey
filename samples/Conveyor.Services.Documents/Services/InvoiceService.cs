using Conveyor.Services.Documents.Domain;
using Conveyor.Services.Documents.Events.External;
using System;
using System.Threading.Tasks;

namespace Conveyor.Services.Documents.Services
{
    public class InvoiceService : IInvoiceService
    {
        public Task<OrderInvoiceDocument> GetAsync(OrderCreated orderCreated)
        {
            return Task.FromResult(new OrderInvoiceDocument(Guid.NewGuid(), orderCreated.OrderId));
        }
    }
}