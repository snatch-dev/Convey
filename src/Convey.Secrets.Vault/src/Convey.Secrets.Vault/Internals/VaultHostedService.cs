using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VaultSharp;

namespace Convey.Secrets.Vault.Internals
{
    internal sealed class VaultHostedService : IHostedService
    {
        private readonly IVaultClient _client;
        private readonly ILeaseService _leaseService;
        private readonly VaultOptions _options;
        private readonly ILogger<VaultHostedService> _logger;
        private Timer _timer;
        private readonly int _interval;

        public VaultHostedService(IVaultClient client, ILeaseService leaseService, VaultOptions options,
            ILogger<VaultHostedService> logger)
        {
            _client = client;
            _leaseService = leaseService;
            _options = options;
            _logger = logger;
            _interval = 5;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_options.Enabled || _options.Lease is null || !_options.Lease.Any(l => l.Value.AutoRenewal))
            {
                return Task.CompletedTask;
            }

            _logger.LogInformation($"Vault is enabled, processing lease renewals every {_interval} s.");
            _timer = new Timer(ProcessLease, null, TimeSpan.Zero, TimeSpan.FromSeconds(_interval));
            return Task.CompletedTask;
        }

        private void ProcessLease(object state)
        {
            _ = ProcessLeaseAsync();
        }

        private async Task ProcessLeaseAsync()
        {
            var now = DateTime.UtcNow;
            var nextIterationAt = now.AddSeconds(2 * _interval);
            foreach (var (key, lease) in _leaseService.All.Where(l => l.Value.AutoRenewal))
            {
                if (lease.ExpiryAt > nextIterationAt)
                {
                    continue;
                }

                _logger.LogInformation($"Renewing a lease with ID: '{lease.Id}', for: '{key}', " +
                                       $"duration: {lease.Duration} s.");
                var renewedLease = await _client.V1.System.RenewLeaseAsync(lease.Id, lease.Duration);
                lease.Refresh(renewedLease.LeaseDurationSeconds);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                return Task.CompletedTask;
            }

            _timer?.Change(Timeout.Infinite, 0);
            if (!_options.RevokeLeaseOnShutdown)
            {
                return Task.CompletedTask;
            }

            foreach (var (key, lease) in _leaseService.All)
            {
                _logger.LogInformation($"Revoking a lease with ID: '{lease.Id}', for: '{key}'.");
                _client.V1.System.RevokeLeaseAsync(lease.Id);
            }

            return Task.CompletedTask;
        }
    }
}