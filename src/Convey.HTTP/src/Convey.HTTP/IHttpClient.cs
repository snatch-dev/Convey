using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Convey.HTTP
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string uri);
        Task<T> GetAsync<T>(string uri);
        Task<HttpResult<T>> GetResultAsync<T>(string uri);
        Task<HttpResponseMessage> PostAsync(string uri, object data = null);
        Task<T> PostAsync<T>(string uri, object data = null);
        Task<HttpResult<T>> PostResultAsync<T>(string uri, object data = null);
        Task<HttpResponseMessage> PutAsync(string uri, object data = null);
        Task<T> PutAsync<T>(string uri, object data = null);
        Task<HttpResult<T>> PutResultAsync<T>(string uri, object data = null);
        Task<HttpResponseMessage> PatchAsync(string uri, object data = null);
        Task<T> PatchAsync<T>(string uri, object data = null);
        Task<HttpResult<T>> PatchResultAsync<T>(string uri, object data = null);
        Task<HttpResponseMessage> DeleteAsync(string uri);
        Task<T> DeleteAsync<T>(string uri);
        Task<HttpResult<T>> DeleteResultAsync<T>(string uri);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
        Task<T> SendAsync<T>(HttpRequestMessage request);
        Task<HttpResult<T>> SendResultAsync<T>(HttpRequestMessage request);
        void SetHeaders(IDictionary<string, string> headers);
        void SetHeaders(Action<HttpRequestHeaders> headers);
    }
}