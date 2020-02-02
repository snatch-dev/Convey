using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VaultSharp;

namespace Convey.Secrets.Vault.Stores
{
    internal sealed class KeyValueVaultStore : IKeyValueVaultStore
    {
        private readonly IVaultClient _client;
        private readonly VaultOptions _options;

        public KeyValueVaultStore(IVaultClient client, VaultOptions options)
        {
            _client = client;
            _options = options;
        }

        public async Task<T> GetDefaultAsync<T>()
            => await GetAsync<T>(_options.Key);

        public async Task<IDictionary<string, object>> GetDefaultAsync()
            => await GetAsync(_options.Key);

        public async Task<T> GetAsync<T>(string key)
            => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(await GetAsync(key)));

        public async Task<IDictionary<string, object>> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new VaultException("Vault secret key can not be empty.");
            }

            try
            {
                var secret = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(key);

                return secret.Data.Data;
            }
            catch (Exception exception)
            {
                throw new VaultException($"Getting Vault secret for key: '{key}' caused an error. " +
                                         $"{exception.Message}", exception, key);
            }
        }
    }
}