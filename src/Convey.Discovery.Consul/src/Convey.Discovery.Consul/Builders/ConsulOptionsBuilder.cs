using System;

namespace Convey.Discovery.Consul.Builders
{
    internal sealed class ConsulOptionsBuilder : IConsulOptionsBuilder
    {
        private readonly ConsulOptions _options = new ConsulOptions();

        public IConsulOptionsBuilder Enable(bool enabled)
        {
            _options.Enabled = enabled;
            return this;
        }

        public IConsulOptionsBuilder WithUrl(string url)
        {
            _options.Url = url;
            return this;
        }

        public IConsulOptionsBuilder WithService(string service)
        {
            _options.Service = service;
            return this;
        }

        public IConsulOptionsBuilder WithAddress(Uri address)
        {
            _options.HostName = address.Host;
            _options.Port = address.Port;
            _options.Scheme = address.Scheme;
            return this;
        }

        public IConsulOptionsBuilder WithSkippingLocalhostDockerDnsReplace(bool skipLocalhostDockerDnsReplace)
        {
            _options.SkipLocalhostDockerDnsReplace = skipLocalhostDockerDnsReplace;
            return this;
        }

        public IConsulOptionsBuilder WithHealthCheck(Action<IConsulHealthCheckOptionsBuilder> healthCheckBuilder)
        {
            var builder = new ConsulHealthCheckOptionsBuilder(_options.HealthCheck);
            healthCheckBuilder(builder);
            return this;
        }

        public ConsulOptions Build() => _options;
    }
}