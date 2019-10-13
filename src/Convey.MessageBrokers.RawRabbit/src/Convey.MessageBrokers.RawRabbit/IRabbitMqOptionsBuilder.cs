using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RawRabbit.Configuration;

namespace Convey.MessageBrokers.RawRabbit
{
    public interface IRabbitMqOptionsBuilder
    {
        IRabbitMqOptionsBuilder WithRequestTimeout(TimeSpan requestTimeout);
        IRabbitMqOptionsBuilder WithPublishConfirmTimeout(TimeSpan publishConfirmTimeout);
        IRabbitMqOptionsBuilder WithGracefulShutdown(TimeSpan gracefulShutdown);
        IRabbitMqOptionsBuilder WithRouteWithGlobalId(bool routeWithGlobalId);
        IRabbitMqOptionsBuilder WithAutomaticRecovery(bool automaticRecovery);
        IRabbitMqOptionsBuilder WithTopologyRecovery(bool topologyRecovery);
        IRabbitMqOptionsBuilder WithExchange(GeneralExchangeConfiguration exchange);
        IRabbitMqOptionsBuilder WithQueue(GeneralQueueConfiguration queue);
        IRabbitMqOptionsBuilder WithPersistentDeliveryMode(bool persistentDeliveryMode);
        IRabbitMqOptionsBuilder WithAutoCloseConnection(bool autoCloseConnection);
        IRabbitMqOptionsBuilder WithSsl(SslOption ssl);
        IRabbitMqOptionsBuilder WithVirtualHost(string virtualHost);
        IRabbitMqOptionsBuilder WithUsername(string username);
        IRabbitMqOptionsBuilder WithPassword(string password);
        IRabbitMqOptionsBuilder WithPort(int port);
        IRabbitMqOptionsBuilder WithHostnames(List<string> hostnames);
        IRabbitMqOptionsBuilder WithRecoveryInterval(TimeSpan recoveryInterval);
        IRabbitMqOptionsBuilder WithNamespace(string @namespace);
        IRabbitMqOptionsBuilder WithRetries(int retries);
        IRabbitMqOptionsBuilder WithRetryInterval(int retryInterval);
        RabbitMqOptions Build();
    }
}