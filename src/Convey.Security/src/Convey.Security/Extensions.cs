using Convey.Security.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.Security
{
    public static class Extensions
    {
        public static IConveyBuilder AddSecurity(this IConveyBuilder builder)
        {
            builder.Services
                .AddSingleton<IEncryptor, Encryptor>()
                .AddSingleton<IHasher, Hasher>()
                .AddSingleton<ISigner, Signer>();

            return builder;
        }
    }
}