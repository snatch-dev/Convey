using Convey.CQRS.Events;
using System;

namespace Conveyor.Services.Documents.Events
{
    public class OrderInvoiceCreated : IEvent
    {
        public Guid OrderId { get; }
        public Guid DocumentId { get; }
        public string FilePath { get; }
        public OrderInvoiceCreated(Guid orderId, Guid documentId, string filePath)
        {
            OrderId = orderId;
            DocumentId = documentId;
            FilePath = filePath;
        }
    }
}