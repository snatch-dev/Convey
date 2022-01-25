namespace Convey.MessageBrokers.AzureServiceBus;

public class AzureServiceBusOptions
{
    /// <summary>
    /// A friendly name to identify the service publishing or subscribing to messages.
    /// </summary>
    public string ServiceName { get; set; } = "my_service";

    /// <summary>
    /// Whether or not topics and subscribers will be created by the library.
    /// </summary>
    public bool AutomaticMessageEntityCreation { get; set; }

    /// <summary>
    /// The options on how to connect to the service bus instance.
    /// </summary>
    public ConnectionOptions Connection { get; set; } = new();
}

public class ConnectionOptions
{
    public string? ConnectionString { get; set; }
}