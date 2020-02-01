using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.WebApi.Helpers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Convey.WebApi.CQRS.Middlewares
{
    public class PublicContractsMiddleware
    {
        private const string ContentType = "application/json";
        private readonly RequestDelegate _next;
        private readonly string _endpoint;
        private readonly bool _attributeRequired;

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(new CamelCaseNamingStrategy())
            },
            Formatting = Formatting.Indented
        };

        private static readonly ContractTypes Contracts = new ContractTypes();
        private static int _initialized;
        private static string _serializedContracts = "{}";

        public PublicContractsMiddleware(RequestDelegate next, string endpoint, Type attributeType,
            bool attributeRequired)
        {
            _next = next;
            _endpoint = endpoint;
            _attributeRequired = attributeRequired;
            if (_initialized == 1)
            {
                return;
            }

            Load(attributeType); 
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path != _endpoint)
            {
                return _next(context);
            }

            context.Response.ContentType = ContentType;
            context.Response.WriteAsync(_serializedContracts);

            return Task.CompletedTask;
        }

        private void Load(Type attributeType)
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 1)
            {
                return;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var contracts = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => (!_attributeRequired || !(t.GetCustomAttribute(attributeType) is null)) && !t.IsInterface)
                .ToArray();

            foreach (var command in contracts.Where(t => typeof(ICommand).IsAssignableFrom(t)))
            {
                var instance = FormatterServices.GetUninitializedObject(command);
                var name = instance.GetType().Name;
                if (Contracts.Commands.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Command: '{name}' already exists.");
                }

                instance.SetDefaultInstanceProperties();
                Contracts.Commands[name] = instance;
            }

            foreach (var @event in contracts.Where(t => typeof(IEvent).IsAssignableFrom(t) &&
                                                        t != typeof(RejectedEvent)))
            {
                var instance = FormatterServices.GetUninitializedObject(@event);
                var name = instance.GetType().Name;
                if (Contracts.Events.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Event: '{name}' already exists.");
                }

                instance.SetDefaultInstanceProperties();
                Contracts.Events[name] = instance;
            }

            _serializedContracts = JsonConvert.SerializeObject(Contracts, SerializerSettings);
        }

        private class ContractTypes
        {
            public Dictionary<string, object> Commands { get; } = new Dictionary<string, object>();
            public Dictionary<string, object> Events { get; } = new Dictionary<string, object>();
        }
    }
}