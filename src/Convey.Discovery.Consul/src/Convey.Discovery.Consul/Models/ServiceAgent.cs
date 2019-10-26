using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Convey.Discovery.Consul.Models
{
    public class ServiceAgent
    {
        [JsonPropertyName("ID")]
        public string Id { get; set; }
        public string Service { get; set; }
        public List<string> Tags { get; set; }
        public IDictionary<string, string> Meta { get; set; }
        public int Port { get; set; }
        public string Address { get; set; }
        public bool EnableTagOverride { get; set; }
        public Weights Weights { get; set; }
    }
}