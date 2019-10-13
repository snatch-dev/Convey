using System;
using Convey.Persistence.Redis.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.Persistence.Redis
{
    public static class Extensions
    {
        private const string SectionName = "redis";
        private const string RegistryName = "persistence.redis";

        public static IConveyBuilder AddRedis(this IConveyBuilder builder, string sectionName = SectionName)
        {
            var options = builder.GetOptions<RedisOptions>(sectionName);
            return builder.AddRedis(options);
        }
        
        public static IConveyBuilder AddRedis(this IConveyBuilder builder, Func<IRedisOptionsBuilder, IRedisOptionsBuilder> buildOptions)
        {
            var options = buildOptions(new RedisOptionsBuilder()).Build();
            return builder.AddRedis(options);
        }
        
        public static IConveyBuilder AddRedis(this IConveyBuilder builder, RedisOptions options)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }
            
            builder.Services.AddDistributedRedisCache(o => 
            {
                o.Configuration = options.ConnectionString;
                o.InstanceName = options.Instance;
            });

            return builder;
        }
    }
}