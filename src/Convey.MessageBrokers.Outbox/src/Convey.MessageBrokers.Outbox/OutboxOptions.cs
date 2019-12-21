namespace Convey.MessageBrokers.Outbox
{
    public class OutboxOptions
    {
        public bool Enabled { get; set; }
        public string Type { get; set; }
        public int Expiry { get; set; }
        public double IntervalMilliseconds { get; set; }
        public string Collection { get; set; }
    }
}