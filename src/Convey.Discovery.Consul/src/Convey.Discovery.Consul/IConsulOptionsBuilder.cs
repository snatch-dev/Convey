using System;

namespace Convey.Discovery.Consul
{
    public interface IConsulOptionsBuilder
    {
        IConsulOptionsBuilder Enable(bool enabled);
        IConsulOptionsBuilder WithUrl(string url);
        IConsulOptionsBuilder WithService(string service);
        IConsulOptionsBuilder WithAddress(Uri address);
        IConsulOptionsBuilder WithHealthCheck(Action<IConsulHealthCheckOptionsBuilder> healthCheckBuilder);
        IConsulOptionsBuilder WithSkippingLocalhostDockerDnsReplace(bool skipLocalhostDockerDnsReplace);
        ConsulOptions Build();
    }
}