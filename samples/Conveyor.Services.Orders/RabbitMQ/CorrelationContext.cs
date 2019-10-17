namespace Conveyor.Services.Orders.RabbitMQ
{
    public class CorrelationContext
    {
        public string CorrelationId { get; set; }
        public string SpanContext { get; set; }
        public int Retries { get; set; }
    }
}
