using Conveyor.Services.Documents.Domain;
using Conveyor.Services.Documents.Events.External;
using System.Threading.Tasks;

namespace Conveyor.Services.Documents.Services
{
    public interface IInvoiceService
    {
        Task<OrderInvoiceDocument> GetAsync(OrderCreated orderCreated);
    }
}