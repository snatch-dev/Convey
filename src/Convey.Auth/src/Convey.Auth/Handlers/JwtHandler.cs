using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Convey.Auth.Dates;
using Microsoft.IdentityModel.Tokens;

namespace Convey.Auth.Handlers
{
    internal sealed class JwtHandler : IJwtHandler
    {
        private static readonly ISet<string> DefaultClaims = new HashSet<string>
        {
            JwtRegisteredClaimNames.Sub,
            JwtRegisteredClaimNames.UniqueName,
            JwtRegisteredClaimNames.Jti,
            JwtRegisteredClaimNames.Iat,
            ClaimTypes.Role,
        };

        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        private readonly JwtOptions _options;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly SigningCredentials _signingCredentials;

        public JwtHandler(JwtOptions options, TokenValidationParameters tokenValidationParameters)
        {
            _options = options;
            _tokenValidationParameters = tokenValidationParameters;
            var issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.IssuerSigningKey));
            _signingCredentials = new SigningCredentials(issuerSigningKey, SecurityAlgorithms.HmacSha256);
        }

        public JsonWebToken CreateToken(string userId, string role = null, IDictionary<string, string> claims = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id claim can not be empty.", nameof(userId));
            }

            var now = DateTime.UtcNow;
            var jwtClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.UniqueName, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToTimestamp().ToString()),
            };
            if (!string.IsNullOrWhiteSpace(role))
            {
                jwtClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var customClaims = claims?.Select(claim => new Claim(claim.Key, claim.Value)).ToArray()
                               ?? Array.Empty<Claim>();
            jwtClaims.AddRange(customClaims);
            var expires = now.AddMinutes(_options.ExpiryMinutes);
            var jwt = new JwtSecurityToken(
                issuer: _options.ValidIssuer,
                claims: jwtClaims,
                notBefore: now,
                expires: expires,
                signingCredentials: _signingCredentials
            );
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new JsonWebToken
            {
                AccessToken = token,
                RefreshToken = string.Empty,
                Expires = expires.ToTimestamp(),
                Id = userId,
                Role = role ?? string.Empty,
                Claims = customClaims.ToDictionary(c => c.Type, c => c.Value)
            };
        }

        public JsonWebTokenPayload GetTokenPayload(string accessToken)
        {
            _jwtSecurityTokenHandler.ValidateToken(accessToken, _tokenValidationParameters,
                out var validatedSecurityToken);
            if (!(validatedSecurityToken is JwtSecurityToken jwt))
            {
                return null;
            }

            return new JsonWebTokenPayload
            {
                Subject = jwt.Subject,
                Role = jwt.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Role)?.Value,
                Expires = jwt.ValidTo.ToTimestamp(),
                Claims = jwt.Claims.Where(x => !DefaultClaims.Contains(x.Type))
                    .ToDictionary(k => k.Type, v => v.Value)
            };
        }
    }
}