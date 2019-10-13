using System.Net.Http;
using Convey.HTTP;
using Microsoft.Extensions.Logging;

namespace Convey.LoadBalancing.Fabio.Http
{
    internal sealed class FabioHttpClient : ConveyHttpClient, IFabioHttpClient
    {
        public FabioHttpClient(HttpClient client, HttpClientOptions options, ILogger<IHttpClient> logger)
            : base(client, options, logger)
        {
        }
    }
}