using System;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartFormat;

namespace Convey.Logging.CQRS.Decorators
{
    internal sealed class CommandHandlerLoggingDecorator<TCommand> : ICommandHandler<TCommand> 
        where TCommand : class, ICommand
    {
        private readonly ICommandHandler<TCommand> _handler;
        private readonly ILogger<TCommand> _logger;
        private readonly IMessageToLogTemplateMapper _mapper;

        public CommandHandlerLoggingDecorator(ICommandHandler<TCommand> handler, ILogger<TCommand> logger, 
            IServiceProvider serviceProvider)
        {
            _handler = handler;
            _logger = logger;
            _mapper = serviceProvider.GetService<IMessageToLogTemplateMapper>() ?? new EmptyMessageToLogTemplateMapper();
        }

        public async Task HandleAsync(TCommand command)
        {
            var template = _mapper.Map(command);

            if (template is null)
            {
                await _handler.HandleAsync(command);
                return;
            }

            try
            {
                Log(command, template.Before);
                await _handler.HandleAsync(command);
                Log(command, template.After);
            }
            catch (Exception ex)
            {
                var exceptionTemplate = template.GetExceptionTemplate(ex);
                
                Log(command, exceptionTemplate, isError: true);
                throw;
            }
        }

        private void Log(TCommand command, string message, bool isError = false)
        {
            if(string.IsNullOrEmpty(message))
            {
                return;
            }

            if (isError)
            {
                _logger.LogError(Smart.Format(message, command));
            }
            else
            {
                _logger.LogInformation(Smart.Format(message, command));
            }
        }

        private class EmptyMessageToLogTemplateMapper : IMessageToLogTemplateMapper
        {
            public HandlerLogTemplate Map<TMessage>(TMessage message) where TMessage : class => null;
        }
    }
}