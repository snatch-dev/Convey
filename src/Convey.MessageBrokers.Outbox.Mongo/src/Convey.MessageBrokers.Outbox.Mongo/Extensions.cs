using Convey.MessageBrokers.Outbox.Messages;
using Convey.MessageBrokers.Outbox.Mongo.Internals;
using Convey.Persistence.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Convey.MessageBrokers.Outbox.Mongo
{
    public static class Extensions
    {
        public static IMessageOutboxConfigurator AddMongo(this IMessageOutboxConfigurator configurator,
            string mongoSectionName = null)
        {
            var builder = configurator.Builder;
            var options = configurator.Options;

            var inboxCollection = string.IsNullOrWhiteSpace(options.InboxCollection)
                ? "inbox"
                : options.InboxCollection;
            var outboxCollection = string.IsNullOrWhiteSpace(options.OutboxCollection)
                ? "outbox"
                : options.OutboxCollection;

            builder.AddMongo(mongoSectionName);
            builder.AddMongoRepository<InboxMessage, string>(inboxCollection);
            builder.AddMongoRepository<OutboxMessage, string>(outboxCollection);
            builder.Services.AddTransient<IMessageOutbox, MongoMessageOutbox>();
            builder.Services.AddTransient<MongoOutboxInitializer>();
            builder.AddInitializer<MongoOutboxInitializer>();

            BsonClassMap.RegisterClassMap<OutboxMessage>(m =>
            {
                m.AutoMap();
                m.UnmapMember(p => p.Message);
                m.UnmapMember(p => p.MessageContext);
            });

            return configurator;
        }
    }
}