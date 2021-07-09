using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Convey.Discovery.Consul.Models;

namespace Convey.Discovery.Consul.Services
{
    internal sealed class ConsulService : IConsulService
    {
        private static readonly StringContent EmptyRequest = GetPayload(new { });
        private const string Version = "v1";
        private readonly HttpClient _client;

        public ConsulService(HttpClient client)
        {
            _client = client;
        }

        public Task<HttpResponseMessage> RegisterServiceAsync(ServiceRegistration registration) =>
            _client.PutAsync(GetEndpoint("agent/service/register"), GetPayload(registration));

        public Task<HttpResponseMessage> DeregisterServiceAsync(string id)
            => _client.PutAsync(GetEndpoint($"agent/service/deregister/{id}"), EmptyRequest);

        public async Task<IDictionary<string, ServiceAgent>> GetServiceAgentsAsync(string service = null)
        {
            var filter = string.IsNullOrWhiteSpace(service) ? string.Empty : $"?filter=Service==\"{service}\"";
            var response = await _client.GetAsync(GetEndpoint($"agent/services{filter}"));
            if (!response.IsSuccessStatusCode)
            {
                return new Dictionary<string, ServiceAgent>();
            }

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<IDictionary<string, ServiceAgent>>(content);
        }

        private static StringContent GetPayload(object request) =>
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");


        private static string GetEndpoint(string endpoint) => $"{Version}/{endpoint}";
    }
}