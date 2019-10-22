using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Utf8Json;

namespace Convey.WebApi.Helpers
{
    internal class JsonInputFormatter : IInputFormatter
    {
        private readonly IJsonFormatterResolver _resolver;

        public JsonInputFormatter(IJsonFormatterResolver resolver)
        {
            _resolver = resolver;
        }

        public bool CanRead(InputFormatterContext context)
        {
            return true;
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var result = await JsonSerializer.NonGeneric.DeserializeAsync(context.ModelType, request.Body, _resolver);
            
            return await InputFormatterResult.SuccessAsync(result);
        }
    }
}