namespace Convey.MessageBrokers.AzureServiceBus.Serializers;

public interface IAzureServiceBusSerializer
{
    ReadOnlySpan<byte>  Serialize(object value);
    object Deserialize(ReadOnlySpan<byte> value, Type type);
    object Deserialize(ReadOnlySpan<byte> value);
}