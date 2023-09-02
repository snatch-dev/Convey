using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.WebApi.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Convey.WebApi.CQRS.Middlewares;

public class PublicContractsMiddleware
{
    private const string ContentType = "application/json";

    private static readonly ContractTypes Contracts = new();
    private static int _initialized;
    private static string _serializedContracts = "{}";

    private readonly RequestDelegate _next;
    private readonly string _endpoint;

    public PublicContractsMiddleware(
        RequestDelegate next,
        string endpoint,
        Type attributeType,
        bool attributeRequired,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _next = next;
        _endpoint = endpoint;

        if (_initialized == 1)
        {
            return;
        }

        jsonSerializerOptions ??= new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            WriteIndented = true
        };

        Load(attributeType, attributeRequired, jsonSerializerOptions);
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

    private void Load(
        Type attributeType,
        bool attributeRequired,
        JsonSerializerOptions jsonSerializerOptions)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return;
        }

        var contractTypes =
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    !t.IsInterface &&
                    (!attributeRequired || t.GetCustomAttribute(attributeType) is not null) &&
                    (typeof(ICommand).IsAssignableFrom(t) ||
                        typeof(IEvent).IsAssignableFrom(t)))
                .ToArray();

        foreach (var command in contractTypes.Where(t => typeof(ICommand).IsAssignableFrom(t)))
        {
            var instance = command.GetDefaultInstance();
            var name = instance.GetType().Name;

            if (Contracts.Commands.ContainsKey(name))
            {
                throw new InvalidOperationException($"Command: '{name}' already exists.");
            }

            Contracts.Commands[name] = instance;
        }

        foreach (var @event in contractTypes.Where(t => typeof(IEvent).IsAssignableFrom(t) && t != typeof(RejectedEvent)))
        {
            var instance = @event.GetDefaultInstance();
            var name = instance.GetType().Name;

            if (Contracts.Events.ContainsKey(name))
            {
                throw new InvalidOperationException($"Event: '{name}' already exists.");
            }

            Contracts.Events[name] = instance;
        }

        _serializedContracts = JsonSerializer.Serialize(Contracts, jsonSerializerOptions);
    }

    private class ContractTypes
    {
        public Dictionary<string, object> Commands { get; } = new();
        public Dictionary<string, object> Events { get; } = new();
    }
}