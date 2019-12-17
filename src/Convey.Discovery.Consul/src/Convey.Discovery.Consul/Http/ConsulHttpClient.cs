using System.Net.Http;
using Convey.HTTP;

namespace Convey.Discovery.Consul.Http
{
    internal sealed class ConsulHttpClient : ConveyHttpClient, IConsulHttpClient
    {
        public ConsulHttpClient(HttpClient client, HttpClientOptions options)
            : base(client, options)
        {
        }
    }
}