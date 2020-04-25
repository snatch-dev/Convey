using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Convey.Security.Internals
{
    internal sealed class Encryptor : IEncryptor
    {
        public string Encrypt(string data, string key)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentException("Data to be encrypted cannot be empty.", nameof(data));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Encryption key cannot be empty.", nameof(key));
            }

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            var iv = Convert.ToBase64String(aes.IV);
            var transform = aes.CreateEncryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(data);
            }

            return iv + Convert.ToBase64String(memoryStream.ToArray());
        }

        public string Decrypt(string data, string key)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentException("Data to be decrypted cannot be empty.", nameof(data));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Encryption key cannot be empty.", nameof(key));
            }

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Convert.FromBase64String(data.Substring(0, 24));
            var transform = aes.CreateDecryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream(Convert.FromBase64String(data.Substring(24)));
            using var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }

        public byte[] Encrypt(byte[] data, byte[] iv, byte[] key)
        {
            if (data is null || !data.Any())
            {
                throw new ArgumentException("Data to be encrypted cannot be empty.", nameof(data));
            }

            if (iv is null || !iv.Any())
            {
                throw new ArgumentException("Initialization vector cannot be empty.", nameof(iv));
            }

            if (key is null || !key.Any())
            {
                throw new ArgumentException("Encryption key cannot be empty.", nameof(key));
            }

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            var transform = aes.CreateEncryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(data);
            }

            return memoryStream.ToArray();
        }

        public byte[] Decrypt(byte[] data, byte[] iv, byte[] key)
        {
            if (data is null || !data.Any())
            {
                throw new ArgumentException("Data to be decrypted cannot be empty.", nameof(data));
            }

            if (iv is null || !iv.Any())
            {
                throw new ArgumentException("Initialization vector cannot be empty.", nameof(iv));
            }

            if (key is null || !key.Any())
            {
                throw new ArgumentException("Encryption key cannot be empty.", nameof(key));
            }

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            var transform = aes.CreateDecryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream(data);
            using var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return memoryStream.ToArray();
        }
    }
}