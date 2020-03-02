using System.Collections.Generic;

namespace Convey.Auth
{
    public interface IJwtHandler
    {
        JsonWebToken CreateToken(string userId, string role = null, string audience = null,
            IDictionary<string, string> claims = null);

        JsonWebToken CreateToken(string userId, string role = null, string audience = null,
            IEnumerable<KeyValuePair<string, string>> claims = null);
        
        JsonWebTokenPayload GetTokenPayload(string accessToken);
    }
}