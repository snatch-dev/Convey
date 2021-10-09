using System;
using Convey.Persistence.Fs.Seaweed.Infrastructure;
using Convey.Persistence.Fs.Seaweed.Operations.Abstractions;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Convey.Persistence.Fs.Seaweed.Operations.Inbound
{
    public class GetFileStreamOperation : OperationBase, IFilerOperation<Stream>
    {
        private readonly string _path;

        public GetFileStreamOperation(string path)
        {
            _path = path;
        }

        public Task<Stream> Execute(IFiler filer)
        {
            return filer.GetStreamAsync(HttpRequestBuilder
                .WithMethod(HttpMethod.Get)
                .WithRelativeUrl(_path)
                .Build());
        }
    }
}