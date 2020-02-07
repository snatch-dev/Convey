using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.SecretsEngines;
using VaultSharp.V1.SecretsEngines.PKI;

namespace Convey.Secrets.Vault.Internals
{
    internal sealed class CertificatesIssuer : ICertificatesIssuer
    {
        private readonly IVaultClient _client;
        private readonly VaultOptions.PkiOptions _options;
        private readonly CertificateFormat _certificateFormat;
        private readonly PrivateKeyFormat _privateKeyFormat;
        private readonly string _mountPoint;

        public CertificatesIssuer(IVaultClient client, VaultOptions options)
        {
            _client = client;
            _options = options.Pki;
            _mountPoint = string.IsNullOrWhiteSpace(_options.MountPoint)
                ? SecretsEngineDefaultPaths.PKI
                : _options.MountPoint;
            _certificateFormat = string.IsNullOrWhiteSpace(_options.CertificateFormat)
                ? CertificateFormat.pem
                : Enum.Parse<CertificateFormat>(_options.CertificateFormat, true);
            _privateKeyFormat = string.IsNullOrWhiteSpace(_options.PrivateKeyFormat)
                ? PrivateKeyFormat.der
                : Enum.Parse<PrivateKeyFormat>(_options.PrivateKeyFormat, true);
        }

        public async Task<X509Certificate2> IssueAsync()
        {
            var credentials =
                await _client.V1.Secrets.PKI.GetCredentialsAsync(_options.RoleName,
                    new CertificateCredentialsRequestOptions
                    {
                        CertificateFormat = _certificateFormat,
                        PrivateKeyFormat = _privateKeyFormat,
                        CommonName = _options.CommonName,
                        TimeToLive = _options.TTL,
                        SubjectAlternativeNames = _options.SubjectAlternativeNames,
                        OtherSubjectAlternativeNames = _options.OtherSubjectAlternativeNames,
                        IPSubjectAlternativeNames = _options.IPSubjectAlternativeNames,
                        URISubjectAlternativeNames = _options.URISubjectAlternativeNames,
                        ExcludeCommonNameFromSubjectAlternativeNames =
                            _options.ExcludeCommonNameFromSubjectAlternativeNames
                    }, _mountPoint);

            var certificate = new X509Certificate2(Encoding.UTF8.GetBytes(credentials.Data.CertificateContent));
            if (!_options.ImportPrivateKey || _privateKeyFormat != PrivateKeyFormat.der)
            {
                return certificate;
            }

            var privateKey = Convert.FromBase64String(credentials.Data.PrivateKeyContent
                .Replace("-----BEGIN RSA PRIVATE KEY-----", string.Empty)
                .Replace("-----END RSA PRIVATE KEY-----", string.Empty));

            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);
            certificate = certificate.CopyWithPrivateKey(rsa);

            return certificate;
        }
    }
}