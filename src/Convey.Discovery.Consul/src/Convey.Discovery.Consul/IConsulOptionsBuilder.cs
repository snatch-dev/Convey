namespace Convey.Discovery.Consul
{
    public interface IConsulOptionsBuilder
    {
        IConsulOptionsBuilder Enable(bool enabled);
        IConsulOptionsBuilder WithUrl(string url);
        IConsulOptionsBuilder WithService(string service);
        IConsulOptionsBuilder WithAddress(string address);
        IConsulOptionsBuilder WithEnabledPing(bool pingEnabled);
        IConsulOptionsBuilder WithPingEndpoint(string pingEndpoint);
        IConsulOptionsBuilder WithPingInterval(int pingInterval);
        IConsulOptionsBuilder WithRemoteAfterInterval(int remoteAfterInterval);
        IConsulOptionsBuilder WithSkippingLocalhostDockerDnsReplace(bool skipLocalhostDockerDnsReplace);
        ConsulOptions Build();
    }
}