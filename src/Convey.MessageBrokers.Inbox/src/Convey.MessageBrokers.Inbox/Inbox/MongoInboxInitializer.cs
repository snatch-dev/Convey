using System;
using System.Threading.Tasks;
using Convey.Types;
using MongoDB.Driver;

namespace Convey.MessageBrokers.Inbox.Inbox
{
    internal sealed class MongoInboxInitializer : IInitializer
    {
        private readonly IMongoDatabase _database;
        private readonly InboxOptions _options;

        public MongoInboxInitializer(IMongoDatabase database, InboxOptions options)
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

            if (_options.MessageExpirySeconds <= 0)
            {
                return;
            }

            var builder = Builders<MongoMessageInbox.InboxMessage>.IndexKeys;
            await _database.GetCollection<MongoMessageInbox.InboxMessage>("inbox")
                .Indexes.CreateOneAsync(
                    new CreateIndexModel<MongoMessageInbox.InboxMessage>(builder.Ascending(i => i.CreatedAt),
                        new CreateIndexOptions
                        {
                            ExpireAfter = TimeSpan.FromSeconds(_options.MessageExpirySeconds)
                        }));
        }
    }
}