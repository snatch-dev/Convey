using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Convey.Persistence.OpenStack.OCS.Http;

internal class HttpRequestBuilder : IHttpRequestBuilder
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };
    private readonly HttpRequestMessage _httpRequestMessage;

    public HttpRequestBuilder()
    {
        _httpRequestMessage = new HttpRequestMessage();
    }

    public IHttpRequestBuilder WithMethod(HttpMethod method)
    {
        _httpRequestMessage.Method = method;
        return this;
    }

    public IHttpRequestBuilder WithRelativeUrl(string url)
    {
        //TODO: use regex for multiple '/' instead of replace function
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

    public IHttpRequestBuilder WithJsonContent(object contentObject, bool camelCasePropertyNames = true)
    {
        var serializerOptions = new JsonSerializerOptions();
        if (camelCasePropertyNames)
        {
            serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }
        var content = JsonSerializer.Serialize(contentObject, serializerOptions);

        _httpRequestMessage.Content = new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json);
        return this;
    }

    public IHttpRequestBuilder WithStreamContent(Stream stream)
    {
        _httpRequestMessage.Content = new StreamContent(stream);
        return this;
    }

    public HttpRequestMessage Build() => _httpRequestMessage;
}