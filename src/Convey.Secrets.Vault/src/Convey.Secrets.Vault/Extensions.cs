using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convey.Secrets.Vault.Internals;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods.UserPass;
using VaultSharp.V1.SecretsEngines;

namespace Convey.Secrets.Vault
{
    public static class Extensions
    {
        private const string SectionName = "vault";
        private static readonly ILeaseService LeaseService = new LeaseService();
        private static readonly ICertificatesService CertificatesService = new CertificatesService();

        public static IHostBuilder UseVault(this IHostBuilder builder, string keyValuePath = null,
            string sectionName = SectionName)
            => builder.ConfigureServices(services => services.AddVault(sectionName))
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    var options = cfg.Build().GetOptions<VaultOptions>(sectionName);
                    if (!options.Enabled)
                    {
                        return;
                    }

                    cfg.AddVaultAsync(options, keyValuePath).GetAwaiter().GetResult();
                });
        
        public static IWebHostBuilder UseVault(this IWebHostBuilder builder, string keyValuePath = null,
            string sectionName = SectionName)
            => builder.ConfigureServices(services => services.AddVault(sectionName))
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    var options = cfg.Build().GetOptions<VaultOptions>(sectionName);
                    if (!options.Enabled)
                    {
                        return;
                    }

                    cfg.AddVaultAsync(options, keyValuePath).GetAwaiter().GetResult();
                });

        private static IServiceCollection AddVault(this IServiceCollection services, string sectionName)
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
            VerifyOptions(options);
            services.AddSingleton(options);
            services.AddTransient<IKeyValueSecrets, KeyValueSecrets>();
            var (client, settings) = GetClientAndSettings(options);
            services.AddSingleton(settings);
            services.AddSingleton(client);
            services.AddSingleton(LeaseService);
            services.AddSingleton(CertificatesService);
            services.AddHostedService<VaultHostedService>();
            if (options.Pki is {})
            {
                services.AddSingleton<ICertificatesIssuer, CertificatesIssuer>();
            }
            else
            {
                services.AddSingleton<ICertificatesIssuer, EmptyCertificatesIssuer>();
            }

            return services;
        }
        
        
        private static void VerifyOptions(VaultOptions options)
        {
            if (options.Kv is null)
            {
                if (!string.IsNullOrWhiteSpace(options.Key))
                {
                    options.Kv = new VaultOptions.KeyValueOptions
                    {
                        Enabled = options.Enabled,
                        Path = options.Key
                    };
                }
                
                return;
            }

            if (options.Kv.EngineVersion > 2 || options.Kv.EngineVersion < 0)
            {
                throw new VaultException($"Invalid KV engine version: {options.Kv.EngineVersion} (available: 1 or 2).");
            }
                
            if (options.Kv.EngineVersion == 0)
            {
                options.Kv.EngineVersion = 2;
            }
        }

        private static async Task AddVaultAsync(this IConfigurationBuilder builder, VaultOptions options,
            string keyValuePath)
        {
            VerifyOptions(options);
            var kvPath = string.IsNullOrWhiteSpace(keyValuePath) ? options.Kv?.Path : keyValuePath;
            var (client, _) = GetClientAndSettings(options);
            if (!string.IsNullOrWhiteSpace(kvPath) && options.Kv.Enabled)
            {
                Console.WriteLine($"Loading settings from Vault: '{options.Url}', KV path: '{kvPath}'.");
                var keyValueSecrets = new KeyValueSecrets(client, options);
                var secret = await keyValueSecrets.GetAsync(kvPath);
                var parser = new JsonParser();
                var data = parser.Parse(JObject.FromObject(secret));
                var source = new MemoryConfigurationSource {InitialData = data};
                builder.Add(source);
            }

            if (options.Pki is {} && options.Pki.Enabled)
            {
                Console.WriteLine("Initializing Vault PKI.");
                await SetPkiSecretsAsync(client, options);
            }
            
            if (options.Lease is null || !options.Lease.Any())
            {
                return;
            }
            
            var configuration = new Dictionary<string, string>();
            foreach (var (key, lease) in options.Lease)
            {
                if (!lease.Enabled || string.IsNullOrWhiteSpace(lease.Type))
                {
                    continue;
                }

                Console.WriteLine($"Initializing Vault lease for: '{key}', type: '{lease.Type}'.");
                await InitLeaseAsync(key, client, lease, configuration);
            }

            if (configuration.Any())
            {
                var source = new MemoryConfigurationSource {InitialData = configuration};
                builder.Add(source);
            }
        }

        private static Task InitLeaseAsync(string key, IVaultClient client, VaultOptions.LeaseOptions options,
            IDictionary<string, string> configuration)
            => options.Type.ToLowerInvariant() switch
            {
                "activedirectory" => SetActiveDirectorySecretsAsync(key, client, options, configuration),
                "azure" => SetAzureSecretsAsync(key, client, options, configuration),
                "consul" => SetConsulSecretsAsync(key, client, options, configuration),
                "database" => SetDatabaseSecretsAsync(key, client, options, configuration),
                "rabbitmq" => SetRabbitMqSecretsAsync(key, client, options, configuration),
                _ => Task.CompletedTask
            };

        private static async Task SetActiveDirectorySecretsAsync(string key, IVaultClient client,
            VaultOptions.LeaseOptions options, IDictionary<string, string> configuration)
        {
            const string name = SecretsEngineDefaultPaths.ActiveDirectory;
            var mountPoint = string.IsNullOrWhiteSpace(options.MountPoint) ? name : options.MountPoint;
            var credentials =
                await client.V1.Secrets.ActiveDirectory.GetCredentialsAsync(options.RoleName, mountPoint);
            SetSecrets(key, options, configuration, name, () => (credentials, new Dictionary<string, string>
            {
                ["username"] = credentials.Data.Username,
                ["currentPassword"] = credentials.Data.CurrentPassword,
                ["lastPassword"] = credentials.Data.LastPassword
            }, credentials.LeaseId, credentials.LeaseDurationSeconds, credentials.Renewable));
        }

        private static async Task SetAzureSecretsAsync(string key, IVaultClient client,
            VaultOptions.LeaseOptions options,
            IDictionary<string, string> configuration)
        {
            const string name = SecretsEngineDefaultPaths.Azure;
            var mountPoint = string.IsNullOrWhiteSpace(options.MountPoint) ? name : options.MountPoint;
            var credentials =
                await client.V1.Secrets.Azure.GetCredentialsAsync(options.RoleName, mountPoint);
            SetSecrets(key, options, configuration, name, () => (credentials, new Dictionary<string, string>
            {
                ["clientId"] = credentials.Data.ClientId,
                ["clientSecret"] = credentials.Data.ClientSecret
            }, credentials.LeaseId, credentials.LeaseDurationSeconds, credentials.Renewable));
        }

        private static async Task SetConsulSecretsAsync(string key, IVaultClient client,
            VaultOptions.LeaseOptions options,
            IDictionary<string, string> configuration)
        {
            const string name = SecretsEngineDefaultPaths.Consul;
            var mountPoint = string.IsNullOrWhiteSpace(options.MountPoint) ? name : options.MountPoint;
            var credentials =
                await client.V1.Secrets.Consul.GetCredentialsAsync(options.RoleName, mountPoint);
            SetSecrets(key, options, configuration, name, () => (credentials, new Dictionary<string, string>
            {
                ["token"] = credentials.Data.Token
            }, credentials.LeaseId, credentials.LeaseDurationSeconds, credentials.Renewable));
        }

        private static async Task SetDatabaseSecretsAsync(string key, IVaultClient client,
            VaultOptions.LeaseOptions options,
            IDictionary<string, string> configuration)
        {
            const string name = SecretsEngineDefaultPaths.Database;
            var mountPoint = string.IsNullOrWhiteSpace(options.MountPoint) ? name : options.MountPoint;
            var credentials =
                await client.V1.Secrets.Database.GetCredentialsAsync(options.RoleName, mountPoint);
            SetSecrets(key, options, configuration, name, () => (credentials, new Dictionary<string, string>
            {
                ["username"] = credentials.Data.Username,
                ["password"] = credentials.Data.Password
            }, credentials.LeaseId, credentials.LeaseDurationSeconds, credentials.Renewable));
        }

        private static async Task SetPkiSecretsAsync(IVaultClient client, VaultOptions options)
        {
            var issuer = new CertificatesIssuer(client, options);
            var certificate = await issuer.IssueAsync();
            CertificatesService.Set(options.Pki.RoleName, certificate);
        }

        private static async Task SetRabbitMqSecretsAsync(string key, IVaultClient client,
            VaultOptions.LeaseOptions options,
            IDictionary<string, string> configuration)
        {
            const string name = SecretsEngineDefaultPaths.RabbitMQ;
            var mountPoint = string.IsNullOrWhiteSpace(options.MountPoint) ? name : options.MountPoint;
            var credentials =
                await client.V1.Secrets.RabbitMQ.GetCredentialsAsync(options.RoleName, mountPoint);
            SetSecrets(key, options, configuration, name, () => (credentials, new Dictionary<string, string>
            {
                ["username"] = credentials.Data.Username,
                ["password"] = credentials.Data.Password
            }, credentials.LeaseId, credentials.LeaseDurationSeconds, credentials.Renewable));
        }

        private static void SetSecrets(string key, VaultOptions.LeaseOptions options,
            IDictionary<string, string> configuration, string name,
            Func<(object, Dictionary<string, string>, string, int, bool)> lease)
        {
            var createdAt = DateTime.UtcNow;
            var (credentials, values, leaseId, duration, renewable) = lease();
            SetTemplates(key, options, configuration, values);
            var leaseData = new LeaseData(name, leaseId, duration, renewable, createdAt, credentials);
            LeaseService.Set(key, leaseData);
        }

        private static (IVaultClient client, VaultClientSettings settings) GetClientAndSettings(VaultOptions options)
        {
            var settings = new VaultClientSettings(options.Url, GetAuthMethod(options));
            var client = new VaultClient(settings);

            return (client, settings);
        }

        private static void SetTemplates(string key, VaultOptions.LeaseOptions lease,
            IDictionary<string, string> configuration, IDictionary<string, string> values)
        {
            if (lease.Templates is null || !lease.Templates.Any())
            {
                return;
            }

            foreach (var (property, template) in lease.Templates)
            {
                if (string.IsNullOrWhiteSpace(property) || string.IsNullOrWhiteSpace(template))
                {
                    continue;
                }

                var templateValue = $"{template}";
                templateValue = values.Aggregate(templateValue,
                    (current, value) => current.Replace($"{{{{{value.Key}}}}}", value.Value));
                configuration.Add($"{key}:{property}", templateValue);
            }
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