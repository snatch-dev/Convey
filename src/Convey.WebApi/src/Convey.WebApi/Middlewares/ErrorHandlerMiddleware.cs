using System;
using System.Net;
using System.Threading.Tasks;
using Convey.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Open.Serialization.Json;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Convey.WebApi.Middlewares
{
    internal sealed class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, IJsonSerializer jsonSerializer,
            ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _jsonSerializer = jsonSerializer;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                await HandleErrorAsync(context, exception);
            }
        }

        private async Task HandleErrorAsync(HttpContext context, Exception exception)
        {
            var code = "error";
            var statusCode = HttpStatusCode.BadRequest;
            var message = exception.Message;
            switch (exception)
            {
                case ConveyException ex:
                    code = ex.Code;
                    message = ex.Message;
                    break;
            }

            var response = new {code, message};
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) statusCode;
            await _jsonSerializer.SerializeAsync(context.Response.Body, response);
        }
    }
}