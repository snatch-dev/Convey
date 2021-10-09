using MimeKit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Convey.Persistence.Fs.Seaweed.Infrastructure.Http
{
    internal class HttpRequestBuilder : IHttpRequestBuilder
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        private readonly HttpRequestMessage _httpRequestMessage;
        public HttpRequestBuilder()
        {
            _httpRequestMessage = new HttpRequestMessage();
        }
        public virtual IHttpRequestBuilder WithMethod(HttpMethod method)
        {
            _httpRequestMessage.Method = method;
            return this;
        }
        public IHttpRequestBuilder WithRelativeUrl(string url)
        {
            _httpRequestMessage.RequestUri = new Uri(url.Replace("//", "/"), UriKind.Relative);
            return this;
        }
        public IHttpRequestBuilder WithHeader(string name, string value)
        {
            _httpRequestMessage.Headers.TryAddWithoutValidation(name, value);
            return this;
        }
        public IHttpRequestBuilder WithHeaders(IDictionary<string, string> headers)
        {
            foreach (var (name, value) in headers)
            {
                _httpRequestMessage.Headers.Add(name, value);
            }

            return this;
        }
        public IHttpRequestBuilder WithStreamContent(Stream stream)
        {
            _httpRequestMessage.Content = new StreamContent(stream);
            return this;
        }
        public IHttpRequestBuilder WithMultipartStreamFormDataContent(Stream stream, string fileName)
        {
            var form = new MultipartFormDataContent($"---{nameof(HttpRequestBuilder)}" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypes.GetMimeType(fileName));
            form.Add(streamContent,"form-data", fileName);
            _httpRequestMessage.Content = form;
            return this;
        }

        public HttpRequestMessage Build() => _httpRequestMessage;
    }
}