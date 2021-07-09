using System;

namespace Convey.Discovery.Consul.Builders
{
    internal sealed class ConsulHealthCheckOptionsBuilder : IConsulHealthCheckOptionsBuilder
    {
        private readonly ConsulOptions.HealthCheckOptions _healthCheckOptions;

        internal ConsulHealthCheckOptionsBuilder(ConsulOptions.HealthCheckOptions healthCheckOptions)
        {
            _healthCheckOptions = healthCheckOptions;
        }

        public IConsulHealthCheckOptionsBuilder Enable(bool enable)
        {
            _healthCheckOptions.Enabled = enable;
            return this;
        }

        public IConsulHealthCheckOptionsBuilder WithHealthCheckPath(string healthCheckPath)
        {
            _healthCheckOptions.HealthCheckPath = healthCheckPath;
            return this;
        }

        public IConsulHealthCheckOptionsBuilder WithHealthCheckInterval(TimeSpan healthCheckInterval)
        {
            _healthCheckOptions.HealthCheckInterval = (int)healthCheckInterval.TotalSeconds;
            return this;
        }

        public IConsulHealthCheckOptionsBuilder WithHealthCheckTimeout(TimeSpan healthCheckTimeout)
        {
            _healthCheckOptions.HealthCheckTimeout = (int)healthCheckTimeout.TotalSeconds;
            return this;
        }

        public IConsulHealthCheckOptionsBuilder WithHealthCheckCriticalTimeout(TimeSpan healthCheckCriticalTimeout)
        {
            _healthCheckOptions.HealthCheckCriticalTimeout = (int)healthCheckCriticalTimeout.TotalMinutes;
            return this;
        }

        public IConsulHealthCheckOptionsBuilder WithHealthCheckTlsSkipVerify(bool skipTlsVerify)
        {
            _healthCheckOptions.HealthCheckTlsSkipVerify = skipTlsVerify;
            return this;
        }
    }
}