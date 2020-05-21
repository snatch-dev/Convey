using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Primitives;

namespace Convey.Auth.Distributed
{
    internal sealed class DistributedAccessTokenService : IAccessTokenService
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TimeSpan _expires;

        public DistributedAccessTokenService(IDistributedCache cache, IHttpContextAccessor httpContextAccessor,
            JwtOptions jwtOptions)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _expires = jwtOptions.Expiry ?? TimeSpan.FromMinutes(jwtOptions.ExpiryMinutes);
        }

        public Task<bool> IsCurrentActiveToken()
            => IsActiveAsync(GetCurrentAsync());

        public Task DeactivateCurrentAsync()
            => DeactivateAsync(GetCurrentAsync());

        public async Task<bool> IsActiveAsync(string token)
            => string.IsNullOrWhiteSpace(await _cache.GetStringAsync(GetKey(token)));

        public Task DeactivateAsync(string token)
            => _cache.SetStringAsync(GetKey(token),
                "revoked", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _expires
                });

        private string GetCurrentAsync()
        {
            var authorizationHeader = _httpContextAccessor
                .HttpContext.Request.Headers["authorization"];

            return authorizationHeader == StringValues.Empty
                ? string.Empty
                : authorizationHeader.Single().Split(' ').Last();
        }

        private static string GetKey(string token) => $"blacklisted-tokens:{token}";
    }
}