using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Convey.Auth
{
    public class AccessTokenValidatorMiddleware : IMiddleware
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly IEnumerable<string> _endpoints;

        public AccessTokenValidatorMiddleware(IAccessTokenService accessTokenService, JwtOptions options)
        {
            _accessTokenService = accessTokenService;
            _endpoints = options.AllowAnonymousEndpoints ?? Enumerable.Empty<string>();
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var path = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;
            if (_endpoints.Contains(path))
            {
                await next(context);

                return;
            }

            if (await _accessTokenService.IsCurrentActiveToken())
            {
                await next(context);

                return;
            }

            context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
        }
    }
}