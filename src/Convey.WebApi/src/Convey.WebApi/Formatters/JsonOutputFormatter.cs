using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Open.Serialization.Json;
using Utf8Json;

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
            await _serializer.SerializeAsync(context.HttpContext.Response.Body, context.Object);
        }
    }
}