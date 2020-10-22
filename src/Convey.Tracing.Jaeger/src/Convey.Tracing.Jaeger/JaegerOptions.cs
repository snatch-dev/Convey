using System.Collections.Generic;

namespace Convey.Tracing.Jaeger
{
    public class JaegerOptions
    {
        public bool Enabled { get; set; }
        public string ServiceName { get; set; }
        public string UdpHost { get; set; }
        public int UdpPort { get; set; }
        public int MaxPacketSize { get; set; } = 64967;
        public string Sampler { get; set; }
        public double MaxTracesPerSecond { get; set; } = 5;
        public double SamplingRate { get; set; } = 0.2;
        public IEnumerable<string> ExcludePaths { get; set; }
        public string Sender { get; set; }
        public HttpSenderOptions HttpSender { get; set; }

        public class HttpSenderOptions
        {
            public string Endpoint { get; set; }
            public string AuthToken { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string UserAgent { get; set; }
            public int MaxPacketSize { get; set; } = 1048576;
        }
    }
}