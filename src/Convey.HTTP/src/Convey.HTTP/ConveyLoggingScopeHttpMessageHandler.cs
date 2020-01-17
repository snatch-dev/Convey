using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Convey.HTTP
{
    internal sealed class ConveyLoggingScopeHttpMessageHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
        private readonly HashSet<string> _maskedRequestUrlParts;
        private readonly string _maskTemplate;

        public ConveyLoggingScopeHttpMessageHandler(ILogger logger, HttpClientOptions options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            _maskedRequestUrlParts =
                new HashSet<string>(options.RequestMasking?.UrlParts ?? Enumerable.Empty<string>());
            _maskTemplate = string.IsNullOrWhiteSpace(options.RequestMasking?.MaskTemplate)
                ? "*****"
                : options.RequestMasking.MaskTemplate;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (Log.BeginRequestPipelineScope(_logger, request, _maskedRequestUrlParts, _maskTemplate))
            {
                Log.RequestPipelineStart(_logger, request, _maskedRequestUrlParts, _maskTemplate);
                var response = await base.SendAsync(request, cancellationToken);
                Log.RequestPipelineEnd(_logger, response);

                return response;
            }
        }

        private static class Log
        {
            private static class EventIds
            {
                public static readonly EventId PipelineStart = new EventId(100, "RequestPipelineStart");
                public static readonly EventId PipelineEnd = new EventId(101, "RequestPipelineEnd");
            }

            private static readonly Func<ILogger, HttpMethod, Uri, IDisposable> _beginRequestPipelineScope =
                LoggerMessage.DefineScope<HttpMethod, Uri>(
                    "HTTP {HttpMethod} {Uri}");

            private static readonly Action<ILogger, HttpMethod, Uri, Exception> _requestPipelineStart =
                LoggerMessage.Define<HttpMethod, Uri>(
                    LogLevel.Information,
                    EventIds.PipelineStart,
                    "Start processing HTTP request {HttpMethod} {Uri}");

            private static readonly Action<ILogger, HttpStatusCode, Exception> _requestPipelineEnd =
                LoggerMessage.Define<HttpStatusCode>(
                    LogLevel.Information,
                    EventIds.PipelineEnd,
                    "End processing HTTP request - {StatusCode}");

            public static IDisposable BeginRequestPipelineScope(ILogger logger, HttpRequestMessage request,
                ISet<string> maskedRequestUrlParts, string maskTemplate)
            {
                var uri = MaskUri(request.RequestUri, maskedRequestUrlParts, maskTemplate);
                return _beginRequestPipelineScope(logger, request.Method, uri);
            }

            public static void RequestPipelineStart(ILogger logger, HttpRequestMessage request,
                ISet<string> maskedRequestUrlParts, string maskTemplate)
            {
                var uri = MaskUri(request.RequestUri, maskedRequestUrlParts, maskTemplate);
                _requestPipelineStart(logger, request.Method, uri, null);
            }

            public static void RequestPipelineEnd(ILogger logger, HttpResponseMessage response)
            {
                _requestPipelineEnd(logger, response.StatusCode, null);
            }

            private static Uri MaskUri(Uri uri, ISet<string> maskedRequestUrlParts, string maskTemplate)
            {
                if (!maskedRequestUrlParts.Any())
                {
                    return uri;
                }
                
                var requestUri = uri.OriginalString;
                var hasMatch = false;
                foreach (var part in maskedRequestUrlParts)
                {
                    if (string.IsNullOrWhiteSpace(part))
                    {
                        continue;
                    }
                    
                    if (!requestUri.Contains(part))
                    {
                        continue;
                    }
                    
                    requestUri = requestUri.Replace(part, maskTemplate);
                    hasMatch = true;
                }

                return hasMatch ? new Uri(requestUri) : uri;
            }
        }
    }
}