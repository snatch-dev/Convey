using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Convey.MessageBrokers.RawRabbit.Processors;

internal sealed class RedisMessageProcessor : IMessageProcessor
{
    private readonly IDistributedCache _cache;
    private readonly RabbitMqOptions _options;
    private readonly string _service;

    public RedisMessageProcessor(IDistributedCache cache, RabbitMqOptions options)
    {
        _cache = cache;
        _options = options;
        _service = string.IsNullOrWhiteSpace(options.Namespace)
            ? Guid.NewGuid().ToString("N")
            : options.Namespace;
    }

    public async Task<bool> TryProcessAsync(string id)
    {
        var key = GetKey(id);
        var message = await _cache.GetStringAsync(key);
        if (!string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        var expiry = _options.MessageProcessor?.MessageExpirySeconds ?? 0;
        if (expiry <= 0)
        {
            await _cache.SetStringAsync(key, id);
        }
        else
        {
            await _cache.SetStringAsync(key, id, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiry)
            });
        }

        return true;
    }

    public Task RemoveAsync(string id) => _cache.RemoveAsync(GetKey(id));

    private string GetKey(string id) => $"messages:{_service}:{id}";
}