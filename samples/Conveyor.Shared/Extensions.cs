using Convey;
using Convey.MessageBrokers;
using Convey.MessageBrokers.AzureServiceBus;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Tracing.Jaeger.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Conveyor.Shared;

public static class Extensions
{
    public static IConveyBuilder AddMessageBroker(this IConveyBuilder builder)
    {
        var options = builder.GetOptions<MessageBrokerOptions>("brokerOptions");
        builder.Services.AddSingleton(options);

        return options.Provider switch
        {
            MessageBrokers.Azure => builder.AddAzureServiceBus(),
            MessageBrokers.RabbitMQ => builder.AddRabbitMq(plugins: p => p.AddJaegerRabbitMqPlugin()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static IBusSubscriber UseMessageBroker(this IApplicationBuilder builder) =>
        builder.ApplicationServices.GetRequiredService<MessageBrokerOptions>()
            .Provider is MessageBrokers.Azure
            ? builder.UseAzureServiceBus()
            : builder.UseRabbitMq();
}