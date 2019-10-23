using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Utf8Json;

namespace Convey.WebApi.Helpers
{
    internal class JsonOutputFormatter : IOutputFormatter
    {
        private readonly IJsonFormatterResolver _resolver;

        public JsonOutputFormatter(IJsonFormatterResolver resolver)
        {
            _resolver = resolver;
        }

        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return true;
        }

        public Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context.Object is null)
            {
                return Task.CompletedTask;
            }
            
            context.HttpContext.Response.ContentType = "application/json";
            return context.ObjectType == typeof(object)
                ? JsonSerializer.NonGeneric.SerializeAsync(context.HttpContext.Response.Body, context.Object, _resolver)
                : JsonSerializer.NonGeneric.SerializeAsync(context.ObjectType, context.HttpContext.Response.Body,
                    context.Object, _resolver);
        }
    }
}