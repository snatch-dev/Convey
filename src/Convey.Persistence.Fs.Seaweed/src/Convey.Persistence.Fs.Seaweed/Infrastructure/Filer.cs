using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Convey.Persistence.Fs.Seaweed.Infrastructure
{
    internal sealed class Filer : IFiler
    {
        private readonly HttpClient _httpClient;

        public Filer(IHttpClientFactory httpClientFactory, SeaweedOptions options)
        {
            _httpClient = httpClientFactory.CreateClient(options.FilerHttpClientName);
        }

        Task<Stream> IFiler.GetStreamAsync(HttpRequestMessage httpRequestMessage)
        {
            return _httpClient.GetStreamAsync(httpRequestMessage.RequestUri);
        }

        Task<HttpResponseMessage> IFiler.SendAsync(HttpRequestMessage httpRequestMessage)
        {
            return _httpClient.SendAsync(httpRequestMessage);
        }
    }
}