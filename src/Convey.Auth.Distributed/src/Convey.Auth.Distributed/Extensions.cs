using Microsoft.Extensions.DependencyInjection;

namespace Convey.Auth.Distributed
{
    public static class Extensions
    {
        private const string RegistryName = "auth.distributed";

        public static IConveyBuilder AddDistributedAccessTokenValidator(this IConveyBuilder builder)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            builder.Services.AddSingleton<IAccessTokenService, DistributedAccessTokenService>();

            return builder;
        }
    }
}