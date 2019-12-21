using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.Outbox.Outbox
{
    public class InMemoryMessageOutbox : IMessageOutbox, IMessageOutboxAccessor
    {
        private readonly ConcurrentDictionary<string, bool> _processedMessages =
            new ConcurrentDictionary<string, bool>();

        private readonly ConcurrentDictionary<Guid, OutboxMessage> _messages =
            new ConcurrentDictionary<Guid, OutboxMessage>();

        private readonly ILogger<InMemoryMessageOutbox> _logger;
        private readonly int _expiry;

        public InMemoryMessageOutbox(OutboxOptions options, ILogger<InMemoryMessageOutbox> logger)
        {
            _logger = logger;
            _expiry = options.Expiry;
            Enabled = options.Enabled;
        }

        public bool Enabled { get; }

        public async Task HandleAsync(string messageId, Func<Task> handler)
        {
            if (!Enabled)
            {
                _logger.LogWarning("Outbox is disabled, incoming messages won't be processed.");
                return;
            }

            if (string.IsNullOrWhiteSpace(messageId))
            {
                _logger.LogTrace("Message id is empty, processing as usual...");
                await handler();
                _logger.LogTrace("Message has been processed.");
                return;
            }

            _logger.LogTrace($"Received a message with id: '{messageId}' to be processed.");
            if (_processedMessages.ContainsKey(messageId))
            {
                _logger.LogTrace($"Message with id: '{messageId}' was already processed.");
                return;
            }


            _logger.LogTrace($"Processing a message with id: '{messageId}'...");
            await handler();
            if (!_processedMessages.TryAdd(messageId, true))
            {
                _logger.LogError($"There was an error when processing a message with id: '{messageId}'.");

                throw new InvalidOperationException($"Couldn't add a processed message with id: '{messageId}'" +
                                                    $"to the internal dictionary.");
            }

            _logger.LogTrace($"Processed a message with id: '{messageId}'.");
        }

        public Task SendAsync<T>(T message, string originatedMessageId = null, string messageId = null,
            string correlationId = null, string spanContext = null, object messageContext = null,
            IDictionary<string, object> headers = null) where T : class
        {
            if (!Enabled)
            {
                _logger.LogWarning("Outbox is disabled, messages won't be saved into the storage.");
                return Task.CompletedTask;
            }

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageId = string.IsNullOrWhiteSpace(messageId) ? Guid.NewGuid().ToString("N") : messageId,
                CorrelationId = correlationId,
                SpanContext = spanContext,
                MessageContextType = messageContext?.GetType().AssemblyQualifiedName,
                Headers = (Dictionary<string, object>) headers,
                Message = message,
                MessageContext = messageContext,
                MessageType = message?.GetType().AssemblyQualifiedName,
                SentAt = DateTime.UtcNow
            };
            _messages.TryAdd(outboxMessage.Id, outboxMessage);

            return Task.CompletedTask;
        }

        Task<IReadOnlyList<OutboxMessage>> IMessageOutboxAccessor.GetUnsentAsync()
            => Task.FromResult<IReadOnlyList<OutboxMessage>>(_messages.Values
                .Where(m => m.ProcessedAt is null)
                .OrderBy(m => m.SentAt)
                .ToList());

        Task IMessageOutboxAccessor.ProcessAsync(IEnumerable<OutboxMessage> outboxMessages)
        {
            foreach (var im in outboxMessages)
            {
                im.ProcessedAt = DateTime.UtcNow;
            }
            
            if (_expiry <= 0)
            {
                return Task.CompletedTask;
            }
            
            foreach (var (id, message) in _messages)
            {
                if (!message.ProcessedAt.HasValue)
                {
                    continue;
                }

                if (message.ProcessedAt.Value.AddSeconds(_expiry) > DateTime.UtcNow)
                {
                    continue;
                }
                
                _messages.TryRemove(id, out _);
                _processedMessages.TryRemove(message.OriginatedMessageId, out _);
            }
            
            return Task.CompletedTask;
        }
    }
}