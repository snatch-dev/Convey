using System;
using System.Collections.Generic;

namespace Convey.MessageBrokers.RabbitMQ
{
    public class RabbitMqOptions
    {
        public string ConnectionName { get; set; }
        public IEnumerable<string> HostNames { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public TimeSpan RequestedHeartbeat { get; set; } = TimeSpan.FromSeconds(60);
        public TimeSpan RequestedConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan SocketReadTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan SocketWriteTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan ContinuationTimeout { get; set; } = TimeSpan.FromSeconds(20);
        public TimeSpan HandshakeContinuationTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);
        public ushort RequestedChannelMax { get; set; }
        public uint RequestedFrameMax { get; set; }
        public bool UseBackgroundThreadsForIO { get; set; }
        public string ConventionsCasing { get; set; }
        public int Retries { get; set; }
        public int RetryInterval { get; set; }
        public bool MessagesPersisted { get; set; }
        public ContextOptions Context { get; set; }
        public ExchangeOptions Exchange { get; set; }
        public LoggerOptions Logger { get; set; }
        public SslOptions Ssl { get; set; }
        public QueueOptions Queue { get; set; }
        public DeadLetterOptions DeadLetter { get; set; }
        public QosOptions Qos { get; set; }
        public string SpanContextHeader { get; set; }
        public int MaxProducerChannels { get; set; }
        public bool RequeueFailedMessages { get; set; }

        public string GetSpanContextHeader()
            => string.IsNullOrWhiteSpace(SpanContextHeader) ? "span_context" : SpanContextHeader;

        public class LoggerOptions
        {
            public bool Enabled { get; set; }
            public string Level { get; set; }
        }

        public class ContextOptions
        {
            public bool Enabled { get; set; }
            public string Header { get; set; }
        }

        public class ExchangeOptions
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public bool Declare { get; set; }
            public bool Durable { get; set; }
            public bool AutoDelete { get; set; }
        }
        
        public class QueueOptions
        {
            public string Template { get; set; }
            public bool Declare { get; set; }
            public bool Durable { get; set; }
            public bool Exclusive { get; set; }
            public bool AutoDelete { get; set; }
        }

        public class DeadLetterOptions
        {
            public bool Enabled { get; set; }
            public string Prefix { get; set; }
            public bool Declare { get; set; }
            public bool Durable { get; set; }
            public bool Exclusive { get; set; }
            public bool AutoDelete { get; set; }
            public int Ttl { get; set; }
        }

        public class SslOptions
        {
            public bool Enabled { get; set; }
            public string ServerName { get; set; }
            public string CertificatePath { get; set; }
            public string CaCertificatePath { get; set; }
            public IEnumerable<string> X509IgnoredStatuses { get; set; }
        }

        public class QosOptions
        {
            public uint PrefetchSize { get; set; }
            public ushort PrefetchCount { get; set; }
            public bool Global { get; set; }
        }
    }
}