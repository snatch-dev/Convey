using System.Threading.Tasks;
using MongoDB.Driver;

namespace Convey.Persistence.MongoDB
{
    public interface IMongoSessionFactory
    {
        Task<IClientSessionHandle> CreateAsync();
    }
}