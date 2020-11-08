using System;
using Convey.CQRS.Queries.Dispatchers;
using Convey.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.CQRS.Queries
{
    public static class Extensions
    {
        public static IConveyBuilder AddQueryHandlers(this IConveyBuilder builder)
        {
            builder.Services.Scan(s =>
                s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>))
                        .WithoutAttribute(typeof(DecoratorAttribute)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            return builder;
        }

        public static IConveyBuilder AddInMemoryQueryDispatcher(this IConveyBuilder builder)
        {
            builder.Services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
            return builder;
        }
    }
}