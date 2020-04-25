using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Convey.Security.Internals
{
    internal sealed class Hasher : IHasher
    {
        public string Hash(string data)
        {
            var hash = Hash(Encoding.UTF8.GetBytes(data));
            var builder = new StringBuilder();
            foreach (var @byte in hash)
            {
                builder.Append(@byte.ToString("x2"));
            }

            return builder.ToString();
        }

        public byte[] Hash(byte[] data)
        {
            if (data is null || !data.Any())
            {
                throw new ArgumentException("Data to be hashed cannot be empty.", nameof(data));
            }

            using var sha256Hash = SHA256.Create();
            
            return sha256Hash.ComputeHash(data);
        }
    }
}