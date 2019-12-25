using Convey.MessageBrokers.Outbox.EntityFramework.Internals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.MessageBrokers.Outbox.EntityFramework
{
    public static class Extensions
    {
        public static IMessageOutboxConfigurator AddEntityFramework<T>(this IMessageOutboxConfigurator configurator)
            where T : DbContext
        {
            var builder = configurator.Builder;

            builder.Services.AddDbContext<T>();
            builder.Services.AddTransient<IMessageOutbox, EntityFrameworkMessageOutbox<T>>();
            builder.Services.AddTransient<IMessageOutboxAccessor, EntityFrameworkMessageOutbox<T>>();

            return configurator;
        }
    }
}