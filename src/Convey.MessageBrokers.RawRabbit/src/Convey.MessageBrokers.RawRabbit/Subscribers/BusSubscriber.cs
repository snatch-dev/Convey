using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RawRabbit;
using RawRabbit.Common;
using RawRabbit.Enrichers.MessageContext;

namespace Convey.MessageBrokers.RawRabbit.Subscribers
{
    internal sealed class BusSubscriber : IBusSubscriber
    {
        private readonly ILogger _logger;
        private readonly IBusClient _busClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _retries;
        private readonly int _retryInterval;
        private readonly IExceptionToMessageMapper _exceptionToMessageMapper;

        public BusSubscriber(IApplicationBuilder app)
        {
            _logger = app.ApplicationServices.GetService<ILogger<BusSubscriber>>();
            _serviceProvider = app.ApplicationServices.GetService<IServiceProvider>();
            _busClient = _serviceProvider.GetService<IBusClient>();
            _exceptionToMessageMapper = _serviceProvider.GetService<IExceptionToMessageMapper>() ??
                                        new EmptyExceptionToMessageMapper();
            var options = _serviceProvider.GetService<RabbitMqOptions>();
            _retries = options.Retries >= 0 ? options.Retries : 3;
            _retryInterval = options.RetryInterval > 0 ? options.RetryInterval : 2;
        }

        public IBusSubscriber Subscribe<TMessage>(Func<IServiceProvider, TMessage, object, Task> handle)
            where TMessage : class
        {
            _busClient.SubscribeAsync<TMessage, object>(async (message, correlationContext) =>
            {
                try
                {
                    var accessor = _serviceProvider.GetService<ICorrelationContextAccessor>();
                    accessor.CorrelationContext = correlationContext;
                    var exception = await TryHandleAsync(message, correlationContext, handle);
                    if (exception is null)
                    {
                        return new Ack();
                    }

                    throw exception;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            }).GetAwaiter().GetResult();

            return this;
        }

        private Task<Exception> TryHandleAsync<TMessage>(TMessage message, object correlationContext,
            Func<IServiceProvider, TMessage, object, Task> handle)
        {
            var currentRetry = 0;
            var messageName = message.GetMessageName();
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retries, i => TimeSpan.FromSeconds(_retryInterval));

            return retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var retryMessage = currentRetry == 0
                        ? string.Empty
                        : $"Retry: {currentRetry}'.";

                    var preLogMessage = $"Handling a message: '{messageName}'. {retryMessage}";

                    _logger.LogInformation(preLogMessage);

                    await handle(_serviceProvider, message, correlationContext);

                    var postLogMessage = $"Handled a message: '{messageName}'. {retryMessage}";
                    _logger.LogInformation(postLogMessage);

                    return null;
                }
                catch (Exception ex)
                {
                    currentRetry++;
                    _logger.LogError(ex, ex.Message);
                    var rejectedEvent = _exceptionToMessageMapper.Map(ex, message);
                    if (rejectedEvent is null)
                    {
                        throw new Exception($"Unable to handle a message: '{messageName}' " +
                                            $"retry {currentRetry - 1}/{_retries}...", ex);
                    }

                    await _busClient.PublishAsync(rejectedEvent, ctx => ctx.UseMessageContext(correlationContext));
                    _logger.LogWarning($"Published a rejected event: '{rejectedEvent.GetMessageName()}' " +
                                       $"for the message: '{messageName}'.");

                    return new Exception($"Handling a message: '{messageName}' failed and rejected event: " +
                                         $"'{rejectedEvent.GetMessageName()}' was published.", ex);
                }
            });
        }

        private class EmptyExceptionToMessageMapper : IExceptionToMessageMapper
        {
            public object Map(Exception exception, object message) => null;
        }

        public void Dispose()
        {
        }
    }
}