using System;

namespace Convey.Auth.Builders
{
    internal sealed class JwtOptionsBuilder : IJwtOptionsBuilder
    {
        private readonly JwtOptions _options = new JwtOptions();
        
        public IJwtOptionsBuilder WithIssuerSigningKey(string issuerSigningKey)
        {
            _options.IssuerSigningKey = issuerSigningKey;
            return this;
        }

        public IJwtOptionsBuilder WithIssuer(string issuer)
        {
            _options.ValidIssuer = issuer;
            return this;
        }

        public IJwtOptionsBuilder WithExpiry(TimeSpan expiry)
        {
            _options.Expiry = expiry;
            return this;
        }
        
        public IJwtOptionsBuilder WithExpiryMinutes(int expiryMinutes)
        {
            _options.ExpiryMinutes = expiryMinutes;
            return this;
        }

        public IJwtOptionsBuilder WithLifetimeValidation(bool validateLifetime)
        {
            _options.ValidateLifetime = validateLifetime;
            return this;
        }

        public IJwtOptionsBuilder WithAudienceValidation(bool validateAudience)
        {
            _options.ValidateAudience = validateAudience;
            return this;
        }

        public IJwtOptionsBuilder WithValidAudience(string validAudience)
        {
            _options.ValidAudience = validAudience;
            return this;
        }

        public JwtOptions Build()
            => _options;
    }
}