using System;
using Convey.CQRS.Events.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.CQRS.Events
{
    public static class Extensions
    {
        public static IConveyBuilder AddEventHandlers(this IConveyBuilder builder)
        {
            builder.Services.Scan(s =>
                s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            return builder;
        }
        
        public static IConveyBuilder AddInMemoryEventDispatcher(this IConveyBuilder builder)
        {
            builder.Services.AddSingleton<IEventDispatcher, EventDispatcher>();
            return builder;
        }
    }
}