using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;

namespace Convey.WebApi.Security
{
    public interface ICertificatePermissionValidator
    {
        bool HasAccess(X509Certificate2 certificate, IEnumerable<string> permissions, HttpContext context);
    }
}