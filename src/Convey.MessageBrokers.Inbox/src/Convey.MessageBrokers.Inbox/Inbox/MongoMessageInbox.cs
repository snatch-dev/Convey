using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Convey.MessageBrokers.Inbox.Inbox
{
    internal sealed class MongoMessageInbox : IMessageInbox
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoClient _client;
        private readonly ILogger<MongoMessageInbox> _logger;
        private readonly bool _enabled;

        public MongoMessageInbox(IMongoDatabase database, IMongoClient client, InboxOptions options,
            ILogger<MongoMessageInbox> logger)
        {
            _database = database;
            _client = client;
            _logger = logger;
            _enabled = options.Enabled;
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

            var existingMessage = await Inbox.Find(m => m.Id == messageId).SingleOrDefaultAsync();
            if (existingMessage is {})
            {
                _logger.LogTrace($"A unique message with id: '{messageId}' was already processed.");

                return false;
            }

            _logger.LogTrace($"Processing a unique message with id: '{messageId}'...");
            var session = await _client.StartSessionAsync();
            session.StartTransaction();
            await handle();
            await Inbox.InsertOneAsync(new InboxMessage
            {
                Id = messageId,
                CreatedAt = DateTime.UtcNow
            });
            await session.CommitTransactionAsync();
            _logger.LogTrace($"Processed a unique message with id: '{messageId}'.");

            return true;
        }

        private IMongoCollection<InboxMessage> Inbox => _database.GetCollection<InboxMessage>("inbox");

        internal sealed class InboxMessage
        {
            public string Id { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}