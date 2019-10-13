using System;
using System.Collections.Generic;
using System.Linq;
using Convey.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Convey
{
    public sealed class ConveyBuilder : IConveyBuilder
    {
        private readonly List<string> _registry;
        private readonly List<Action<IServiceProvider>> _buildActions;
        private readonly IServiceCollection _services;
        IServiceCollection IConveyBuilder.Services => _services;

        private ConveyBuilder(IServiceCollection services)
        {
            _registry = new List<string>();
            _buildActions = new List<Action<IServiceProvider>>();
            _services = services;
            _services.AddSingleton<IStartupInitializer>(new StartupInitializer());
        }

        public static IConveyBuilder Create(IServiceCollection services)
            => new ConveyBuilder(services);

        public bool TryRegister(string name)
        {
            var isAlreadyRegistered = _registry.Any(r => r == name);

            if (isAlreadyRegistered)
            {
                return false;
            }

            _registry.Add(name);
            return true;
        }

        public void AddBuildAction(Action<IServiceProvider> execute)
            => _buildActions.Add(execute);

        public void AddInitializer(IInitializer initializer)
            => AddBuildAction(sp =>
            {
                var startupInitializer = sp.GetService<IStartupInitializer>();
                startupInitializer.AddInitializer(initializer);
            });

        public void AddInitializer<TInitializer>() where TInitializer : IInitializer
            => AddBuildAction(sp =>
            {
                var initializer = sp.GetService<TInitializer>();
                var startupInitializer = sp.GetService<IStartupInitializer>();
                startupInitializer.AddInitializer(initializer);
            });

        public IServiceProvider Build()
        {
            var serviceProvider = _services.BuildServiceProvider();
            _buildActions.ForEach(a => a(serviceProvider));
            return serviceProvider;
        }
    }
}