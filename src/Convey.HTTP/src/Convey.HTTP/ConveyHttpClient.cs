using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;

namespace Convey.HTTP
{
    internal class ConveyHttpClient : IHttpClient
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

        public ConveyHttpClient(HttpClient client, HttpClientOptions options)
        {
            _client = client;
            _options = options;
        }

        public virtual Task<HttpResponseMessage> GetAsync(string uri)
            => SendAsync(uri, Method.Get);

        public virtual Task<T> GetAsync<T>(string uri)
            => SendAsync<T>(uri, Method.Get);

        public Task<HttpResult<T>> GetResultAsync<T>(string uri)
            => SendResultAsync<T>(uri, Method.Get);

        public virtual Task<HttpResponseMessage> PostAsync(string uri, object data = null)
            => SendAsync(uri, Method.Post, data);

        public virtual Task<T> PostAsync<T>(string uri, object data = null)
            => SendAsync<T>(uri, Method.Post, data);

        public Task<HttpResult<T>> PostResultAsync<T>(string uri, object data = null)
            => SendResultAsync<T>(uri, Method.Post, data);

        public virtual Task<HttpResponseMessage> PutAsync(string uri, object data = null)
            => SendAsync(uri, Method.Put, data);

        public virtual Task<T> PutAsync<T>(string uri, object data = null)
            => SendAsync<T>(uri, Method.Put, data);

        public Task<HttpResult<T>> PutResultAsync<T>(string uri, object data = null)
            => SendResultAsync<T>(uri, Method.Put, data);

        public Task<HttpResponseMessage> PatchAsync(string uri, object data = null)
            => SendAsync(uri, Method.Patch, data);

        public Task<T> PatchAsync<T>(string uri, object data = null)
            => SendAsync<T>(uri, Method.Patch, data);

        public Task<HttpResult<T>> PatchResultAsync<T>(string uri, object data = null)
            => SendResultAsync<T>(uri, Method. Patch, data);
        
        public virtual Task<HttpResponseMessage> DeleteAsync(string uri)
            => SendAsync(uri, Method.Delete);

        public Task<T> DeleteAsync<T>(string uri)
            => SendAsync<T>(uri, Method.Delete);

        public Task<HttpResult<T>> DeleteResultAsync<T>(string uri)
            => SendResultAsync<T>(uri, Method.Delete);

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            => Policy.Handle<Exception>()
                .WaitAndRetryAsync(_options.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
                .ExecuteAsync(() => _client.SendAsync(request));

        public Task<T> SendAsync<T>(HttpRequestMessage request)
            => Policy.Handle<Exception>()
                .WaitAndRetryAsync(_options.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
                .ExecuteAsync(async () =>
                {
                    var response = await _client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        return default;
                    }

                    var stream = await response.Content.ReadAsStreamAsync();

                    return DeserializeJsonFromStream<T>(stream);
                });

        public Task<HttpResult<T>> SendResultAsync<T>(HttpRequestMessage request)
            => Policy.Handle<Exception>()
                .WaitAndRetryAsync(_options.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
                .ExecuteAsync(async () =>
                {
                    var response = await _client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        return new HttpResult<T>(default, response);
                    }

                    var stream = await response.Content.ReadAsStreamAsync();
                    var result = DeserializeJsonFromStream<T>(stream);

                    return new HttpResult<T>(result, response);
                });
        
        public void SetHeaders(IDictionary<string, string> headers)
        {
            if (headers is null)
            {
                return;
            }

            foreach (var (key, value) in headers)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                _client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
            }
        }

        public void SetHeaders(Action<HttpRequestHeaders> headers) => headers?.Invoke(_client.DefaultRequestHeaders);
        
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
        
        protected virtual async Task<HttpResult<T>> SendResultAsync<T>(string uri, Method method, object data = null)
        {
            var response = await SendAsync(uri, method, data);
            if (!response.IsSuccessStatusCode)
            {
                return new HttpResult<T>(default, response);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var result = DeserializeJsonFromStream<T>(stream);
            
            return new HttpResult<T>(result, response);
        }

        protected virtual Task<HttpResponseMessage> SendAsync(string uri, Method method, object data = null)
            => Policy.Handle<Exception>()
                .WaitAndRetryAsync(_options.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
                .ExecuteAsync(() =>
                {
                    var requestUri = uri.StartsWith("http") ? uri : $"http://{uri}";
                    
                    return GetResponseAsync(requestUri, method, data);
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
                case Method.Patch:
                    return _client.PatchAsync(uri, GetJsonPayload(data));
                case Method.Delete:
                    return _client.DeleteAsync(uri);
                default:
                    throw new InvalidOperationException($"Unsupported HTTP method: {method}");
            }
        }

        protected static StringContent GetJsonPayload(object data)
            => data is null
                ? EmptyJson
                : new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, ApplicationJsonContentType);

        protected static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream is null || stream.CanRead is false)
            {
                return default;
            }

            using var streamReader = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(streamReader);
            
            return JsonSerializer.Deserialize<T>(jsonTextReader);
        }

        protected enum Method
        {
            Get,
            Post,
            Put,
            Patch,
            Delete
        }
    }
}