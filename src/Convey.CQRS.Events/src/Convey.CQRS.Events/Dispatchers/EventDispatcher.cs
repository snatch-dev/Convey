using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.CQRS.Events.Dispatchers
{
    internal sealed class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceScopeFactory _serviceFactory;

        public EventDispatcher(IServiceScopeFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task PublishAsync<T>(T @event) where T : class, IEvent
        {
            using var scope = _serviceFactory.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IEventHandler<T>>();
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(@event);
            }
        }
    }
}