using System;
using System.IO;
using System.Threading.Tasks;

namespace Convey.Persistence.Fs.Seaweed.Operations.Abstractions
{
    public abstract class OutboundStreamOperation : OperationBase, IDisposable
    {
        protected readonly Stream _stream;

        protected OutboundStreamOperation(Stream stream)
        {
            _stream = stream;
        }
        public void Dispose()
        {
            _stream?.Dispose();
        }
        public ValueTask DisposeAsync()
        {
            return _stream?.DisposeAsync() ?? ValueTask.CompletedTask;
        }
    }
}