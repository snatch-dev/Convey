using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Open.Serialization.Json;

namespace Convey.WebApi.Formatters
{
    internal class JsonOutputFormatter : IOutputFormatter
    {
        private readonly IJsonSerializer _serializer;

        public JsonOutputFormatter(IJsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return true;
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context.Object is null)
            {
                return;
            }

            context.HttpContext.Response.ContentType = "application/json";
            if (context.Object is string json)
            {
                await context.HttpContext.Response.WriteAsync(json);
                return;
            }
            
            await _serializer.SerializeAsync(context.HttpContext.Response.Body, context.Object);
        }
    }
}