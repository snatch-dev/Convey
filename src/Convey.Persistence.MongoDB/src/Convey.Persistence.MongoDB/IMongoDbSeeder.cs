using System.Threading.Tasks;
using MongoDB.Driver;

namespace Convey.Persistence.MongoDB
{
    public interface IMongoDbSeeder
    {
        Task SeedAsync(IMongoDatabase database);
    }
}