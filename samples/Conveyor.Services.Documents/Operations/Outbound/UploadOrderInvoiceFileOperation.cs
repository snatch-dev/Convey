using Convey.Persistence.Fs.Seaweed.Operations.Outbound;
using Conveyor.Services.Documents.Files;

namespace Conveyor.Services.Documents.Operations.Outbound
{
    public class UploadOrderInvoiceFileOperation : UploadFileStreamOperation
    {
        public UploadOrderInvoiceFileOperation(OrderInvoice orderInvoice) : base(orderInvoice.Path, orderInvoice.AsStream())
        {
        }
    }
}