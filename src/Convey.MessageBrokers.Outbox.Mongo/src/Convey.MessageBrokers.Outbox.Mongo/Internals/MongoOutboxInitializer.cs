using System;
using System.Threading.Tasks;
using Convey.MessageBrokers.Outbox.Messages;
using Convey.Types;
using MongoDB.Driver;

namespace Convey.MessageBrokers.Outbox.Mongo.Internals
{
    internal sealed class MongoOutboxInitializer : IInitializer
    {
        private readonly IMongoDatabase _database;
        private readonly OutboxOptions _options;

        public MongoOutboxInitializer(IMongoDatabase database, OutboxOptions options)
        {
            _database = database;
            _options = options;
        }

        public async Task InitializeAsync()
        {
            if (!_options.Enabled)
            {
                return;
            }

            if (_options.Expiry <= 0)
            {
                return;
            }

            var inboxCollection = string.IsNullOrWhiteSpace(_options.InboxCollection)
                ? "inbox"
                : _options.InboxCollection;
            var builder = Builders<InboxMessage>.IndexKeys;
            await _database.GetCollection<InboxMessage>(inboxCollection)
                .Indexes.CreateOneAsync(
                    new CreateIndexModel<InboxMessage>(builder.Ascending(i => i.ProcessedAt),
                        new CreateIndexOptions
                        {
                            ExpireAfter = TimeSpan.FromSeconds(_options.Expiry)
                        }));

            var outboxCollection = string.IsNullOrWhiteSpace(_options.OutboxCollection)
                ? "outbox"
                : _options.OutboxCollection;
            var outboxBuilder = Builders<OutboxMessage>.IndexKeys;
            await _database.GetCollection<OutboxMessage>(outboxCollection)
                .Indexes.CreateOneAsync(
                    new CreateIndexModel<OutboxMessage>(outboxBuilder.Ascending(i => i.ProcessedAt),
                        new CreateIndexOptions
                        {
                            ExpireAfter = TimeSpan.FromSeconds(_options.Expiry)
                        }));
        }
    }
}