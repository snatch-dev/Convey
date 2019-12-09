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

        private readonly IMongoRepository<OutboxMessage, Guid> _repository;
        private readonly ILogger<MongoMessageOutbox> _logger;
        private const string EmptyJsonObject = "{}";
        
        public bool Enabled { get; }

        public MongoMessageOutbox(IMongoRepository<OutboxMessage, Guid> repository, OutboxOptions options,
            ILogger<MongoMessageOutbox> logger)
        {
            _repository = repository;
            _logger = logger;
            Enabled = options.Enabled;
        }

        public async Task SendAsync<T>(T message, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null,
            string userId = null) where T : class
        {
            if (!Enabled)
            {
                _logger.LogWarning("Outbox is disabled, messages will not be sent.");
                return;
            }

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageId = string.IsNullOrWhiteSpace(messageId) ? Guid.NewGuid().ToString("N") : messageId,
                CorrelationId = correlationId,
                UserId = userId,
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
            }).ToList();
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