using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.Inbox.Inbox
{
    internal sealed class InMemoryMessageInbox : IMessageInbox
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<InMemoryMessageInbox> _logger;
        private readonly bool _enabled;
        private readonly int _expiry;

        public InMemoryMessageInbox(IMemoryCache cache, InboxOptions options, ILogger<InMemoryMessageInbox> logger)
        {
            _cache = cache;
            _logger = logger;
            _enabled = options.Enabled;
            _expiry = options.MessageExpirySeconds;
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
            if (_cache.TryGetValue(key, out _))
            {
                _logger.LogTrace($"A unique message with id: '{messageId}' was already processed.");
                
                return true;
            }

            _logger.LogTrace($"Processing a unique message with id: '{messageId}'...");
            await handle();
            _logger.LogTrace($"Processed a unique message with id: '{messageId}'.");
            
            if (_expiry <= 0)
            {
                _cache.Set(key, messageId);
            }
            else
            {
                _cache.Set(key, messageId, TimeSpan.FromSeconds(_expiry));
            }
            
            return true;
        }

        private static string GetKey(string messageId) => $"messages:{messageId}";
    }
}