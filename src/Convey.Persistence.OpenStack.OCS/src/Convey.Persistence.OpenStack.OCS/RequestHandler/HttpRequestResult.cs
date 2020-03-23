using System;
using System.Net;
using System.Net.Http;

namespace Convey.Persistence.OpenStack.OCS.RequestHandler
{
    internal class HttpRequestResult
    {
        public bool IsSuccess { get; }
        public HttpStatusCode StatusCode { get; }
        public HttpContent Content { get; }
        public Exception Exception { get; }

        public HttpRequestResult(Exception exception = null)
        {
            IsSuccess = false;
            Exception = exception;
        }

        public HttpRequestResult(bool isSuccess, HttpStatusCode statusCode, HttpContent content)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Content = content;
        }
    }
}
