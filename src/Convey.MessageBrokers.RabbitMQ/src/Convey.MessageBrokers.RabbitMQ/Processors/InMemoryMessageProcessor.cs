using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Convey.MessageBrokers.RabbitMQ.Processors
{
    internal sealed class InMemoryMessageProcessor : IMessageProcessor
    {
        private readonly IMemoryCache _cache;
        private readonly RabbitMqOptions _options;

        public InMemoryMessageProcessor(IMemoryCache cache, RabbitMqOptions options)
        {
            _cache = cache;
            _options = options;
        }

        public Task<bool> TryProcessAsync(string id)
        {
            var key = GetKey(_options.Exchange?.Name, id);
            if (_cache.TryGetValue(key, out _))
            {
                return Task.FromResult(false);
            }

            var expiry = _options.MessageProcessor?.MessageExpirySeconds ?? 0;
            _cache.Set(key, id, TimeSpan.FromSeconds(expiry));

            return Task.FromResult(true);
        }

        public Task RemoveAsync(string id)
        {
            _cache.Remove(GetKey(_options.Exchange?.Name, id));

            return Task.CompletedTask;
        }

        private static string GetKey(string exchange, string id)
            => $"messages:{(string.IsNullOrWhiteSpace(exchange) ? id : $"{exchange}:{id}")}";
    }
}