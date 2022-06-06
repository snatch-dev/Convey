using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;

namespace Convey.Secrets.Vault.Internals
{
    internal static  class VaultKeyValueConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddVaultKeyValueConfiguration(this IConfigurationBuilder builder,
           VaultOptions options,
           IVaultClient client)
        {
            IVaultPeriodicalWatcher watcher = null;
            if (options.Kv.AutoRenewal)
            {
                watcher = new VaultPeriodicalWatcher(TimeSpan.FromSeconds(options.RenewalsInterval));
            }

            return builder.Add(new VaultKeyValueConfigurationSource()
            {
                Options = options,
                Client = client,
                PeriodicalWatcher = watcher
            });
        }
    }
}
