using System;

namespace Conveyor.Services.Documents.Domain
{
    public class OrderInvoiceDocument : Document
    {
        public Guid OrderId { get; private set; }

        public OrderInvoiceDocument(Guid id, Guid orderId)
        : base(id)
        {
            OrderId = orderId;
        }
    }
}