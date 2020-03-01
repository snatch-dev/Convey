using System.Collections.Generic;

namespace Convey.WebApi.Security
{
    public class SecurityOptions
    {
        public CertificateOptions Certificate { get; set; }

        public class CertificateOptions
        {
            public bool Enabled { get; set; }
            public string Header { get; set; }
            public bool AllowSubdomains { get; set; }
            public IEnumerable<string> AllowedDomains { get; set; }
            public IEnumerable<string> AllowedHosts { get; set; }
            public IDictionary<string, AclOptions> Acl { get; set; }
            public bool SkipRevocationCheck { get; set; }

            public string GetHeaderName() => string.IsNullOrWhiteSpace(Header) ? "Certificate" : Header;

            public class AclOptions
            {
                public string ValidIssuer { get; set; }
                public string ValidThumbprint { get; set; }
                public string ValidSerialNumber { get; set; }
                public IEnumerable<string> Permissions { get; set; }
            }
        }
    }
}