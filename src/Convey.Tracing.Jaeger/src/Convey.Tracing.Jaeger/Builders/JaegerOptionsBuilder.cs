namespace Convey.Tracing.Jaeger.Builders
{
    internal sealed class JaegerOptionsBuilder : IJaegerOptionsBuilder
    {
        private readonly JaegerOptions _options = new JaegerOptions();

        public IJaegerOptionsBuilder Enable(bool enabled)
        {
            _options.Enabled = enabled;
            return this;
        }

        public IJaegerOptionsBuilder WithServiceName(string serviceName)
        {
            _options.ServiceName = serviceName;
            return this;
        }

        public IJaegerOptionsBuilder WithUdpHost(string udpHost)
        {
            _options.UdpHost = udpHost;
            return this;
        }

        public IJaegerOptionsBuilder WithUdpPort(int udpPort)
        {
            _options.UdpPort = udpPort;
            return this;
        }

        public IJaegerOptionsBuilder WithMaxPacketSize(int maxPacketSize)
        {
            _options.MaxPacketSize = maxPacketSize;
            return this;
        }

        public IJaegerOptionsBuilder WithSampler(string sampler)
        {
            _options.Sampler = sampler;
            return this;
        }

        public IJaegerOptionsBuilder WithMaxTracesPerSecond(double maxTracesPerSecond)
        {
            _options.MaxTracesPerSecond = maxTracesPerSecond;
            return this;
        }

        public IJaegerOptionsBuilder WithSamplingRate(double samplingRate)
        {
            _options.SamplingRate = samplingRate;
            return this;
        }

        public JaegerOptions Build()
            => _options;
    }
}