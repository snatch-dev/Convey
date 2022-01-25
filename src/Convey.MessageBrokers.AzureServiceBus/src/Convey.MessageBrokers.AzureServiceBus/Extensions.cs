using Convey.MessageBrokers.AzureServiceBus.Internals;
using Convey.MessageBrokers.AzureServiceBus.Subscribers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.MessageBrokers.AzureServiceBus;

public static class Extensions
{
    /// <summary>
    /// Adds the required services for the azure service bus implementation of the Convey.MessageBrokers 
    /// </summary>
    /// <param name="builder">The <see cref="IConveyBuilder"/> instance.</param>
    /// <param name="optionsSectionName">The section name to pull the <see cref="AzureServiceBusOptions"/> from. Defaults to "AzureServiceBusOptions"</param>
    /// <returns>The provided <see cref="IConveyBuilder"/> instance.</returns>
    public static IConveyBuilder AddAzureServiceBus(this IConveyBuilder builder, string? optionsSectionName = null)
    {
        optionsSectionName ??= nameof(AzureServiceBusOptions);

        builder.Services.AddOptions<AzureServiceBusOptions>()
            .Configure<IConfiguration>((options, cfg) => 
                cfg.GetSection(optionsSectionName).Bind(options));

        builder.Services.AddHostedService<AzureServiceBusHostedService>();
        builder.Services.AddSingleton<ISubscribersChannel, SubscribersChannel>();

        return builder;
    }

    
    /// <summary>
    /// Provides an <see cref="IBusSubscriber"/> to subscriber to messages.
    /// </summary>
    /// <param name="builder">The instance of the <see cref="IApplicationBuilder"/>.</param>
    /// <returns>A <see cref="IBusSubscriber"/> used to subscribe to messages.</returns>
    public static IBusSubscriber UseAzureServiceBus(this IApplicationBuilder builder) =>
        new AzureServiceBusSubscriber(builder.ApplicationServices.GetRequiredService<ISubscribersChannel>());
    
    public static IApplicationBuilder UseAzureServiceBus(this IApplicationBuilder builder, Action<IBusSubscriber> busSubscriberAction)
    {
        var subscriber =  new AzureServiceBusSubscriber(builder.ApplicationServices.GetRequiredService<ISubscribersChannel>());
        
        busSubscriberAction.Invoke(subscriber);

        return builder;
    }
}