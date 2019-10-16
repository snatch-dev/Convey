using Convey.MessageBrokers;

namespace Conveyor.Services.Deliveries.RabbitMQ
{
    public class CorrelationContext
    {
        public string CorrelationId { get; set; }
        public string SpanContext { get; set; }
        public int Retries { get; set; }
    }
}
