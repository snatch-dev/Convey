using Convey.CQRS.Commands;
using Convey.CQRS.Queries;
using Convey.WebApi.CQRS.Builders;
using Convey.WebApi.CQRS.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Convey.WebApi.CQRS;

public static class Extensions
{
    public static IConveyBuilder AddInMemoryDispatcher(this IConveyBuilder builder)
    {
        builder.Services.AddSingleton<IDispatcher, InMemoryDispatcher>();
        return builder;
    }
        
    public static IApplicationBuilder UseDispatcherEndpoints(this IApplicationBuilder app,
        Action<IDispatcherEndpointsBuilder> builder, bool useAuthorization = true,
        Action<IApplicationBuilder> middleware = null)
    {
        var definitions = app.ApplicationServices.GetRequiredService<WebApiEndpointDefinitions>();
        app.UseRouting();
        if (useAuthorization)
        {
            app.UseAuthorization();
        }

        middleware?.Invoke(app);
            
        app.UseEndpoints(router => builder(new DispatcherEndpointsBuilder(
            new EndpointsBuilder(router, definitions))));

        return app;
    }

    public static IDispatcherEndpointsBuilder Dispatch(this IEndpointsBuilder endpoints,
        Func<IDispatcherEndpointsBuilder, IDispatcherEndpointsBuilder> builder)
        => builder(new DispatcherEndpointsBuilder(endpoints));

    public static IApplicationBuilder UsePublicContracts<T>(this IApplicationBuilder app,
        string endpoint = "/_contracts") => app.UsePublicContracts(endpoint, typeof(T));

    public static IApplicationBuilder UsePublicContracts(this IApplicationBuilder app,
        bool attributeRequired, string endpoint = "/_contracts")
        => app.UsePublicContracts(endpoint, null, attributeRequired);

    public static IApplicationBuilder UsePublicContracts(
        this IApplicationBuilder app,
        string endpoint = "/_contracts",
        Type attributeType = null,
        bool attributeRequired = true,
        JsonSerializerOptions jsonSerializerOptions = null)
        => app.UseMiddleware<PublicContractsMiddleware>(
            string.IsNullOrWhiteSpace(endpoint) ? "/_contracts" : endpoint.StartsWith("/") ? endpoint : $"/{endpoint}",
            attributeType ?? typeof(PublicContractAttribute),
            attributeRequired,
            jsonSerializerOptions ?? new()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                WriteIndented = true
            });

    public static Task SendAsync<T>(this HttpContext context, T command) where T : class, ICommand
        => context.RequestServices.GetRequiredService<ICommandDispatcher>().SendAsync(command);

    public static Task<TResult> QueryAsync<TResult>(this HttpContext context, IQuery<TResult> query)
        => context.RequestServices.GetRequiredService<IQueryDispatcher>().QueryAsync(query);

    public static Task<TResult> QueryAsync<TQuery, TResult>(this HttpContext context, TQuery query)
        where TQuery : class, IQuery<TResult>
        => context.RequestServices.GetRequiredService<IQueryDispatcher>().QueryAsync<TQuery, TResult>(query);
}