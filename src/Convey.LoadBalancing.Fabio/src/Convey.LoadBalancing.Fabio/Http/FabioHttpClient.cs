using System.Net.Http;
using Convey.HTTP;

namespace Convey.LoadBalancing.Fabio.Http
{
    internal sealed class FabioHttpClient : ConveyHttpClient, IFabioHttpClient
    {
        public FabioHttpClient(HttpClient client, HttpClientOptions options,
            ICorrelationContextFactory correlationContextFactory, ICorrelationIdFactory correlationIdFactory)
            : base(client, options, correlationContextFactory, correlationIdFactory)
        {
        }
    }
}