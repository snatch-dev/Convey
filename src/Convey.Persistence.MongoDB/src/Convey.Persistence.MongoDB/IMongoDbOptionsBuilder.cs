namespace Convey.Persistence.MongoDB
{
    public interface IMongoDbOptionsBuilder
    {
        IMongoDbOptionsBuilder WithConnectionString(string connectionString);
        IMongoDbOptionsBuilder WithDatabase(string database);
        IMongoDbOptionsBuilder WithSeed(bool seed);
        MongoDbOptions Build();
    }
}