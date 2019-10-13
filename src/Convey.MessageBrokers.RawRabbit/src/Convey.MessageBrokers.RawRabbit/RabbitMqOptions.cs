using RawRabbit.Configuration;

namespace Convey.MessageBrokers.RawRabbit
{
    public class RabbitMqOptions : RawRabbitConfiguration
    {
        public string Namespace { get; set; }
        public int Retries { get; set; }
        public int RetryInterval { get; set; }
        public MessageProcessorOptions MessageProcessor { get; set; }

        public class MessageProcessorOptions
        {
            public bool Enabled { get; set; }
            public string Type { get; set; }
            public int MessageExpirySeconds { get; set; }
        }
    }
}