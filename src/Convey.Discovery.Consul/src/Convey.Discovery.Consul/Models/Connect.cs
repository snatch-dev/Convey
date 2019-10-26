using System.Text.Json.Serialization;

namespace Convey.Discovery.Consul.Models
{
    public class Connect
    {
        [JsonPropertyName("sidecar_service")]
        public SidecarService SidecarService { get; set; }
    }

}