using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Convey.MessageBrokers.RabbitMQ.Processors
{
    internal sealed class InMemoryMessageProcessor : IMessageProcessor
    {
        private readonly IMemoryCache _cache;
        private readonly RabbitMqOptions _options;
        private readonly string _service;

        public InMemoryMessageProcessor(IMemoryCache cache, RabbitMqOptions options)
        {
            _cache = cache;
            _options = options;
            _service = string.IsNullOrWhiteSpace(options.ConnectionName)
                ? Guid.NewGuid().ToString("N")
                : options.ConnectionName;
        }

        public Task<bool> TryProcessAsync(string id)
        {
            var key = GetKey(id);
            if (_cache.TryGetValue(key, out _))
            {
                return Task.FromResult(false);
            }

            var expiry = _options.MessageProcessor?.MessageExpirySeconds ?? 0;
            if (expiry <= 0)
            {
                _cache.Set(key, id);
            }
            else
            {
                _cache.Set(key, id, TimeSpan.FromSeconds(expiry));
            }

            return Task.FromResult(true);
        }

        public Task RemoveAsync(string id)
        {
            _cache.Remove(GetKey(id));

            return Task.CompletedTask;
        }

        private string GetKey(string id) => $"messages:{_service}:{id}";
    }
}