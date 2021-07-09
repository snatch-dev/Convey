using System;

namespace Convey.Discovery.Consul
{
    public interface IConsulHealthCheckOptionsBuilder
    {
        IConsulHealthCheckOptionsBuilder Enable(bool enable);
        IConsulHealthCheckOptionsBuilder WithHealthCheckPath(string healthCheckPath);
        IConsulHealthCheckOptionsBuilder WithHealthCheckInterval(TimeSpan healthCheckInterval);
        IConsulHealthCheckOptionsBuilder WithHealthCheckTimeout(TimeSpan healthCheckTimeout);
        IConsulHealthCheckOptionsBuilder WithHealthCheckCriticalTimeout(TimeSpan healthCheckCriticalTimeout);
        IConsulHealthCheckOptionsBuilder WithHealthCheckTlsSkipVerify(bool skipTlsVerify);
    }
}