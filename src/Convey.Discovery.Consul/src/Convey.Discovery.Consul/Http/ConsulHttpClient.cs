using System.Net.Http;
using Convey.HTTP;
using Microsoft.Extensions.Logging;

namespace Convey.Discovery.Consul.Http
{
    internal sealed class ConsulHttpClient : ConveyHttpClient, IConsulHttpClient
    {
        public ConsulHttpClient(HttpClient client, HttpClientOptions options, ILogger<IHttpClient> logger)
            : base(client, options, logger)
        {
        }
    }
}