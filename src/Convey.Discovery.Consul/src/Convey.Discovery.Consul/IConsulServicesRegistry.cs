using System.Threading.Tasks;
using Convey.Discovery.Consul.Models;

namespace Convey.Discovery.Consul
{
    public interface IConsulServicesRegistry
    {
        Task<ServiceAgent> GetAsync(string name);
    }
}