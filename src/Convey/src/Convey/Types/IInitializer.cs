using System.Threading.Tasks;

namespace Convey.Types
{
    public interface IInitializer
    {
        Task InitializeAsync();
    }
}