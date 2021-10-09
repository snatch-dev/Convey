using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Convey.Persistence.Fs.Seaweed.Infrastructure
{
    public interface IFiler : IOperator
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage);
        Task<Stream> GetStreamAsync(HttpRequestMessage httpRequestMessage);
    }
}