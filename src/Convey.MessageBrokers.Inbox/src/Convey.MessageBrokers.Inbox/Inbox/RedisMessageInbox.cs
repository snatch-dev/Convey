using System;
using System.Threading.Tasks;
using Convey.Persistence.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.Inbox.Inbox
{
    internal sealed class RedisMessageInbox : IMessageInbox
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisMessageInbox> _logger;
        private readonly bool _enabled;
        private readonly int _expiry;
        private readonly string _instance;

        public RedisMessageInbox(IDistributedCache cache, InboxOptions inboxOptions, RedisOptions redisOptions,
            ILogger<RedisMessageInbox> logger)
        {
            if (string.IsNullOrWhiteSpace(redisOptions.Instance))
            {
                throw new ArgumentException("Redis instance cannot be empty.", nameof(redisOptions.Instance));
            }

            _cache = cache;
            _logger = logger;
            _enabled = inboxOptions.Enabled;
            _expiry = inboxOptions.ExpirySeconds;
            _instance = redisOptions.Instance;
        }

        public async Task<bool> TryProcessAsync(string messageId, Func<Task> handle)
        {
            if (!_enabled)
            {
                _logger.LogTrace("Inbox is disabled, processing a message as usual...");
                await handle();
                _logger.LogTrace("Message has been processed.");
                return true;
            }

            if (string.IsNullOrWhiteSpace(messageId))
            {
                _logger.LogTrace("Message id is empty, processing a message as usual...");
                await handle();
                _logger.LogTrace("Message has been processed.");
                return true;
            }

            _logger.LogTrace($"Received a unique message with id: '{messageId}' to be processed.");

            var key = GetKey(messageId);
            var existingMessage = await _cache.GetStringAsync(key);
            if (!string.IsNullOrWhiteSpace(existingMessage))
            {
                _logger.LogTrace($"A unique message with id: '{messageId}' was already processed.");

                return false;
            }

            _logger.LogTrace($"Processing a unique message with id: '{messageId}'...");
            await handle();
            if (_expiry <= 0)
            {
                await _cache.SetStringAsync(key, messageId);
            }
            else
            {
                await _cache.SetStringAsync(key, messageId, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_expiry)
                });
            }

            _logger.LogTrace($"Processed a unique message with id: '{messageId}'.");

            return true;
        }

        private string GetKey(string messageId) => $"{_instance}messages:{messageId}";
    }
}