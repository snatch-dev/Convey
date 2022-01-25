using System.Net.NetworkInformation;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Convey.MessageBrokers.AzureServiceBus.Providers;

public class DefaultNativeClientProvider : INativeClientProvider
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusAdministrationClient _admin;

    public DefaultNativeClientProvider(IOptionsMonitor<AzureServiceBusOptions> serviceBusOptions)
    {
        var (client, admin) = BuildClients(serviceBusOptions.CurrentValue);
        _client = client;
        _admin = admin;
    }

    private (ServiceBusClient serviceBusClient, ServiceBusAdministrationClient administrationClient) BuildClients(
        AzureServiceBusOptions serviceBusOptions)
    {
        if (serviceBusOptions.Connection.ConnectionString is not null)
        {
            var client = new ServiceBusClient(serviceBusOptions.Connection.ConnectionString);
            var admin = new ServiceBusAdministrationClient(serviceBusOptions.Connection.ConnectionString);

            return (client, admin);
        }

        throw new ArgumentNullException(
            nameof(AzureServiceBusOptions.Connection.ConnectionString),
            "A connection string is required");
    }

    public ValueTask<T> UseClientAsync<T>(Func<ServiceBusClient, ValueTask<T>> use) =>
        use(_client);

    public Task<T> UseClientAsync<T>(Func<ServiceBusClient, Task<T>> use) =>
        use(_client);

    public T UseClient<T>(Func<ServiceBusClient, T> use) =>
        use(_client);

    public void UseClient<T>(Action<ServiceBusClient> use) =>
        use(_client);

    public Task UseClientAsync<T>(Func<ServiceBusClient, Task> use) =>
        use(_client);

    public ValueTask UseClientAsync<T>(Func<ServiceBusClient, ValueTask> use) =>
        use(_client);

    public ValueTask<T> UseAdminClientAsync<T>(Func<ServiceBusAdministrationClient, ValueTask<T>> use) =>
        use(_admin);

    public Task<T> UseAdminClientAsync<T>(Func<ServiceBusAdministrationClient, Task<T>> use) =>
        use(_admin);

    public T UseAdminClient<T>(Func<ServiceBusAdministrationClient, T> use) =>
        use(_admin);

    public void UseAdminClient<T>(Action<ServiceBusAdministrationClient> use) =>
        use(_admin);

    public Task UseAdminClientAsync<T>(Func<ServiceBusAdministrationClient, Task> use) =>
        use(_admin);

    public ValueTask UseAdminClientAsync<T>(Func<ServiceBusAdministrationClient, ValueTask> use) =>
        use(_admin);
}