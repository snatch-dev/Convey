using System;
using System.Collections.Generic;
using RawRabbit.DependencyInjection;
using RawRabbit.Instantiation;

namespace Convey.MessageBrokers.RawRabbit.Registers
{
    internal sealed class RabbitMqPluginRegister : IRabbitMqPluginRegister
    {
        public IServiceProvider ServiceProvider { get; }
        
        private readonly List<(Action<IClientBuilder> buildClient, Action<IDependencyRegister>registerDependencies)> _plugins = 
            new List<(Action<IClientBuilder> buildClient, Action<IDependencyRegister> registerDependencies)>();
        
        public RabbitMqPluginRegister(IServiceProvider serviceProvider)
            => ServiceProvider = serviceProvider;

        public IRabbitMqPluginRegister AddPlugin(Action<IClientBuilder> buildClient,
            Action<IDependencyRegister> registerDependencies = null)
        {
            if (buildClient is null)
            {
                throw new InvalidOperationException("Build action for RabbitMQ plugin cannot be null");
            }
            
            _plugins.Add((buildClient, registerDependencies));
            return this;
        }

        public void Register(IDependencyRegister ioc)
            => _plugins.ForEach(t => t.registerDependencies?.Invoke(ioc));

        public void Register(IClientBuilder builder)
            => _plugins.ForEach(t => t.buildClient(builder));
    }
}