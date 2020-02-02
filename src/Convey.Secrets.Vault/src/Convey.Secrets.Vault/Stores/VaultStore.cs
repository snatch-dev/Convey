using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods.UserPass;

namespace Convey.Secrets.Vault.Stores
{
    public class VaultStore : IVaultStore
    {
        private readonly VaultOptions _options;

        public VaultStore(VaultOptions options)
        {
            _options = options;
            LoadEnvironmentVariables();
        }

        public async Task<T> GetDefaultAsync<T>()
            => await GetAsync<T>(_options.Key);

        public async Task<IDictionary<string, object>> GetDefaultAsync()
            => await GetAsync(_options.Key);

        public async Task<T> GetAsync<T>(string key)
            => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject((await GetAsync(key))));

        public async Task<IDictionary<string, object>> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new VaultException("Vault secret key can not be empty.");            
            }
            try
            {
                var settings = new VaultClientSettings(_options.Url, GetAuthMethod());
                var client = new VaultClient(settings);
                var secret = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(key);

                return secret.Data.Data;
            }
            catch (Exception exception)
            {
                throw new VaultException($"Getting Vault secret for key: '{key}' caused an error. " +
                    $"{exception.Message}", exception, key);
            }
        }

        private IAuthMethodInfo GetAuthMethod()
        {
            switch(_options.AuthType?.ToLowerInvariant())
            {
                case "token": return new TokenAuthMethodInfo(_options.Token);
                case "userpass": return new UserPassAuthMethodInfo(_options.Username, _options.Password);
            }

            throw new VaultAuthTypeNotSupportedException($"Vault auth type: '{_options.AuthType}' is not supported.",
                _options.AuthType);
        }

        private void LoadEnvironmentVariables()
        {
            _options.Url = GetEnvironmentVariableValue("VAULT_URL") ?? _options.Url;
            _options.Key = GetEnvironmentVariableValue("VAULT_KEY") ?? _options.Key;
            _options.AuthType = GetEnvironmentVariableValue("VAULT_AUTH_TYPE") ?? _options.AuthType;
            _options.Token = GetEnvironmentVariableValue("VAULT_TOKEN") ?? _options.Token;
            _options.Username = GetEnvironmentVariableValue("VAULT_USERNAME") ?? _options.Username;
            _options.Password = GetEnvironmentVariableValue("VAULT_PASSWORD") ?? _options.Password;
        }

        private static string GetEnvironmentVariableValue(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}