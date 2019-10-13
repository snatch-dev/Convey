using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;

namespace Convey.HTTP
{
    public class ConveyHttpClient : IHttpClient
    {
        private const string ApplicationJsonContentType = "application/json";

        private static readonly StringContent EmptyJson =
            new StringContent("{}", Encoding.UTF8, ApplicationJsonContentType);

        private static readonly JsonSerializer JsonSerializer = new JsonSerializer
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly HttpClient _client;
        private readonly HttpClientOptions _options;
        private readonly ILogger<IHttpClient> _logger;

        public ConveyHttpClient(HttpClient client, HttpClientOptions options, ILogger<IHttpClient> logger)
        {
            _client = client;
            _options = options;
            _logger = logger;
        }

        public virtual Task<HttpResponseMessage> GetAsync(string uri)
            => SendAsync(uri, Method.Get);

        public virtual Task<T> GetAsync<T>(string uri)
            => SendAsync<T>(uri, Method.Get);

        public virtual Task<HttpResponseMessage> PostAsync(string uri, object data = null)
            => SendAsync(uri, Method.Post, data);

        public virtual Task<T> PostAsync<T>(string uri, object data = null)
            => SendAsync<T>(uri, Method.Post, data);

        public virtual Task<HttpResponseMessage> PutAsync(string uri, object data = null)
            => SendAsync(uri, Method.Put, data);

        public virtual Task<T> PutAsync<T>(string uri, object data = null)
            => SendAsync<T>(uri, Method.Put, data);

        public virtual Task<HttpResponseMessage> DeleteAsync(string uri)
            => SendAsync(uri, Method.Delete);

        protected virtual async Task<T> SendAsync<T>(string uri, Method method, object data = null)
        {
            var response = await SendAsync(uri, method, data);
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var stream = await response.Content.ReadAsStreamAsync();

            return DeserializeJsonFromStream<T>(stream);
        }

        protected virtual Task<HttpResponseMessage> SendAsync(string uri, Method method, object data = null)
            => Policy.Handle<Exception>()
                .WaitAndRetryAsync(_options.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
                .ExecuteAsync(async () =>
                {
                    var methodName = method.ToString().ToUpperInvariant();
                    var requestUri = uri.StartsWith("http://") ? uri : $"http://{uri}";
                    _logger.LogDebug($"Sending HTTP {methodName} request to URI: {uri}");
                    var response = await GetResponseAsync(requestUri, method, data);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogDebug($"Received a valid response to HTTP {methodName} request from URI: " +
                                         $"{requestUri}{Environment.NewLine}{response}");
                    }
                    else
                    {
                        _logger.LogError($"Received an invalid response to HTTP {methodName} request from URI: " +
                                         $"{requestUri}{Environment.NewLine}{response}");
                    }

                    return response;
                });

        protected virtual Task<HttpResponseMessage> GetResponseAsync(string uri, Method method, object data = null)
        {
            switch (method)
            {
                case Method.Get:
                    return _client.GetAsync(uri);
                case Method.Post:
                    return _client.PostAsync(uri, GetJsonPayload(data));
                case Method.Put:
                    return _client.PutAsync(uri, GetJsonPayload(data));
                case Method.Delete:
                    return _client.DeleteAsync(uri);
                default:
                    throw new InvalidOperationException($"Unsupported HTTP method: " +
                                                        $"{method.ToString().ToUpperInvariant()}.");
            }
        }

        protected static StringContent GetJsonPayload(object data)
            => data is null
                ? EmptyJson
                : new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, ApplicationJsonContentType);

        protected static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
            {
                return default;
            }

            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return JsonSerializer.Deserialize<T>(jsonTextReader);
            }
        }

        protected enum Method
        {
            Get,
            Post,
            Put,
            Delete
        }
    }
}