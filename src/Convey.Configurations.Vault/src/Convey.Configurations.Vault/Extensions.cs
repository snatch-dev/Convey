using System;
using Convey.Configurations.Vault.Stores;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Convey.Configurations.Vault
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
                    services.AddTransient<IVaultStore, VaultStore>();
                })
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    var options = cfg.Build().GetOptions<VaultOptions>(sectionName);
                    var enabled = options.Enabled;
                    var vaultEnabled = Environment.GetEnvironmentVariable("VAULT_ENABLED")?.ToLowerInvariant();
                    if (!string.IsNullOrWhiteSpace(vaultEnabled))
                    {
                        enabled = vaultEnabled == "true" || vaultEnabled == "1";
                    }

                    if (!enabled)
                    {
                        return;
                    }

                    Console.WriteLine($"Loading settings from Vault: {options.Url}");
                    cfg.AddVault(options, key);
                });

        private static void AddVault(this IConfigurationBuilder builder, VaultOptions options, string key)
        {
            var client = new VaultStore(options);
            var secret = string.IsNullOrWhiteSpace(key)
                ? client.GetDefaultAsync().GetAwaiter().GetResult()
                : client.GetAsync(key).GetAwaiter().GetResult();
            var parser = new JsonParser();
            var data = parser.Parse(JObject.FromObject(secret));
            var source = new MemoryConfigurationSource {InitialData = data};
            builder.Add(source);
        }
    }
}