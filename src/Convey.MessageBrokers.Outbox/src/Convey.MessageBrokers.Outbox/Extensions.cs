using Convey.MessageBrokers.Outbox.Outbox;
using Convey.MessageBrokers.Outbox.Processors;
using Convey.Persistence.MongoDB;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.MessageBrokers.Outbox
{
    public static class Extensions
    {
        private const string SectionName = "outbox";
        private const string RegistryName = "messageBrokers.outbox";

        public static IConveyBuilder AddMessageOutbox(this IConveyBuilder builder, string sectionName = SectionName)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            var options = builder.GetOptions<OutboxOptions>(sectionName);
            builder.Services.AddSingleton(options);
            if (!options.Enabled)
            {
                builder.RegisterInMemoryOutbox();
                return builder;
            }

            var inboxCollection = string.IsNullOrWhiteSpace(options.InboxCollection)
                ? "inbox"
                : options.InboxCollection;
            var outboxCollection = string.IsNullOrWhiteSpace(options.OutboxCollection)
                ? "outbox"
                : options.OutboxCollection;
            switch (options.Type?.ToLowerInvariant())
            {
                default:
                    builder.RegisterInMemoryOutbox();
                    break;
                case "mongo":
                    builder.AddMongo();
                    builder.AddMongoRepository<InboxMessage, string>(inboxCollection);
                    builder.AddMongoRepository<OutboxMessage, string>(outboxCollection);
                    builder.Services.AddTransient<IMessageOutbox, MongoMessageOutbox>();
                    builder.Services.AddTransient<MongoOutboxInitializer>();
                    builder.AddInitializer<MongoOutboxInitializer>();
                    break;
            }
            
            builder.Services.AddHostedService<OutboxProcessor>();

            return builder;
        }
        
        private static void RegisterInMemoryOutbox(this IConveyBuilder builder)
            => builder.Services.AddTransient<IMessageOutbox, InMemoryMessageOutbox>();
    }
}