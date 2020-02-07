using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VaultSharp;

namespace Convey.Secrets.Vault.Internals
{
    internal sealed class VaultHostedService : BackgroundService
    {
        private readonly IVaultClient _client;
        private readonly ILeaseService _leaseService;
        private readonly ICertificatesIssuer _certificatesIssuer;
        private readonly ICertificatesService _certificatesService;
        private readonly VaultOptions _options;
        private readonly ILogger<VaultHostedService> _logger;
        private readonly int _interval;

        public VaultHostedService(IVaultClient client, ILeaseService leaseService, ICertificatesIssuer certificatesIssuer,
            ICertificatesService certificatesService, VaultOptions options, ILogger<VaultHostedService> logger)
        {
            _client = client;
            _leaseService = leaseService;
            _certificatesIssuer = certificatesIssuer;
            _certificatesService = certificatesService;
            _options = options;
            _logger = logger;
            _interval = _options.RenewalsInterval <= 0 ? 10 : _options.RenewalsInterval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                return;
            }

            if ((_options.Pki is null || !_options.Pki.Enabled) &&
                (_options.Lease is null || _options.Lease.All(l => !l.Value.Enabled) ||
                 !_options.Lease.Any(l => l.Value.AutoRenewal)))
            {
                return;
            }

            _logger.LogInformation($"Vault lease renewals will be processed every {_interval} s.");
            var interval = TimeSpan.FromSeconds(_interval);
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextIterationAt = now.AddSeconds(2 * _interval);
                
                if (_options.Pki is {} && _options.Pki.Enabled)
                {
                    foreach (var (role, cert) in _certificatesService.All)
                    {
                        if (cert.NotAfter > nextIterationAt)
                        {
                            continue;
                        }
                        
                        _logger.LogInformation($"Issuing a certificate for: '{role}'.");
                        var certificate = await _certificatesIssuer.IssueAsync();
                        _certificatesService.Set(role, certificate);
                    }
                }

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

                await Task.Delay(interval, stoppingToken);
            }

            if (!_options.RevokeLeaseOnShutdown)
            {
                return;
            }

            foreach (var (key, lease) in _leaseService.All)
            {
                _logger.LogInformation($"Revoking a lease with ID: '{lease.Id}', for: '{key}'.");
                await _client.V1.System.RevokeLeaseAsync(lease.Id);
            }
        }
    }
}