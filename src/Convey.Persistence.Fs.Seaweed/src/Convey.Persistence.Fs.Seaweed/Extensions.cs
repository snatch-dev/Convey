using Convey.Persistence.Fs.Seaweed.Infrastructure;
using Convey.Persistence.Fs.Seaweed.Infrastructure.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace Convey.Persistence.Fs.Seaweed
{
    public static class Extensions
    {
        private const string SectionName = "seaweed";
        public static IConveyBuilder AddSeaweed(this IConveyBuilder builder, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }
            var seaweedOptions = builder.GetOptions<SeaweedOptions>(sectionName);
            builder.Services.AddSingleton(seaweedOptions);
            if (!seaweedOptions.Enabled) return builder;

            builder.Services.AddHttpClient(seaweedOptions.FilerHttpClientName, c =>
            {
                c.BaseAddress = new Uri(seaweedOptions.FilerUrl);
                c.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse(seaweedOptions.FilerHttpClientName));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            });

            builder.Services.AddTransient<IFiler, Filer>();


            return builder;
        }
    }
}