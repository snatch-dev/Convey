using System.IO;
using System.Threading.Tasks;

namespace Convey.HTTP;

public interface IHttpClientSerializer
{
    string Serialize<T>(T value);
    ValueTask<T> DeserializeAsync<T>(Stream stream);
}