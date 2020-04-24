using System.Collections.Generic;

namespace Convey.Secrets.Vault
{
    public class VaultOptions
    {
        public bool Enabled { get; set; }
        public string Url { get; set; }
        public string Key { get; set; }
        public string AuthType { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RevokeLeaseOnShutdown { get; set; }
        public int RenewalsInterval { get; set; }
        public KeyValueOptions Kv { get; set; }
        public PkiOptions Pki { get; set; }
        public IDictionary<string, LeaseOptions> Lease { get; set; }

        public class KeyValueOptions
        {
            public bool Enabled { get; set; }
            public int EngineVersion { get; set; } = 2;
            public string MountPoint { get; set; } = "kv";
            public string Path { get; set; }
            public int? Version { get; set; }
        }

        public class LeaseOptions
        {
            public bool Enabled { get; set; }
            public string Type { get; set; }
            public string RoleName { get; set; }
            public string MountPoint { get; set; }
            public bool AutoRenewal { get; set; }
            public IDictionary<string, string> Templates { get; set; }
        }

        public class PkiOptions
        {
            public bool Enabled { get; set; }
            public string RoleName { get; set; }
            public string MountPoint { get; set; }
            public string CertificateFormat { get; set; }
            public string PrivateKeyFormat { get; set; }
            public string CommonName { get; set; }
            public string TTL { get; set; }
            public string SubjectAlternativeNames { get; set; }
            public string OtherSubjectAlternativeNames { get; set; }
            public bool ExcludeCommonNameFromSubjectAlternativeNames { get; set; }
            public string IPSubjectAlternativeNames { get; set; }
            public string URISubjectAlternativeNames { get; set; }
            public bool ImportPrivateKey { get; set; }
        }
    }
}