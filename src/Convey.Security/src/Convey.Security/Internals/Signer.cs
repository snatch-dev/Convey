using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace Convey.Security.Internals;

internal sealed class Signer : ISigner
{
    public string Sign(object data, X509Certificate2 certificate)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data), "Data to be signed cannot be null.");
        }
            
        if (certificate is null)
        {
            throw new ArgumentNullException(nameof(certificate), "Certificate cannot be null.");
        }

        var bytes = JsonSerializer.SerializeToUtf8Bytes(data);
        var signature = Sign(bytes, certificate);

        return BitConverter.ToString(signature).Replace("-", string.Empty);
    }

    public bool Verify(object data, X509Certificate2 certificate, string signature, bool throwException = false)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data), "Data to be verified cannot be null.");
        }

        if (certificate is null)
        {
            throw new ArgumentNullException(nameof(certificate), "Certificate cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new ArgumentException("Signature cannot be empty.", nameof(signature));
        }

        var bytes = JsonSerializer.SerializeToUtf8Bytes(data);

        return Verify(bytes, certificate, ToByteArray(signature), throwException);
    }

    public byte[] Sign(byte[] data, X509Certificate2 certificate)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data), "Data to be signed cannot be null.");
        }

        if (certificate is null)
        {
            throw new ArgumentNullException(nameof(certificate), "Certificate cannot be null.");
        }

        using var rsa = certificate.GetRSAPrivateKey();
        if (rsa is null)
        {
            throw new InvalidOperationException("RSA private key couldn't be loaded.");
        }

        return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    public bool Verify(byte[] data, X509Certificate2 certificate, byte[] signature, bool throwException = false)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data), "Data to be verified cannot be null.");
        }
            
        if (signature is null || !signature.Any())
        {
            throw new ArgumentException("Signature cannot be empty.", nameof(signature));
        }
            
        if (certificate is null)
        {
            throw new ArgumentNullException(nameof(certificate), "Certificate cannot be null.");
        }
            
        try
        {
            using var rsa = certificate.GetRSAPublicKey();
            if (rsa is null)
            {
                throw new InvalidOperationException("RSA public key couldn't be loaded.");
            }
                
            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch
        {
            if (throwException)
            {
                throw;
            }

            return false;
        }
    }

    private static byte[] ToByteArray(string hex)
    {
        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }
}