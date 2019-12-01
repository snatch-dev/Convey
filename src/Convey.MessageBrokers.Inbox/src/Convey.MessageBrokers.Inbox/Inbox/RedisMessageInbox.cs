using System;
using System.Threading.Tasks;
using Convey.Persistence.Redis;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Convey.MessageBrokers.Inbox.Inbox
{
    internal sealed class RedisMessageInbox : IMessageInbox
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisMessageInbox> _logger;
        private readonly bool _enabled;
        private readonly int _expiry;
        private readonly string _instance;

        public RedisMessageInbox(IDatabase database, InboxOptions inboxOptions, RedisOptions redisOptions,
            ILogger<RedisMessageInbox> logger)
        {
            if (string.IsNullOrWhiteSpace(redisOptions.Instance))
            {
                throw new ArgumentException("Redis instance cannot be empty.", nameof(redisOptions.Instance));
            }

            _database = database;
            _logger = logger;
            _enabled = inboxOptions.Enabled;
            _expiry = inboxOptions.MessageExpirySeconds;
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
            var existingMessage = await _database.StringGetAsync(key);
            if (!string.IsNullOrWhiteSpace(existingMessage))
            {
                _logger.LogTrace($"A unique message with id: '{messageId}' was already processed.");

                return false;
            }

            _logger.LogTrace($"Processing a unique message with id: '{messageId}'...");
            var transaction = _database.CreateTransaction();
            await handle();
            if (_expiry <= 0)
            {
                await transaction.StringSetAsync(key, key);
            }
            else
            {
                await transaction.StringSetAsync(key, key, TimeSpan.FromSeconds(_expiry));
            }

            _logger.LogTrace($"Processed a unique message with id: '{messageId}'.");

            return await transaction.ExecuteAsync();
        }

        private string GetKey(string messageId) => $"{_instance}messages:{messageId}";
    }
}