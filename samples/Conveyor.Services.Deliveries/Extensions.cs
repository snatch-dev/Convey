using Convey;
using Convey.MessageBrokers;
using Convey.MessageBrokers.AzureServiceBus;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Tracing.Jaeger.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Conveyor.Services.Deliveries;

public static class Extensions
{
    public static IConveyBuilder AddMessageBroker(this IConveyBuilder builder)
    {
        var options = builder.GetOptions<Broker>("broker");
        if (options.Type == "Azure")
        {
            builder.AddAzureServiceBus();
        }
        else
        {
            builder.AddRabbitMq(plugins: p => p.AddJaegerRabbitMqPlugin());
        }

        return builder;
    }

    public static IBusSubscriber UseMessageBroker(this IApplicationBuilder builder)
    {
        var config = builder.ApplicationServices.GetRequiredService<IConfiguration>();
        return config.GetSection("broker").GetValue<string>("type") == "Azure"
            ? builder.UseAzureServiceBus()
            : builder.UseRabbitMq();
    }

    public class Broker
    {
        public string Type { get; set; } = "Azure";
    }
}