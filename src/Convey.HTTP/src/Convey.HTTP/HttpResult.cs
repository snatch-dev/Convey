using System.Net.Http;

namespace Convey.HTTP;

public class HttpResult<T>
{
    public T Result { get; }
    public HttpResponseMessage Response { get; }
    public bool HasResult => Result is not null;

    public HttpResult(T result, HttpResponseMessage response)
    {
        Result = result;
        Response = response;
    }
}