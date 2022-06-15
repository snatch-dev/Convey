
using Convey.Secrets.Vault;
using Convey.Secrets.Vault.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;

namespace Convey.Secrets.Vault.Internals
{
    internal class VaultKeyValueConfigurationProvider: ConfigurationProvider
    {
        private readonly VaultKeyValueConfigurationSource _source;
        private readonly IVaultClient _client;
        private readonly VaultOptions _options;
        private readonly IDisposable _changeTokenRegistration;

        public VaultKeyValueConfigurationProvider(VaultKeyValueConfigurationSource source)
        {
            _source = source;
            _client = source.Client;
            _options = source.Options;

            if (_source.PeriodicalWatcher != null)
            {
                _changeTokenRegistration = ChangeToken.OnChange(
                    () => _source.PeriodicalWatcher.Watch(),
                    Load
                );
            }
        }


        public override void Load()
        {
            var kvPaths =  _options.Kv?.Paths;
            if(kvPaths is null || kvPaths.Count() == 0)
            {
                kvPaths.Add(_options.Kv.Path);
            }
            JObject kvConfiguration = new JObject();
            foreach (var kvPath in kvPaths)
            {
                if (!string.IsNullOrWhiteSpace(kvPath) && _options.Kv.Enabled)
                {
                    Console.WriteLine($"Loading settings from Vault: '{_options.Url}', KV path: '{kvPath}'.");
                    var keyValueSecrets = new KeyValueSecrets(_client, _options);
                    var secret =  keyValueSecrets.GetAsync(kvPath).GetAwaiter().GetResult();
                    kvConfiguration.Merge(JObject.FromObject(secret));

                }
            }
            Data = new JsonParser().Parse(kvConfiguration.ToString());
        }
    }
}
