using System;

namespace Convey.Secrets.Vault
{
    internal sealed class VaultException : Exception
    {
        public string Key { get; }
        
        public VaultException(string key) : this(null, key)
        {
        }

        public VaultException(Exception innerException, string key) : this(string.Empty, innerException, key)
        {
        }

        public VaultException(string message, Exception innerException, string key) : base(message, innerException)
        {
            Key = key;
        }
    }
}