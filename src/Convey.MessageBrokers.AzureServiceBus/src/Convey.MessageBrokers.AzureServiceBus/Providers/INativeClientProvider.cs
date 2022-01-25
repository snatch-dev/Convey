using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Convey.MessageBrokers.AzureServiceBus.Providers;

public interface INativeClientProvider
{
    ValueTask<T> UseClientAsync<T>(Func<ServiceBusClient, ValueTask<T>> use);
    
    Task<T> UseClientAsync<T>(Func<ServiceBusClient, Task<T>> use);
    
    T UseClient<T>(Func<ServiceBusClient, T> use);
    
    void UseClient<T>(Action<ServiceBusClient> use);
    
    Task UseClientAsync<T>(Func<ServiceBusClient, Task> use);
    
    ValueTask UseClientAsync<T>(Func<ServiceBusClient, ValueTask> use);
    
    ValueTask<T> UseAdminClientAsync<T>(Func<ServiceBusAdministrationClient, ValueTask<T>> use);
    
    Task<T> UseAdminClientAsync<T>(Func<ServiceBusAdministrationClient, Task<T>> use);
    
    T UseAdminClient<T>(Func<ServiceBusAdministrationClient, T> use);
    
    void UseAdminClient<T>(Action<ServiceBusAdministrationClient> use);
    
    Task UseAdminClientAsync<T>(Func<ServiceBusAdministrationClient, Task> use);
    
    ValueTask UseAdminClientAsync<T>(Func<ServiceBusAdministrationClient, ValueTask> use);
}