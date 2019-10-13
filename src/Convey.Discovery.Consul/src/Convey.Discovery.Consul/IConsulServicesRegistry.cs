using System.Threading.Tasks;
using Consul;

namespace Convey.Discovery.Consul
{
    public interface IConsulServicesRegistry
    {
        Task<AgentService> GetAsync(string name);
    }
}