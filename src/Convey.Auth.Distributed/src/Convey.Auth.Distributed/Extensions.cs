using Convey.Persistence.Redis;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.Auth.Distributed
{
    public static class Extensions
    {
        private const string RedisSectionName = "redis";
        private const string RegistryName = "auth.distributed";

        public static IConveyBuilder AddDistributedAccessTokenValidator(this IConveyBuilder builder,
            string redisSectionName = RedisSectionName)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            builder.Services.AddSingleton<IAccessTokenService, DistributedAccessTokenService>();
            builder.AddRedis(redisSectionName);

            return builder;
        }
    }
}