using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Convey.MessageBrokers.AzureServiceBus.Serializers;

public sealed class NewtonsoftJsonSerializer : IAzureServiceBusSerializer
{
    private readonly JsonSerializerSettings _settings;

    public NewtonsoftJsonSerializer(JsonSerializerSettings? settings = null)
    {
        _settings = settings ?? new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
    }

    public ReadOnlySpan<byte>  Serialize(object value) => 
        Encode(JsonConvert.SerializeObject(value, _settings));

    public object Deserialize(ReadOnlySpan<byte> value, Type type) => 
        JsonConvert.DeserializeObject(Decode(value), type, _settings)!;

    public object Deserialize(ReadOnlySpan<byte> value) => 
        JsonConvert.DeserializeObject(Decode(value), _settings)!;

    private static ReadOnlySpan<byte>  Encode(string value) => 
        Encoding.UTF8.GetBytes(value);
        
    private static string Decode(ReadOnlySpan<byte> value) => 
        Encoding.UTF8.GetString(value);
}