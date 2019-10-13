using System;
using System.Text;
using Convey.Auth.Builders;
using Convey.Auth.Handlers;
using Convey.Auth.Services;
using Convey.Persistence.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Convey.Auth
{
    public static class Extensions
    {
        private const string SectionName = "jwt";
        private const string RegistryName = "auth";

        public static IConveyBuilder AddJwt(this IConveyBuilder builder, string sectionName = SectionName,
            string redisSectionName = "redis")
        {
            var options = builder.GetOptions<JwtOptions>(sectionName);
            var redisOptions = builder.GetOptions<RedisOptions>(redisSectionName);
            return builder.AddJwt(options, b => b.AddRedis(redisOptions));
        }

        public static IConveyBuilder AddJwt(this IConveyBuilder builder,
            Func<IJwtOptionsBuilder, IJwtOptionsBuilder> buildOptions,
            Func<IRedisOptionsBuilder, IRedisOptionsBuilder> buildRedisOptions = null)
        {
            var options = buildOptions(new JwtOptionsBuilder()).Build();
            return buildRedisOptions is null
                ? builder.AddJwt(options)
                : builder.AddJwt(options, b => b.AddRedis(buildRedisOptions));
        }

        public static IConveyBuilder AddJwt(this IConveyBuilder builder, JwtOptions options,
            RedisOptions redisOptions = null)
            => builder.AddJwt(options, b => b.AddRedis(redisOptions ?? new RedisOptions()));

        private static IConveyBuilder AddJwt(this IConveyBuilder builder, JwtOptions options,
            Action<IConveyBuilder> registerRedis)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            registerRedis(builder);
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IJwtHandler, JwtHandler>();
            builder.Services.AddTransient<IAccessTokenService, AccessTokenService>();
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
                        ValidateLifetime = options.ValidateLifetime
                    };
                });

            return builder;
        }

        public static IApplicationBuilder UseAccessTokenValidator(this IApplicationBuilder app)
            => app.UseMiddleware<AccessTokenValidatorMiddleware>();
    }
}