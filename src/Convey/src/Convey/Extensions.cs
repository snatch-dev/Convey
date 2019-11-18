using System;
using System.Linq;
using System.Threading.Tasks;
using Convey.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Convey
{
    public static class Extensions
    {
        public static IConveyBuilder AddConvey(this IServiceCollection services, string appOptionsSectionName = "app")
        {
            var builder = ConveyBuilder.Create(services);
            var options = builder.GetOptions<AppOptions>(appOptionsSectionName);
            builder.Services.AddMemoryCache();
            services.AddSingleton(options);
            services.AddSingleton<IServiceId, ServiceId>();
            if (options.DisplayBanner && !string.IsNullOrWhiteSpace(options.Name))
            {
                Console.WriteLine(Figgle.FiggleFonts.Doom.Render($"{options.Name} {options.Version}"));
            }

            return builder;
        }

        public static IApplicationBuilder UseConvey(this IApplicationBuilder app)
            => app.UseInitializers();

        public static TModel GetOptions<TModel>(this IConfiguration configuration, string sectionName)
            where TModel : new()
        {
            var model = new TModel();
            configuration.GetSection(sectionName).Bind(model);
            return model;
        }

        public static TModel GetOptions<TModel>(this IConveyBuilder builder, string settingsSectionName)
            where TModel : new()
        {
            using (var serviceProvider = builder.Services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                return configuration.GetOptions<TModel>(settingsSectionName);
            }
        }

        public static IApplicationBuilder UseInitializers(this IApplicationBuilder builder)
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetService<IStartupInitializer>();
                if (initializer is null)
                {
                    throw new InvalidOperationException("Startup initializer was not found.");
                }

                Task.Run(() => initializer.InitializeAsync()).GetAwaiter().GetResult();
            }

            return builder;
        }

        public static string Underscore(this string value)
            => string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));
    }
}