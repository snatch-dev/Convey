using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Convey.Discovery.Consul
{
    public class ConsulOptions
    {
        private string _hostName;
        private string _scheme = "http";

        public bool Enabled { get; set; } = true;
        public string Url { get; set; }
        public string Service { get; set; }
        public bool PreferIpAddress { get; set; } = false;
        public string IpAddress { get; set; }
        public string Scheme
        {
            get => _scheme;
            set => _scheme = value?.ToLower();
        }
        public string HostName
        {
            get => PreferIpAddress ? IpAddress : _hostName;
            set => _hostName = value;
        }
        public int Port { get; set; } = 80;
        public List<string> Tags { get; set; }
        public IDictionary<string, string> Meta { get; set; }
        public bool EnableTagOverride { get; set; }
        public bool SkipLocalhostDockerDnsReplace { get; set; }
        public ConnectOptions Connect { get; set; }
        public HealthCheckOptions HealthCheck { get; set; } = new HealthCheckOptions();

        public ConsulOptions()
        {
            _hostName = Dns.GetHostName();
            IpAddress = Dns.GetHostAddresses(_hostName)
                .FirstOrDefault(ip => ip.AddressFamily.Equals(AddressFamily.InterNetwork))?
                .ToString();
        }

        public class ConnectOptions
        {
            public bool Enabled { get; set; }
        }

        public class HealthCheckOptions
        {
            public bool Enabled { get; set; } = false;
            public string HealthCheckPath { get; set; } = "/health";
            public string HealthCheckMethod { get; set; } = "GET";
            public int HealthCheckInterval { get; set; } = 10;
            public int HealthCheckTimeout { get; set; } = 10;
            public int HealthCheckCriticalTimeout { get; set; } = 3;
            public bool HealthCheckTlsSkipVerify { get; set; } = false;

        }

        internal string GetServiceAddress()
        {
            var host = PreferIpAddress ? IpAddress : HostName;
            return $"{_scheme}://{host}:{Port}";
        }
    }
}