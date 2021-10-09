using Convey.Persistence.Fs.Seaweed.Infrastructure.Builders;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace Convey.Persistence.Fs.Seaweed.Infrastructure.Http
{
    public interface IHttpRequestBuilder : IRequestBuilder<HttpRequestMessage>
    {
        IHttpRequestBuilder WithMethod(HttpMethod method);
        IHttpRequestBuilder WithRelativeUrl(string url);
        IHttpRequestBuilder WithHeader(string name, string value);
        IHttpRequestBuilder WithHeaders(IDictionary<string, string> headers);
        IHttpRequestBuilder WithStreamContent(Stream stream);
        IHttpRequestBuilder WithMultipartStreamFormDataContent(Stream stream, string fileName);
    }
}