using System.Net.Http;
using Convey.HTTP;

namespace Convey.Discovery.Consul.Http;

internal sealed class ConsulHttpClient : ConveyHttpClient, IConsulHttpClient
{
    public ConsulHttpClient(HttpClient client, HttpClientOptions options, IHttpClientSerializer serializer,
        ICorrelationContextFactory correlationContextFactory, ICorrelationIdFactory correlationIdFactory)
        : base(client, options, serializer, correlationContextFactory, correlationIdFactory)
    {
    }
}