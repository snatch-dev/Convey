using System;
using System.Net;
using System.Threading.Tasks;
using Convey.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Utf8Json;

namespace Convey.WebApi.Middlewares
{
    internal sealed class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
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

        private static Task HandleErrorAsync(HttpContext context, Exception exception)
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
            return JsonSerializer.SerializeAsync(context.Response.Body, response, Extensions.Resolver);
        }
    }
}