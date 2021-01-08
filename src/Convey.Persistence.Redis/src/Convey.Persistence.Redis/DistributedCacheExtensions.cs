using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Convey.Persistence.Redis
{
    public static class DistributedCacheExtensions
    {
        public static void Set<T>(this IDistributedCache cache, string key, T value)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var valueBytes = Encoding.UTF8.GetBytes(serializedValue);

            cache.Set(key, valueBytes);
        }
        public static void Set<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var valueBytes = Encoding.UTF8.GetBytes(serializedValue);

            cache.Set(key, valueBytes, options);
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, CancellationToken token = default)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var valueBytes = Encoding.UTF8.GetBytes(serializedValue);

            return cache.SetAsync(key, valueBytes, token);
        }
        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var valueBytes = Encoding.UTF8.GetBytes(serializedValue);

            return cache.SetAsync(key, valueBytes, options, token);
        }

        public static T Get<T>(this IDistributedCache cache, string key)
        {
            var cachedValue = cache.Get(key);
            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken token = default)
        {
            var cachedValue = await cache.GetAsync(key, token);
            return JsonSerializer.Deserialize<T>(cachedValue);
        }
    }
}
