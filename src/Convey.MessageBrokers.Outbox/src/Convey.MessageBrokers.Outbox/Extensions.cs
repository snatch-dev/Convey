using System;
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
            builder.AddMongo();
            builder.AddMongoRepository<OutboxMessage, Guid>("outbox");
            builder.Services.AddTransient<IMessageOutbox, MongoMessageOutbox>();
            builder.Services.AddHostedService<OutboxProcessor>();
            return builder;
        }
    }
}