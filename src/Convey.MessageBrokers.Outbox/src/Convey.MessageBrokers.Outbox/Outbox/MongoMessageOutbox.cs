using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convey.Persistence.MongoDB;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Convey.MessageBrokers.Outbox.Outbox
{
    internal sealed class MongoMessageOutbox : IMessageOutbox, IMessageOutboxAccessor
    {
        private readonly IMongoRepository<OutboxMessage, Guid> _repository;
        private const string EmptyJsonObject = "{}";

        public MongoMessageOutbox(IMongoRepository<OutboxMessage, Guid> repository)
            => _repository = repository;
        
        public async Task SendAsync<T>(T message, string messageId = null, string correlationId = null, 
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null)
        where T : class
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                CorrelationId = correlationId,
                SpanContext = spanContext,
                SerializedMessageContext = messageContext is null ? EmptyJsonObject : JsonConvert.SerializeObject(messageContext),
                MessageContextType = messageContext?.GetType().AssemblyQualifiedName,
                Headers = (Dictionary<string, object>) headers,
                SerializedMessage = message is null? EmptyJsonObject : JsonConvert.SerializeObject(message),
                MessageType = message?.GetType().AssemblyQualifiedName,
                SentAt = DateTime.UtcNow
            };
            await _repository.AddAsync(outboxMessage);
        }

        async Task<IEnumerable<OutboxMessage>> IMessageOutboxAccessor.GetUnsentAsync()
        {
            var outboxMessages = await _repository.FindAsync(om => om.ProcessedAt == null);

            return outboxMessages.Select(om =>
            {
                if (om.MessageContextType is {})
                {
                    var messageContextType = Type.GetType(om.MessageContextType);
                    om.MessageContext = JsonConvert.DeserializeObject(om.SerializedMessageContext, messageContextType);
                }
                if (om.MessageType is {})
                {
                    var messageType = Type.GetType(om.MessageType);
                    om.Message = JsonConvert.DeserializeObject(om.SerializedMessage, messageType);
                }
                return om;
            });
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