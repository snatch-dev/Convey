using Convey.Secrets.Vault;
using Convey.Secrets.Vault.Internals;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;

namespace Convey.Secrets.Vault.Internals
{
    internal class VaultKeyValueConfigurationSource : IConfigurationSource
    {
        public VaultOptions Options { get; set; }
        public IVaultClient Client { get; set; }
        public IVaultPeriodicalWatcher PeriodicalWatcher { get; set; }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            
            return new VaultKeyValueConfigurationProvider(this);
        }
    }
}
