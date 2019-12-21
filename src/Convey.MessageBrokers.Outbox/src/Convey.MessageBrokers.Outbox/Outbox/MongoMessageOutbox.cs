using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convey.Persistence.MongoDB;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Convey.MessageBrokers.Outbox.Outbox
{
    internal sealed class MongoMessageOutbox : IMessageOutbox, IMessageOutboxAccessor
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(new CamelCaseNamingStrategy())
            }
        };

        private const string EmptyJsonObject = "{}";
        private readonly IMongoSessionFactory _sessionFactory;
        private readonly IMongoRepository<OutboxMessage, Guid> _repository;
        private readonly ILogger<MongoMessageOutbox> _logger;

        public bool Enabled { get; }

        public MongoMessageOutbox(IMongoSessionFactory sessionFactory, IMongoRepository<OutboxMessage, Guid> repository,
            OutboxOptions options, ILogger<MongoMessageOutbox> logger)
        {
            _sessionFactory = sessionFactory;
            _repository = repository;
            _logger = logger;
            Enabled = options.Enabled;
        }

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
            if (await _repository.ExistsAsync(m => m.OriginatedMessageId == messageId))
            {
                _logger.LogTrace($"Message with id: '{messageId}' was already processed.");
                return;
            }

            using var session = await _sessionFactory.CreateAsync();
            session.StartTransaction();

            try
            {
                _logger.LogTrace($"Processing a message with id: '{messageId}'...");
                await handler();
                await session.CommitTransactionAsync();
                _logger.LogTrace($"Processed a message with id: '{messageId}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"There was an error when processing a message with id: '{messageId}'.");
                await session.AbortTransactionAsync();
                throw;
            }
        }

        public async Task SendAsync<T>(T message, string originatedMessageId = null, string messageId = null,
            string correlationId = null, string spanContext = null, object messageContext = null,
            IDictionary<string, object> headers = null)
            where T : class
        {
            if (!Enabled)
            {
                _logger.LogWarning("Outbox is disabled, outgoing messages won't be saved into the storage.");
                return;
            }

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OriginatedMessageId = originatedMessageId,
                MessageId = string.IsNullOrWhiteSpace(messageId) ? Guid.NewGuid().ToString("N") : messageId,
                CorrelationId = correlationId,
                SpanContext = spanContext,
                SerializedMessageContext =
                    messageContext is null
                        ? EmptyJsonObject
                        : JsonConvert.SerializeObject(messageContext, SerializerSettings),
                MessageContextType = messageContext?.GetType().AssemblyQualifiedName,
                Headers = (Dictionary<string, object>) headers,
                SerializedMessage = message is null
                    ? EmptyJsonObject
                    : JsonConvert.SerializeObject(message, SerializerSettings),
                MessageType = message?.GetType().AssemblyQualifiedName,
                SentAt = DateTime.UtcNow
            };
            await _repository.AddAsync(outboxMessage);
        }

        async Task<IReadOnlyList<OutboxMessage>> IMessageOutboxAccessor.GetUnsentAsync()
        {
            var outboxMessages = await _repository.FindAsync(om => om.ProcessedAt == null);
            return outboxMessages.Select(om =>
            {
                if (om.MessageContextType is {})
                {
                    var messageContextType = Type.GetType(om.MessageContextType);
                    om.MessageContext = JsonConvert.DeserializeObject(om.SerializedMessageContext, messageContextType,
                        SerializerSettings);
                }

                if (om.MessageType is {})
                {
                    var messageType = Type.GetType(om.MessageType);
                    om.Message = JsonConvert.DeserializeObject(om.SerializedMessage, messageType, SerializerSettings);
                }

                return om;
            }).OrderBy(m => m.SentAt).ToList();
        }

        async Task IMessageOutboxAccessor.ProcessAsync(IEnumerable<OutboxMessage> outboxMessages)
        {
            var updateTasks = outboxMessages.Select(om =>
            {
                om.ProcessedAt = DateTime.UtcNow;
                return _repository.UpdateAsync(om);
            });

            await Task.WhenAll(updateTasks);
        }
    }
}