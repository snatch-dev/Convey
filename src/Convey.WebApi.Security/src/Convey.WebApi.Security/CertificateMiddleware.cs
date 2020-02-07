using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Convey.WebApi.Security
{
    internal sealed class CertificateMiddleware : IMiddleware
    {
        private readonly ICertificatePermissionValidator _certificatePermissionValidator;
        private readonly SecurityOptions.CertificateOptions _options;
        private readonly HashSet<string> _allowedHosts;
        private readonly IDictionary<string, SecurityOptions.CertificateOptions.AclOptions> _acl;
        private readonly IDictionary<string, string> _subjects = new Dictionary<string, string>();
        private readonly bool _validateAcl;

        public CertificateMiddleware(ICertificatePermissionValidator certificatePermissionValidator,
            SecurityOptions options)
        {
            _certificatePermissionValidator = certificatePermissionValidator;
            _options = options.Certificate;
            _allowedHosts = new HashSet<string>(_options.AllowedHosts ?? Array.Empty<string>());
            _validateAcl = _options.Acl is {} && _options.Acl.Any();
            if (!_validateAcl)
            {
                return;
            }

            _acl = new Dictionary<string, SecurityOptions.CertificateOptions.AclOptions>();
            foreach (var (key, acl) in _options.Acl)
            {
                if (!string.IsNullOrWhiteSpace(acl.ValidIssuer) && !acl.ValidIssuer.StartsWith("CN="))
                {
                    acl.ValidIssuer = $"CN={acl.ValidIssuer}";
                }

                var subject = key.StartsWith("CN=") ? key : $"CN={key}";
                if (_options.AllowSubdomains)
                {
                    foreach (var domain in options.Certificate.AllowedDomains ?? Enumerable.Empty<string>())
                    {
                        _subjects.Add($"{subject}.{domain}", key);
                    }
                }

                _acl.Add(_subjects.Any() ? key : subject, acl);
            }
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!_options.Enabled)
            {
                return next(context);
            }

            if (IsAllowedHost(context))
            {
                return next(context);
            }

            var certificate = context.Connection.ClientCertificate;
            if (certificate is null || !certificate.Verify())
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }

            if (!_validateAcl)
            {
                return next(context);
            }

            SecurityOptions.CertificateOptions.AclOptions acl;
            if (_subjects.TryGetValue(certificate.Subject, out var subject))
            {
                if (!_acl.TryGetValue(subject, out var existingAcl))
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }

                acl = existingAcl;
            }
            else
            {
                if (!_acl.TryGetValue(certificate.Subject, out var existingAcl))
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }

                acl = existingAcl;
            }

            if (!string.IsNullOrWhiteSpace(acl.ValidIssuer) && certificate.Issuer != acl.ValidIssuer)
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            }

            if (!string.IsNullOrWhiteSpace(acl.ValidThumbprint) && certificate.Thumbprint != acl.ValidThumbprint)
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            }

            if (!string.IsNullOrWhiteSpace(acl.ValidSerialNumber) && certificate.SerialNumber != acl.ValidSerialNumber)
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            }

            if (acl.Permissions is null || !acl.Permissions.Any())
            {
                return next(context);
            }

            if (_certificatePermissionValidator.HasAccess(certificate, acl.Permissions, context))
            {
                return next(context);
            }

            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }

        private bool IsAllowedHost(HttpContext context)
        {
            var host = context.Request.Host.Host;
            if (_allowedHosts.Contains(host))
            {
                return true;
            }

            return context.Request.Headers.TryGetValue("x-forwarded-for", out var forwardedFor) &&
                   _allowedHosts.Contains(forwardedFor);
        }
    }
}