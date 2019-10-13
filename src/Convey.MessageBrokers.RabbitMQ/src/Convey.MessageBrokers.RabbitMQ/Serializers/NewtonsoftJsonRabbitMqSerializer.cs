using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Convey.MessageBrokers.RabbitMQ.Serializers
{
    internal sealed class NewtonsoftJsonRabbitMqSerializer : IRabbitMqSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public NewtonsoftJsonRabbitMqSerializer(JsonSerializerSettings settings = null)
        {
            _settings = settings ?? new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
        
        public string Serialize<T>(T value) => JsonConvert.SerializeObject(value, _settings);

        public string Serialize(object value) => JsonConvert.SerializeObject(value, _settings);

        public T Deserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value, _settings);

        public object Deserialize(string value) => JsonConvert.DeserializeObject(value, _settings);
    }
}