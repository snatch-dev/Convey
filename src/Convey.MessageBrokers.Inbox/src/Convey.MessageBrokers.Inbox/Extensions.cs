using Convey.MessageBrokers.Inbox.Inbox;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.MessageBrokers.Inbox
{
    public static class Extensions
    {
        private const string SectionName = "inbox";
        private const string RegistryName = "messageBrokers.inbox";

        public static IConveyBuilder AddMessageInbox(this IConveyBuilder builder, string sectionName = SectionName)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            var options = builder.GetOptions<InboxOptions>(sectionName);
            builder.Services.AddSingleton(options);

            switch (options.Type?.ToLowerInvariant() ?? string.Empty)
            {
                case "memory":
                    builder.Services.AddTransient<IMessageInbox, InMemoryMessageInbox>();
                    break;
                case "mongo":
                    builder.Services.AddTransient<IMessageInbox, MongoMessageInbox>();
                    builder.Services.AddTransient<MongoInboxInitializer>();
                    builder.AddInitializer<MongoInboxInitializer>();
                    break;
                case "redis":
                    builder.Services.AddTransient<IMessageInbox, RedisMessageInbox>();
                    break;
                default:
                    builder.Services.AddTransient<IMessageInbox, InMemoryMessageInbox>();
                    break;
            }

            return builder;
        }
    }
}