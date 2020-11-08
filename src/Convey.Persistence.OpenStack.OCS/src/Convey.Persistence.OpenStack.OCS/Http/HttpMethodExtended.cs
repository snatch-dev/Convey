using System.Net.Http;

namespace Convey.Persistence.OpenStack.OCS.Http
{
    internal static class HttpMethodExtended
    {
        public static HttpMethod Copy => new HttpMethod("COPY");
    }
}
