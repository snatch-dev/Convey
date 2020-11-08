using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace Convey.Persistence.OpenStack.OCS.Http
{
    internal interface IHttpRequestBuilder
    {
        IHttpRequestBuilder WithMethod(HttpMethod method);
        IHttpRequestBuilder WithRelativeUrl(string requestUrl);
        IHttpRequestBuilder WithHeader(string name, string value);
        IHttpRequestBuilder WithHeaders(IDictionary<string, string> headers);
        IHttpRequestBuilder WithJsonContent(object contentObject, bool camelCasePropertyNames = true);
        IHttpRequestBuilder WithStreamContent(Stream stream);
        HttpRequestMessage Build();
    }
}