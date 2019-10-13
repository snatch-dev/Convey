using System;
using System.Threading.Tasks;
using Convey.CQRS.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartFormat;

namespace Convey.Logging.CQRS.Decorators
{
    internal sealed class EventHandlerLoggingDecorator<TEvent> : IEventHandler<TEvent> 
        where TEvent : class, IEvent
    {
        private readonly IEventHandler<TEvent> _handler;
        private readonly ILogger<TEvent> _logger;
        private readonly IMessageToLogTemplateMapper _mapper;

        public EventHandlerLoggingDecorator(IEventHandler<TEvent> handler, ILogger<TEvent> logger, 
            IServiceProvider serviceProvider)
        {
            _handler = handler;
            _logger = logger;
            _mapper = serviceProvider.GetService<IMessageToLogTemplateMapper>() ?? new EmptyMessageToLogTemplateMapper();
        }

        public async Task HandleAsync(TEvent @event)
        {
            var template = _mapper.Map(@event);

            if (template is null)
            {
                await _handler.HandleAsync(@event);
                return;
            }

            try
            {
                Log(@event, template.Before);
                await _handler.HandleAsync(@event);
                Log(@event, template.After);
            }
            catch (Exception ex)
            {
                var exceptionTemplate = template.GetExceptionTemplate(ex);
                
                Log(@event, exceptionTemplate, isError: true);
                throw;
            }
        }

        private void Log(TEvent @event, string message, bool isError = false)
        {
            if(string.IsNullOrEmpty(message))
            {
                return;
            }

            if (isError)
            {
                _logger.LogError(Smart.Format(message, @event));
            }
            else
            {
                _logger.LogInformation(Smart.Format(message, @event));
            }
        }

        private class EmptyMessageToLogTemplateMapper : IMessageToLogTemplateMapper
        {
            public HandlerLogTemplate Map<TMessage>(TMessage message) where TMessage : class => null;
        }
    }
}