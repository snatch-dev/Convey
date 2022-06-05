using Dylan.Convey.Secrets.Vault;
using Dylan.Convey.Secrets.Vault.Internals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;

namespace Dylan.Convey.Secrets.Vault.Internals
{
    internal class KeyValueConfigurationManager
    {
        public string FileName => "vault-kv.json";
        private readonly IVaultClient _client;
        private readonly VaultOptions _options;

        public KeyValueConfigurationManager(IVaultClient client, VaultOptions options)
        {
            _client = client;
            _options = options;
        }

        public async Task UpdateConfiguration(string keyValuePath = null)
        {
            var kvPaths = string.IsNullOrWhiteSpace(keyValuePath) ? _options.Kv?.Paths : new List<string> { keyValuePath };
            JObject kvConfiguration = new JObject();
            foreach (var kvPath in kvPaths)
            {
                if (!string.IsNullOrWhiteSpace(kvPath) && _options.Kv.Enabled)
                {
                    Console.WriteLine($"Loading settings from Vault: '{_options.Url}', KV path: '{kvPath}'.");
                    var keyValueSecrets = new KeyValueSecrets(_client, _options);
                    var secret = await keyValueSecrets.GetAsync(kvPath);
                    kvConfiguration.Merge( JObject.FromObject(secret));

                }
            }
            File.WriteAllText(FileName, kvConfiguration.ToString());
        }
    }
}
