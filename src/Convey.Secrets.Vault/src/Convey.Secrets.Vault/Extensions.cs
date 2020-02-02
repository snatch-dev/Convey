using System;
using Convey.Secrets.Vault.Stores;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.JWT;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods.UserPass;

namespace Convey.Secrets.Vault
{
    public static class Extensions
    {
        private const string SectionName = "vault";

        public static IWebHostBuilder UseVault(this IWebHostBuilder builder, string key = null,
            string sectionName = SectionName)
            => builder.ConfigureServices(services =>
                {
                    if (string.IsNullOrWhiteSpace(sectionName))
                    {
                        sectionName = SectionName;
                    }

                    IConfiguration configuration;
                    using (var serviceProvider = services.BuildServiceProvider())
                    {
                        configuration = serviceProvider.GetService<IConfiguration>();
                    }

                    var options = configuration.GetOptions<VaultOptions>(sectionName);
                    if (!options.Enabled)
                    {
                        return;
                    }

                    services.AddSingleton(options);
                    services.AddTransient<IKeyValueVaultStore, KeyValueVaultStore>();
                    var (client, settings) = GetClientAndSettings(options);
                    services.AddSingleton(settings);
                    services.AddSingleton(client);
                })
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    var options = cfg.Build().GetOptions<VaultOptions>(sectionName);
                    if (!options.Enabled)
                    {
                        return;
                    }

                    Console.WriteLine($"Loading settings from Vault: {options.Url}");
                    cfg.AddVault(options, key);
                });

        private static void AddVault(this IConfigurationBuilder builder, VaultOptions options, string key)
        {
            var (client, _) = GetClientAndSettings(options);
            var keyValueVaultStore = new KeyValueVaultStore(client, options);
            var secret = string.IsNullOrWhiteSpace(key)
                ? keyValueVaultStore.GetDefaultAsync().GetAwaiter().GetResult()
                : keyValueVaultStore.GetAsync(key).GetAwaiter().GetResult();
            var parser = new JsonParser();
            var data = parser.Parse(JObject.FromObject(secret));
            var source = new MemoryConfigurationSource {InitialData = data};
            builder.Add(source);
        }

        private static (IVaultClient client, VaultClientSettings settings) GetClientAndSettings(VaultOptions options)
        {
            var settings = new VaultClientSettings(options.Url, GetAuthMethod(options));
            var client = new VaultClient(settings);

            return (client, settings);
        }

        private static IAuthMethodInfo GetAuthMethod(VaultOptions options)
            => options.AuthType?.ToLowerInvariant() switch
            {
                "token" => new TokenAuthMethodInfo(options.Token),
                "userpass" => new UserPassAuthMethodInfo(options.Username, options.Password),
                _ => throw new VaultAuthTypeNotSupportedException(
                    $"Vault auth type: '{options.AuthType}' is not supported.", options.AuthType)
            };
    }
}