namespace Convey.Tracing.Jaeger
{
    public interface IJaegerOptionsBuilder
    {
        IJaegerOptionsBuilder Enable(bool enabled);
        IJaegerOptionsBuilder WithServiceName(string serviceName);
        IJaegerOptionsBuilder WithUdpHost(string udpHost);
        IJaegerOptionsBuilder WithUdpPort(int udpPort);
        IJaegerOptionsBuilder WithMaxPacketSize(int maxPacketSize);
        IJaegerOptionsBuilder WithSampler(string sampler);
        IJaegerOptionsBuilder WithMaxTracesPerSecond(double maxTracesPerSecond);
        IJaegerOptionsBuilder WithSamplingRate(double samplingRate);
        JaegerOptions Build();
    }
}