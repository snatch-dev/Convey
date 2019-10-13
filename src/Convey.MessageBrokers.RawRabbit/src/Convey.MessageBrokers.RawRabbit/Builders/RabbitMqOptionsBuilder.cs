using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RawRabbit.Configuration;

namespace Convey.MessageBrokers.RawRabbit.Builders
{
    internal sealed class RabbitMqOptionsBuilder : IRabbitMqOptionsBuilder
    {
        private readonly RabbitMqOptions _options = new RabbitMqOptions();
        
        public IRabbitMqOptionsBuilder WithRequestTimeout(TimeSpan requestTimeout)
        {
            _options.RequestTimeout = requestTimeout;
            return this;
        }

        public IRabbitMqOptionsBuilder WithPublishConfirmTimeout(TimeSpan publishConfirmTimeout)
        {
            _options.PublishConfirmTimeout = publishConfirmTimeout;
            return this;
        }

        public IRabbitMqOptionsBuilder WithGracefulShutdown(TimeSpan gracefulShutdown)
        {
            _options.GracefulShutdown = gracefulShutdown;
            return this;
        }

        public IRabbitMqOptionsBuilder WithRouteWithGlobalId(bool routeWithGlobalId)
        {
            _options.RouteWithGlobalId = routeWithGlobalId;
            return this;
        }

        public IRabbitMqOptionsBuilder WithAutomaticRecovery(bool automaticRecovery)
        {
            _options.AutomaticRecovery = automaticRecovery;
            return this;
        }

        public IRabbitMqOptionsBuilder WithTopologyRecovery(bool topologyRecovery)
        {
            _options.TopologyRecovery = topologyRecovery;
            return this;
        }

        public IRabbitMqOptionsBuilder WithExchange(GeneralExchangeConfiguration exchange)
        {
            _options.Exchange = exchange;
            return this;
        }

        public IRabbitMqOptionsBuilder WithQueue(GeneralQueueConfiguration queue)
        {
            _options.Queue = queue;
            return this;
        }

        public IRabbitMqOptionsBuilder WithPersistentDeliveryMode(bool persistentDeliveryMode)
        {
            _options.PersistentDeliveryMode = persistentDeliveryMode;
            return this;
        }

        public IRabbitMqOptionsBuilder WithAutoCloseConnection(bool autoCloseConnection)
        {
            _options.AutoCloseConnection = autoCloseConnection;
            return this;
        }

        public IRabbitMqOptionsBuilder WithSsl(SslOption ssl)
        {
            _options.Ssl = ssl;
            return this;
        }

        public IRabbitMqOptionsBuilder WithVirtualHost(string virtualHost)
        {
            _options.VirtualHost = virtualHost;
            return this;
        }

        public IRabbitMqOptionsBuilder WithUsername(string username)
        {
            _options.Username = username;
            return this;
        }

        public IRabbitMqOptionsBuilder WithPassword(string password)
        {
            _options.Password = password;
            return this;
        }

        public IRabbitMqOptionsBuilder WithPort(int port)
        {
            _options.Port = port;
            return this;
        }

        public IRabbitMqOptionsBuilder WithHostnames(List<string> hostnames)
        {
            _options.Hostnames = hostnames;
            return this;
        }

        public IRabbitMqOptionsBuilder WithRecoveryInterval(TimeSpan recoveryInterval)
        {
            _options.RecoveryInterval = recoveryInterval;
            return this;
        }

        public IRabbitMqOptionsBuilder WithNamespace(string @namespace)
        {
            _options.Namespace = @namespace;
            return this;
        }

        public IRabbitMqOptionsBuilder WithRetries(int retries)
        {
            _options.Retries = retries;
            return this;
        }

        public IRabbitMqOptionsBuilder WithRetryInterval(int retryInterval)
        {
            _options.RetryInterval = retryInterval;
            return this;
        }

        public RabbitMqOptions Build()
            => _options;
    }
}