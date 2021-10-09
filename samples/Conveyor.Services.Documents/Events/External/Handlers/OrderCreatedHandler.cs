using Convey.CQRS.Events;
using Convey.MessageBrokers;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Persistence.Fs.Seaweed.Infrastructure;
using Convey.Persistence.Fs.Seaweed.Operations.Inbound;
using Convey.Persistence.Fs.Seaweed.Operations.Outbound;
using Conveyor.Services.Documents.Files;
using Conveyor.Services.Documents.Services;
using Microsoft.Extensions.Logging;
using OpenTracing;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Conveyor.Services.Documents.Events.External.Handlers
{
    public class OrderCreatedHandler : IEventHandler<OrderCreated>
    {
        private readonly IBusPublisher _publisher;
        private readonly IMessagePropertiesAccessor _messagePropertiesAccessor;
        private readonly ITracer _tracer;
        private readonly ILogger<OrderCreatedHandler> _logger;
        private readonly IFiler _filer;
        private readonly IInvoiceService _invoiceService;
        private readonly string _spanContextHeader;

        public OrderCreatedHandler(IBusPublisher publisher, IMessagePropertiesAccessor messagePropertiesAccessor,
            RabbitMqOptions rabbitMqOptions, ITracer tracer, ILogger<OrderCreatedHandler> logger, IFiler filer, IInvoiceService invoiceService)
        {
            _publisher = publisher;
            _messagePropertiesAccessor = messagePropertiesAccessor;
            _tracer = tracer;
            _logger = logger;
            _filer = filer;
            _invoiceService = invoiceService;
            _spanContextHeader = string.IsNullOrWhiteSpace(rabbitMqOptions.SpanContextHeader)
                ? "span_context"
                : rabbitMqOptions.SpanContextHeader;
        }

        public async Task HandleAsync(OrderCreated @event)
        {
            _logger.LogInformation($"Received 'order created' event with order id: {@event.OrderId}");
            var orderInvoiceDocument = await _invoiceService.GetAsync(@event);
            _logger.LogInformation($"Got order invoice document with id: {orderInvoiceDocument.Id}");

            //Refactor to an external decorator
            var correlationId = _messagePropertiesAccessor?.MessageProperties?.CorrelationId;
            var spanContext = string.Empty;
            if (_messagePropertiesAccessor.MessageProperties.Headers.TryGetValue(_spanContextHeader, out var span)
                && span is byte[] spanBytes)
            {
                spanContext = Encoding.UTF8.GetString(spanBytes);
            }

            if (string.IsNullOrWhiteSpace(spanContext))
            {
                spanContext = _tracer.ActiveSpan is null ? string.Empty : _tracer.ActiveSpan.Context.ToString();
            }

            var orderInvoiceFile = OrderInvoice.Create(orderInvoiceDocument);
            _logger.LogInformation($"Got order invoice file for document with id: {orderInvoiceFile.DocumentId}");

            //write
            await using (var ufso =
                new UploadFileStreamOperation(orderInvoiceFile.Path, orderInvoiceFile.AsStream()))
            {
                if ((await ufso.Execute(_filer)).StatusCode == HttpStatusCode.Created)
                    _logger.LogInformation($"File successfully uploaded at: {orderInvoiceFile.Path}");
            }

            //read
            await using (var gfso = await (new GetFileStreamOperation(orderInvoiceFile.Path).Execute(_filer)))
            {
                //do whatever you want
            }


            await _publisher.PublishAsync(new OrderInvoiceCreated(orderInvoiceDocument.OrderId, orderInvoiceDocument.Id, orderInvoiceFile.Path), correlationId: correlationId,
                spanContext: spanContext);
        }
    }
}