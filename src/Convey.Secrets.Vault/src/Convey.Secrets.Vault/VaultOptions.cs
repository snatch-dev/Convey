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
        public IDictionary<string, LeaseOptions> Lease { get; set; }

        public class LeaseOptions
        {
            public string Type { get; set; }
            public string RoleName { get; set; }
            public string MountPoint { get; set; }
            public bool AutoRenewal { get; set; }
            public bool Enabled { get; set; }
            public IDictionary<string, string> Templates { get; set; }
        }
    }
}