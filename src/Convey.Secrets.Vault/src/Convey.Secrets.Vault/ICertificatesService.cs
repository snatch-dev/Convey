using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Convey.Secrets.Vault;

public interface ICertificatesService
{
    IReadOnlyDictionary<string, X509Certificate2> All { get; }
    X509Certificate2 Get(string name);
    void Set(string name, X509Certificate2 certificate);
}