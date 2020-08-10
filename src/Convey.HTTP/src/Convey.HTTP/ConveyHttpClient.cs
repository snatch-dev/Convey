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
    public class ConveyHttpClient : IHttpClient
    {
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
            => SendAsync(uri, Method.Post, GetJsonPayload(data));

        public Task<HttpResponseMessage> PostAsync(string uri, HttpContent content = null)
            => SendAsync(uri, Method.Post, content);

        public virtual Task<T> PostAsync<T>(string uri, object data = null)
            => SendAsync<T>(uri, Method.Post, GetJsonPayload(data));

        public Task<T> PostAsync<T>(string uri, HttpContent content = null)
            => SendAsync<T>(uri, Method.Post, content);

        public Task<HttpResult<T>> PostResultAsync<T>(string uri, object data = null)
            => SendResultAsync<T>(uri, Method.Post, GetJsonPayload(data));

        public Task<HttpResult<T>> PostResultAsync<T>(string uri, HttpContent content = null)
            => SendResultAsync<T>(uri, Method.Post, content);

        public virtual Task<HttpResponseMessage> PutAsync(string uri, object data = null)
            => SendAsync(uri, Method.Put, GetJsonPayload(data));

        public Task<HttpResponseMessage> PutAsync(string uri, HttpContent content = null)
            => SendAsync(uri, Method.Put, content);

        public virtual Task<T> PutAsync<T>(string uri, object data = null)
            => SendAsync<T>(uri, Method.Put, GetJsonPayload(data));

        public Task<T> PutAsync<T>(string uri, HttpContent content = null)
            => SendAsync<T>(uri, Method.Put, content);

        public Task<HttpResult<T>> PutResultAsync<T>(string uri, object data = null)
            => SendResultAsync<T>(uri, Method.Put, GetJsonPayload(data));

        public Task<HttpResult<T>> PutResultAsync<T>(string uri, HttpContent content = null)
            => SendResultAsync<T>(uri, Method.Put, content);

        public Task<HttpResponseMessage> PatchAsync(string uri, object data = null)
            => SendAsync(uri, Method.Patch, GetJsonPayload(data));

        public Task<HttpResponseMessage> PatchAsync(string uri, HttpContent content = null)
            => SendAsync(uri, Method.Patch, content);

        public Task<T> PatchAsync<T>(string uri, object data = null)
            => SendAsync<T>(uri, Method.Patch, GetJsonPayload(data));

        public Task<T> PatchAsync<T>(string uri, HttpContent content = null)
            => SendAsync<T>(uri, Method.Patch, content);

        public Task<HttpResult<T>> PatchResultAsync<T>(string uri, object data = null)
            => SendResultAsync<T>(uri, Method. Patch, GetJsonPayload(data));

        public Task<HttpResult<T>> PatchResultAsync<T>(string uri, HttpContent content = null)
            => SendResultAsync<T>(uri, Method. Patch, content);

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
        
        protected virtual async Task<T> SendAsync<T>(string uri, Method method, HttpContent content = null)
        {
            var response = await SendAsync(uri, method, content);
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var stream = await response.Content.ReadAsStreamAsync();

            return DeserializeJsonFromStream<T>(stream);
        }
        
        protected virtual async Task<HttpResult<T>> SendResultAsync<T>(string uri, Method method, HttpContent content = null)
        {
            var response = await SendAsync(uri, method, content);
            if (!response.IsSuccessStatusCode)
            {
                return new HttpResult<T>(default, response);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var result = DeserializeJsonFromStream<T>(stream);
            
            return new HttpResult<T>(result, response);
        }

        protected virtual Task<HttpResponseMessage> SendAsync(string uri, Method method, HttpContent content = null)
            => Policy.Handle<Exception>()
                .WaitAndRetryAsync(_options.Retries, r => TimeSpan.FromSeconds(Math.Pow(2, r)))
                .ExecuteAsync(() =>
                {
                    var requestUri = uri.StartsWith("http") ? uri : $"http://{uri}";
                    
                    return GetResponseAsync(requestUri, method, content);
                });

        protected virtual Task<HttpResponseMessage> GetResponseAsync(string uri, Method method, HttpContent content = null)
        {
            switch (method)
            {
                case Method.Get:
                    return _client.GetAsync(uri);
                case Method.Post:
                    return _client.PostAsync(uri, content);
                case Method.Put:
                    return _client.PutAsync(uri, content);
                case Method.Patch:
                    return _client.PatchAsync(uri, content);
                case Method.Delete:
                    return _client.DeleteAsync(uri);
                default:
                    throw new InvalidOperationException($"Unsupported HTTP method: {method}");
            }
        }

        protected StringContent GetJsonPayload(object data)
        {
            if (data is null)
            {
                return null;
            }

            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            if (_options.RemoveCharsetFromContentType)
            {
                content.Headers.ContentType.CharSet = null;
            }

            return content;
        }

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