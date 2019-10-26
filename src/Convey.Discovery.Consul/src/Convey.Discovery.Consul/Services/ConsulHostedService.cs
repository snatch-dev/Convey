using System.Threading;
using System.Threading.Tasks;
using Convey.Discovery.Consul.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Convey.Discovery.Consul.Services
{
    internal sealed class ConsulHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ConsulHostedService> _logger;

        public ConsulHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<ConsulHostedService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var consulService = scope.ServiceProvider.GetRequiredService<IConsulService>();
            var registration = scope.ServiceProvider.GetRequiredService<ServiceRegistration>();
            _logger.LogInformation($"Registering a service [id: {registration.Id}] in Consul...");
            var response = await consulService.RegisterServiceAsync(registration);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Registered a service [id: {registration.Id}] in Consul.");
                return;
            }

            _logger.LogError($"There was an error when registering a service [id: {registration.Id}] in Consul. {response}");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var consulService = scope.ServiceProvider.GetRequiredService<IConsulService>();
            var registration = scope.ServiceProvider.GetRequiredService<ServiceRegistration>();
            _logger.LogInformation($"Deregistering a service [id: {registration.Id}] from Consul...");
            var response = await consulService.DeregisterServiceAsync(registration.Id);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Deregistered a service [id: {registration.Id}] from Consul.");
                return;
            }

            _logger.LogError($"There was an error when deregistering a service [id: {registration.Id}] from Consul. {response}");
        }
    }
}