using System;
using System.Collections.Generic;

namespace Convey.HTTP.RestEase.Builders
{
    internal sealed class RestEaseOptionsBuilder : IRestEaseOptionsBuilder
    {
        private readonly RestEaseOptions _options = new RestEaseOptions();
        private readonly List<RestEaseOptions.Service> _services = new List<RestEaseOptions.Service>();
        
        public IRestEaseOptionsBuilder WithLoadBalancer(string loadBalancer)
        {
            _options.LoadBalancer = loadBalancer;
            return this;
        }

        public IRestEaseOptionsBuilder WithService(Func<IRestEaseServiceBuilder, IRestEaseServiceBuilder> buildService)
        {
            var service = buildService(new RestEaseServiceBuilder()).Build();
            _services.Add(service);
            return this;
        }

        public RestEaseOptions Build()
        {
            _options.Services = _services;
            return _options;
        }

        private class RestEaseServiceBuilder : IRestEaseServiceBuilder
        {
            private readonly RestEaseOptions.Service _service = new RestEaseOptions.Service();
            
            public IRestEaseServiceBuilder WithName(string name)
            {
                _service.Name = name;
                return this;
            }

            public IRestEaseServiceBuilder WithScheme(string scheme)
            {
                _service.Scheme = scheme;
                return this;
            }

            public IRestEaseServiceBuilder WithHost(string host)
            {
                _service.Host = host;
                return this;
            }

            public IRestEaseServiceBuilder WithPort(int port)
            {
                _service.Port = port;
                return this;
            }

            public RestEaseOptions.Service Build() => _service;
        }
    }
}