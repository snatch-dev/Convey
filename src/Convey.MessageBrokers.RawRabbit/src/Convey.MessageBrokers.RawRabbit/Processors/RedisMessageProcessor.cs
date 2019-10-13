using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Convey.MessageBrokers.RawRabbit.Processors
{
    public class RedisMessageProcessor : IMessageProcessor
    {
        private readonly IDistributedCache _cache;
        private readonly RabbitMqOptions _options;

        public RedisMessageProcessor(IDistributedCache cache, RabbitMqOptions options)
        {
            _cache = cache;
            _options = options;
        }

        public async Task<bool> TryProcessAsync(string id)
        {
            var key = GetKey(_options.Namespace, id);
            var message = await _cache.GetStringAsync(key);
            if (!string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            var expiry = _options.MessageProcessor?.MessageExpirySeconds ?? 0;
            await _cache.SetStringAsync(key, id, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiry)
            });

            return true;
        }

        public Task RemoveAsync(string id) => _cache.RemoveAsync(GetKey(_options.Namespace, id));

        private static string GetKey(string @namespace, string id) => $"messages:{@namespace}:{id}";
    }
}