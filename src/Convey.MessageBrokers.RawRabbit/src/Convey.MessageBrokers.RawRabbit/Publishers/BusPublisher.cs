using System.Collections.Generic;
using System.Threading.Tasks;
using RawRabbit;
using RawRabbit.Enrichers.MessageContext;

namespace Convey.MessageBrokers.RawRabbit.Publishers
{
    internal sealed class BusPublisher : IBusPublisher
    {
        private readonly IBusClient _busClient;

        public BusPublisher(IBusClient busClient)
        {
            _busClient = busClient;
        }

        public Task PublishAsync<T>(T message, string messageId = null, string correlationId = null,
            string spanContext = null, object messageContext = null, IDictionary<string, object> headers = null)
            where T : class
        {
            return _busClient.PublishAsync(message, ctx => ctx.UseMessageContext(messageContext));
        }
    }
}