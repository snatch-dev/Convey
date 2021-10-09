using Conveyor.Services.Documents.Domain;
using System.IO;

namespace Conveyor.Services.Documents.Files
{
    public class OrderInvoice : File
    {
        public OrderInvoiceDocument Document { get; }

        protected OrderInvoice(OrderInvoiceDocument document, string fileName)
        : base(document.Id, fileName)
        {
            Document = document;
        }
        public string Path => $"orders/{this.Document.OrderId}/invoices/{this.FileName}";
        public static OrderInvoice Create(OrderInvoiceDocument document) => new OrderInvoice(document, $"{document.Id}.pdf");
        public override byte[] AsRaw() => System.IO.File.ReadAllBytes("invoice.pdf");
        public override Stream AsStream() => System.IO.File.OpenRead("invoice.pdf");
    }
}