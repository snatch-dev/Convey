using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Convey.MessageBrokers.AzureServiceBus.Client;
using Convey.MessageBrokers.AzureServiceBus.Conventions;
using Convey.MessageBrokers.AzureServiceBus.Internals;
using Convey.MessageBrokers.AzureServiceBus.Options;
using Convey.MessageBrokers.AzureServiceBus.Providers;
using Convey.MessageBrokers.AzureServiceBus.Publishers;
using Convey.MessageBrokers.AzureServiceBus.Registries;
using Convey.MessageBrokers.AzureServiceBus.Serializers;
using Convey.MessageBrokers.AzureServiceBus.Subscribers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Convey.MessageBrokers.AzureServiceBus;

public static class Extensions
{
    /// <summary>
    /// Adds the required services for the azure service bus implementation of the Convey.MessageBrokers 
    /// </summary>
    /// <param name="builder">The <see cref="IConveyBuilder"/> instance.</param>
    /// <param name="optionsSectionName">The section name to pull the <see cref="AzureServiceBusOptions"/> from. Defaults to "AzureServiceBusOptions"</param>
    /// <param name="optionsAction">An action to allow options to be provided in code.</param>
    /// <param name="conventionsBuilder">The builder used to generate publish and subscribe conventions.</param>
    /// <param name="exceptionsToDeadLetterRegistryAction">Allows for mapping exceptions straight to the dead letter queue.</param>
    /// <returns>The provided <see cref="IConveyBuilder"/> instance.</returns>
    public static IConveyBuilder AddAzureServiceBus(
        this IConveyBuilder builder,
        string? optionsSectionName = null,
        Action<AzureServiceBusOptions>? optionsAction = null,
        Action<IExceptionHandlingRegistry>? exceptionsToDeadLetterRegistryAction = null)
    {
        optionsSectionName ??= nameof(AzureServiceBusOptions);

        builder.Services.AddOptions<AzureServiceBusOptions>()
            .Configure<IConfiguration>((options, cfg) =>
                cfg.GetSection(optionsSectionName).Bind(options));

        if (optionsAction is not null)
        {
            builder.Services.PostConfigure(optionsAction);
        }

        var exceptionsToDeadLetterRegistry = new ExceptionHandlingRegistry();
        exceptionsToDeadLetterRegistryAction?.Invoke(exceptionsToDeadLetterRegistry);
        builder.Services.AddSingleton<IExceptionHandlingRegistry>(exceptionsToDeadLetterRegistry);

        builder.Services.AddHostedService<AzureServiceBusHostedService>();
        builder.Services.AddSingleton<ISubscribersChannel, SubscribersChannel>();
        builder.Services.AddSingleton<IValidateOptions<AzureServiceBusOptions>, AzureServiceBusOptionsValidation>();
        builder.Services.AddSingleton<ISubscribersRegistry, SubscribersRegistry>();
        builder.Services.AddSingleton<IAzureBusClient, DefaultAzureBusClient>();
        builder.Services.AddSingleton<INativeClientProvider, DefaultNativeClientProvider>();
        builder.Services.AddSingleton<IConventionsBuilder, DefaultConventionsBuilder>();
        builder.Services.AddSingleton<IAzureServiceBusSerializer, SystemTextJsonSerializer>();
        builder.Services.AddSingleton<IBrokerMessageProcessorHandler, BrokerMessageProcessorHandler>();
        builder.Services.AddSingleton<IBusPublisher, AzureServiceBusPublisher>();
        builder.Services.AddSingleton<ISenderRegistry, SenderRegistry>();
        
        //TODO: figure out what this does.
        builder.Services.AddSingleton<IMessagePropertiesAccessor>(new MessagePropertiesAccessor());

        return builder;
    }


    /// <summary>
    /// Provides an <see cref="IBusSubscriber"/> to subscriber to messages.
    /// </summary>
    /// <param name="builder">The instance of the <see cref="IApplicationBuilder"/>.</param>
    /// <returns>A <see cref="IBusSubscriber"/> used to subscribe to messages.</returns>
    public static IBusSubscriber UseAzureServiceBus(this IApplicationBuilder builder) =>
        new AzureServiceBusSubscriber(builder.ApplicationServices.GetRequiredService<ISubscribersChannel>());

    /// <summary>
    /// Provides a way to subscribe to messages.
    /// </summary>
    /// <param name="builder">The instance of the <see cref="IApplicationBuilder"/>.</param>
    /// <param name="busSubscriberAction">An action which provides an <see cref="IBusSubscriber"/> to subscribe to messages.</param>
    /// <returns>The provided instance of the <see cref="IApplicationBuilder"/>.</returns>
    public static IApplicationBuilder UseAzureServiceBus(this IApplicationBuilder builder,
        Action<IBusSubscriber> busSubscriberAction)
    {
        var subscriber =
            new AzureServiceBusSubscriber(builder.ApplicationServices.GetRequiredService<ISubscribersChannel>());

        busSubscriberAction.Invoke(subscriber);

        return builder;
    }


    internal static string ToSnakeCase(this string value)
        => string.Concat(value.Select((x, i) =>
                i > 0 && value[i - 1] != '.' && value[i - 1] != '/' && char.IsUpper(x) ? "_" + x : x.ToString()))
            .ToLowerInvariant();

    internal static async Task<bool> TryCreateTopicAsync(
        this ServiceBusAdministrationClient client,
        CreateTopicOptions options)
    {
        try
        {
            await client.CreateTopicAsync(options);
        }
        catch (ServiceBusException e) when (e.Reason is ServiceBusFailureReason.MessagingEntityAlreadyExists)
        {
            return false;
        }

        return true;
    }

    internal static async Task<bool> TryCreateSubscriberAsync(
        this ServiceBusAdministrationClient client,
        CreateSubscriptionOptions options)
    {
        try
        {
            await client.CreateSubscriptionAsync(options);
        }
        catch (ServiceBusException e) when (e.Reason is ServiceBusFailureReason.MessagingEntityAlreadyExists)
        {
            return false;
        }

        return true;
    }

    internal static Dictionary<string, string> LoggingDetails(this AzureServiceBusOptions options) => new()
    {
        {nameof(AzureServiceBusOptions.ServiceName), options.ServiceName},
        {nameof(AzureServiceBusOptions.AutomaticMessageEntityCreation), options.AutomaticMessageEntityCreation.ToString()}
    };
}