using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace Convey.MessageBrokers.RabbitMQ.Middleware
{
    public class UniqueMessagesMiddleware : IUniqueMessagesMiddleware
    {
        private readonly IMessageProcessor _messageProcessor;
        private readonly ILogger<UniqueMessagesMiddleware> _logger;
        private readonly bool _loggerEnabled;

        public UniqueMessagesMiddleware(IMessageProcessor messageProcessor, RabbitMqOptions options,
            ILogger<UniqueMessagesMiddleware> logger)
        {
            _messageProcessor = messageProcessor;
            _logger = logger;
            _loggerEnabled = options.Logger?.Enabled == true;
        }

        public async Task HandleAsync(Func<Task> next, object message, object correlationContext,
            BasicDeliverEventArgs args)
        {
            var messageId = args.BasicProperties.MessageId;
            if (_loggerEnabled)
            {
                _logger.LogTrace($"Received a unique message with id: '{messageId}' to be processed.");
            }

            if (!await _messageProcessor.TryProcessAsync(messageId))
            {
                if (_loggerEnabled)
                {
                    _logger.LogTrace($"A unique message with id: '{messageId}' was already processed.");
                }

                return;
            }

            try
            {
                if (_loggerEnabled)
                {
                    _logger.LogTrace($"Processing a unique message with id: '{messageId}'...");
                }

                await next();

                if (_loggerEnabled)
                {
                    _logger.LogTrace($"Processed a unique message with id: '{messageId}'.");
                }
            }
            catch
            {
                if (_loggerEnabled)
                {
                    _logger.LogTrace($"There was an error when processing a unique message with id: '{messageId}'.");
                }

                await _messageProcessor.RemoveAsync(messageId);
                throw;
            }
        }
    }
}