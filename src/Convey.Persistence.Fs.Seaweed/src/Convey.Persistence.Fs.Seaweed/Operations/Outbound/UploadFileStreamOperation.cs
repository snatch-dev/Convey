using Convey.Persistence.Fs.Seaweed.Infrastructure;
using Convey.Persistence.Fs.Seaweed.Operations.Abstractions;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Convey.Persistence.Fs.Seaweed.Operations.Outbound
{
    public class UploadFileStreamOperation : OutboundStreamOperation, IFilerOperation<HttpResponseMessage>, IAsyncDisposable
    {
        private readonly string _path;

        public UploadFileStreamOperation(string path, Stream stream)
            : base(stream)
        {
            _path = path;
        }
        public string FileName => Path.GetFileName(_path);
        public Task<HttpResponseMessage> Execute(IFiler filer)
        {
            var request = this.BuildRequest();
            return filer.SendAsync(request);
        }

        protected virtual HttpRequestMessage BuildRequest()
        {
            return HttpRequestBuilder.WithRelativeUrl(_path)
                .WithMethod(HttpMethod.Post)
                .WithMultipartStreamFormDataContent(_stream, FileName)
                .Build();
        }
    }
}