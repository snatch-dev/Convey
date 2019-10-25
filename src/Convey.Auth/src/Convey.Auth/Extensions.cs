using System;
using System.Text;
using Convey.Auth.Builders;
using Convey.Auth.Handlers;
using Convey.Auth.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Convey.Auth
{
    public static class Extensions
    {
        private const string SectionName = "jwt";
        private const string RegistryName = "auth";

        public static IConveyBuilder AddJwt(this IConveyBuilder builder, string sectionName = SectionName)
        {
            var options = builder.GetOptions<JwtOptions>(sectionName);
            return builder.AddJwt(options);
        }

        private static IConveyBuilder AddJwt(this IConveyBuilder builder, JwtOptions options)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IJwtHandler, JwtHandler>();
            builder.Services.AddSingleton<IAccessTokenService, InMemoryAccessTokenService>();
            builder.Services.AddTransient<AccessTokenValidatorMiddleware>();
            builder.Services.AddAuthentication()
                .AddJwtBearer(cfg =>
                {
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey)),
                        ValidIssuer = options.Issuer,
                        ValidAudience = options.ValidAudience,
                        ValidateAudience = options.ValidateAudience,
                        ValidateLifetime = options.ValidateLifetime,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return builder;
        }

        public static IApplicationBuilder UseAccessTokenValidator(this IApplicationBuilder app)
            => app.UseMiddleware<AccessTokenValidatorMiddleware>();
    }
}