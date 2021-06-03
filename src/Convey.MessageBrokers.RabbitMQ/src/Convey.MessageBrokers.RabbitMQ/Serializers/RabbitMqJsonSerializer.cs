using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Convey.MessageBrokers.RabbitMQ.Serializers
{
    internal sealed class RabbitMqJsonSerializer : IRabbitMqSerializer
    {
        private readonly JsonSerializerOptions _options;

        public RabbitMqJsonSerializer(JsonSerializerOptions options = null)
        {
            _options = options ?? new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
        
        public string Serialize<T>(T value) => JsonSerializer.Serialize(value, _options);

        public string Serialize(object value) => JsonSerializer.Serialize(value, _options);

        public T Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value, _options);

        public object Deserialize(string value) => JsonSerializer.Deserialize<ExpandoObject>(value, _options);
    }
}
