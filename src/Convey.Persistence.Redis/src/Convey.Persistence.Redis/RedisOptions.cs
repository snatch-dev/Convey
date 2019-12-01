namespace Convey.Persistence.Redis
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; } = "localhost";
        public string Instance { get; set; }
    }
}