using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convey.MessageBrokers.RabbitMQ.Publishers
{
    internal sealed class RabbitMqPublisher : IBusPublisher
    {
        private readonly IRabbitMqClient _client;
        private readonly IConventionsProvider _conventionsProvider;

        public RabbitMqPublisher(IRabbitMqClient client, IConventionsProvider conventionsProvider)
        {
            _client = client;
            _conventionsProvider = conventionsProvider;
        }

        public Task PublishAsync<T>(T message, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null)
            where T : class
        {
            _client.Send(message, _conventionsProvider.Get(message.GetType()), messageId, correlationId, spanContext,
                messageContext, headers);

            return Task.CompletedTask;
        }
    }
}