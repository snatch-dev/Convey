using System.Collections.Generic;

namespace Convey.Discovery.Consul.Models
{
    public class ServiceCheck
    {
        public string DeregisterCriticalServiceAfter { get; set; }
        public List<string> Args { get; set; }
        public string Http { get; set; }
        public string Interval { get; set; }
        public string Ttl { get; set; }
        public string Timeout { get; set; }
        public bool TLSSkipVerify { get; set; }
        public string Method { get; set; }
    }
}