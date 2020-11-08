using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Convey.Logging
{
    public class CorrelationContextLoggingMiddleware : IMiddleware
    {
        private readonly ILogger<CorrelationContextLoggingMiddleware> _logger;

        public CorrelationContextLoggingMiddleware(ILogger<CorrelationContextLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var headers = Activity.Current.Baggage
                .ToDictionary(x => x.Key, x => x.Value);
            using (_logger.BeginScope(headers))
            {
                return next(context);
            }
        }
    }
}