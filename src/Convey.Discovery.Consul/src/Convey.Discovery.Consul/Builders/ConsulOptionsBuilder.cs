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

        public IConsulOptionsBuilder WithAddress(string address)
        {
            _options.Address = address;
            return this;
        }

        public IConsulOptionsBuilder WithEnabledPing(bool pingEnabled)
        {
            _options.PingEnabled = pingEnabled;
            return this;
        }

        public IConsulOptionsBuilder WithPingEndpoint(string pingEndpoint)
        {
            _options.PingEndpoint = pingEndpoint;
            return this;
        }

        public IConsulOptionsBuilder WithPingInterval(string pingInterval)
        {
            _options.PingInterval = pingInterval;
            return this;
        }

        public IConsulOptionsBuilder WithRemoteAfterInterval(string remoteAfterInterval)
        {
            _options.RemoveAfterInterval = remoteAfterInterval;
            return this;
        }

        public IConsulOptionsBuilder WithSkippingLocalhostDockerDnsReplace(bool skipLocalhostDockerDnsReplace)
        {
            _options.SkipLocalhostDockerDnsReplace = skipLocalhostDockerDnsReplace;
            return this;
        }

        public ConsulOptions Build() => _options;
    }
}