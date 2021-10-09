using Convey.Persistence.Fs.Seaweed.Infrastructure.Http;
using System;

namespace Convey.Persistence.Fs.Seaweed.Operations.Abstractions
{
    public abstract class OperationBase
    {
        protected readonly IHttpRequestBuilder HttpRequestBuilder;

        protected OperationBase()
        {
            HttpRequestBuilder = new HttpRequestBuilder();
            Created = DateTime.Now;
        }
        public DateTime Created { get; }
    }
}