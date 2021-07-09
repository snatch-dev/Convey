using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Convey.MessageBrokers.RabbitMQ.Serializers
{
    public sealed class SystemTextJsonJsonRabbitMqSerializer : IRabbitMqSerializer
    {
        private readonly JsonSerializerOptions _options;

        public SystemTextJsonJsonRabbitMqSerializer(JsonSerializerOptions options = null)
        {
            _options = options ?? new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = {new JsonStringEnumConverter()}
            };
        }
        
        public string Serialize<T>(T value) => JsonSerializer.Serialize(value, _options);

        public string Serialize(object value) => JsonSerializer.Serialize(value, _options);

        public T Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value, _options);

        public object Deserialize(string value, Type type) => JsonSerializer.Deserialize(value, type, _options);

        public object Deserialize(string value) => JsonSerializer.Deserialize(value, typeof(object), _options);
    }
}