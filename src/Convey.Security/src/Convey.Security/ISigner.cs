using System.Security.Cryptography.X509Certificates;

namespace Convey.Security
{
    // RSA + SHA-256
    public interface ISigner
    {
        string Sign(object data, X509Certificate2 certificate);
        bool Verify(object data, X509Certificate2 certificate, string signature, bool throwException = false);
        byte[] Sign(byte[] data, X509Certificate2 certificate);
        bool Verify(byte[] data, X509Certificate2 certificate, byte[] signature, bool throwException = false);
    }
}