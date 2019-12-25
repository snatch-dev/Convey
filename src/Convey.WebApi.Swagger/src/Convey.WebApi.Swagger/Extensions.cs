using System;
using Convey.Docs.Swagger;
using Convey.WebApi.Swagger.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.WebApi.Swagger
{
    public static class Extensions
    {
        private const string SectionName = "swagger";

        public static IConveyBuilder AddWebApiSwaggerDocs(this IConveyBuilder builder, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            return builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(sectionName));
        }
        
        public static IConveyBuilder AddWebApiSwaggerDocs(this IConveyBuilder builder, 
            Func<ISwaggerOptionsBuilder, ISwaggerOptionsBuilder> buildOptions)
            => builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(buildOptions));
        
        public static IConveyBuilder AddWebApiSwaggerDocs(this IConveyBuilder builder, SwaggerOptions options)
            => builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(options));
        
        private static IConveyBuilder AddWebApiSwaggerDocs(this IConveyBuilder builder, Action<IConveyBuilder> registerSwagger)
        {
            registerSwagger(builder);
            builder.Services.AddSwaggerGen(c => c.DocumentFilter<WebApiDocumentFilter>());
            return builder;
        }
    }
}