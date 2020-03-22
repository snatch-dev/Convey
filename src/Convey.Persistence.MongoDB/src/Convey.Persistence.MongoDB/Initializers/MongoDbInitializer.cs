using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Convey.Persistence.MongoDB.Initializers
{
    internal sealed class MongoDbInitializer : IMongoDbInitializer
    {
        private static int _initialized;
        private readonly bool _seed;
        private readonly IMongoDatabase _database;
        private readonly IMongoDbSeeder _seeder;

        public MongoDbInitializer(IMongoDatabase database, IMongoDbSeeder seeder, MongoDbOptions options)
        {
            _database = database;
            _seeder = seeder;
            _seed = options.Seed;
        }

        public Task InitializeAsync()
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 1)
            {
                return Task.CompletedTask;
            }

            return _seed ? _seeder.SeedAsync(_database) : Task.CompletedTask;
        }
    }
}