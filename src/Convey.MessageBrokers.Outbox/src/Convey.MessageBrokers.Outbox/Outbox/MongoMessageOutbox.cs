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

        public MongoMessageOutbox(IMongoRepository<OutboxMessage, Guid> repository)
            => _repository = repository;
        
        public async Task SendAsync<T>(T message)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageId = Guid.NewGuid(),
                SerializedMessage = JsonConvert.SerializeObject(message),
                Type = message.GetType().AssemblyQualifiedName,
                SentAt = DateTime.UtcNow
            };
            await _repository.AddAsync(outboxMessage);
        }

        async Task<IEnumerable<OutboxMessage>> IMessageOutboxAccessor.GetUnsentAsync()
        {
            var outboxMessages = await _repository.FindAsync(om => om.ProcessedAt == null);

            return outboxMessages.Select(om =>
            {
                var type = Type.GetType(om.Type);
                om.Message = JsonConvert.DeserializeObject(om.SerializedMessage, type);
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