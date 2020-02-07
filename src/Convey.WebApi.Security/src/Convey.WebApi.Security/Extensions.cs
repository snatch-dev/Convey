using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.WebApi.Security
{
    public static class Extensions
    {
        private const string SectionName = "security";
        private const string RegistryName = "security";

        public static IConveyBuilder AddCertificateAuthentication(this IConveyBuilder builder,
            string sectionName = SectionName, Type permissionValidatorType = null)
        {
            var options = builder.GetOptions<SecurityOptions>(sectionName);
            builder.Services.AddSingleton(options);
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            if (options.Certificate is null || !options.Certificate.Enabled)
            {
                return builder;
            }

            if (permissionValidatorType is {})
            {
                builder.Services.AddSingleton(typeof(ICertificatePermissionValidator), permissionValidatorType);
            }
            else
            {
                builder.Services.AddSingleton<ICertificatePermissionValidator, DefaultCertificatePermissionValidator>();
            }
            
            builder.Services.AddSingleton<CertificateMiddleware>();
            builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate();
            builder.Services.AddCertificateForwarding(c =>
            {
                c.CertificateHeader = options.Certificate.GetHeaderName();
                c.HeaderConverter = headerValue =>
                    string.IsNullOrWhiteSpace(headerValue)
                        ? null
                        : new X509Certificate2(StringToByteArray(headerValue));
            });

            return builder;
        }

        public static IApplicationBuilder UseCertificateAuthentication(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<SecurityOptions>();
            if (options.Certificate is null || !options.Certificate.Enabled)
            {
                return app;
            }

            app.UseCertificateForwarding();
            app.UseMiddleware<CertificateMiddleware>();

            return app;
        }

        private static byte[] StringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}