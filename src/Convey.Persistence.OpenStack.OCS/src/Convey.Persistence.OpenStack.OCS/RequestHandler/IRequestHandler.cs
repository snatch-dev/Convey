using System;
using System.Threading.Tasks;
using Convey.Persistence.OpenStack.OCS.Http;

namespace Convey.Persistence.OpenStack.OCS.RequestHandler
{
    internal interface IRequestHandler
    {
        Task<HttpRequestResult> Send(Func<IHttpRequestBuilder, IHttpRequestBuilder> httpRequestBuilder);
    }
}