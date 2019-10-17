using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Convey.WebApi.Middlewares;
using Convey.WebApi.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Convey.WebApi
{
    public static class Extensions
    {
        private static readonly byte[] InvalidJsonRequestBytes = Encoding.UTF8.GetBytes("An invalid JSON was sent.");
        private static readonly JsonSerializer Serializer = new JsonSerializer
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            Converters = {new StringEnumConverter(new CamelCaseNamingStrategy())}
        };

        private const string SectionName = "webapi";
        private const string RegistryName = "webapi";
        private const string EmptyJsonObject = "{}";
        private const string LocationHeader = "Location";
        private const string JsonContentType = "application/json";

        public static IApplicationBuilder UseEndpoints(this IApplicationBuilder app, Action<IEndpointsBuilder> build)
            => app.UseRouter(router =>
            {
                var definitions = app.ApplicationServices.GetService<WebApiEndpointDefinitions>();
                build(new EndpointsBuilder(router, definitions));
            });

        public static IConveyBuilder AddWebApi(this IConveyBuilder builder, string sectionName = SectionName)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton(new WebApiEndpointDefinitions());
            builder.Services.AddRouting()
                .AddLogging()
                .AddMvcCore()
                .AddDataAnnotations()
                .AddApiExplorer()
                .AddDefaultJsonOptions()
                .AddAuthorization();

            builder.Services.Scan(s =>
                s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(c => c.AssignableTo(typeof(IRequestHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            builder.Services.AddTransient<IRequestDispatcher, RequestDispatcher>();

            return builder;
        }

        private static IMvcCoreBuilder AddDefaultJsonOptions(this IMvcCoreBuilder builder)
            => builder.AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                o.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                o.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                o.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                o.SerializerSettings.Formatting = Formatting.Indented;
                o.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

        public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder)
            => builder.UseMiddleware<ErrorHandlerMiddleware>();

        public static IApplicationBuilder UseAllForwardedHeaders(this IApplicationBuilder builder)
            => builder.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

        public static Task<TResult> DispatchAsync<TRequest, TResult>(this HttpContext httpContext, TRequest request)
            where TRequest : class, IRequest
            => httpContext.RequestServices.GetService<IRequestHandler<TRequest, TResult>>().HandleAsync(request);

        public static T Bind<T>(this T model, Expression<Func<T, object>> expression, object value)
            => model.Bind<T, object>(expression, value);

        public static T BindId<T>(this T model, Expression<Func<T, Guid>> expression)
            => model.Bind(expression, Guid.NewGuid());

        public static T BindId<T>(this T model, Expression<Func<T, string>> expression)
            => model.Bind(expression, Guid.NewGuid().ToString("N"));

        private static TModel Bind<TModel, TProperty>(this TModel model, Expression<Func<TModel, TProperty>> expression,
            object value)
        {
            if (!(expression.Body is MemberExpression memberExpression))
            {
                memberExpression = ((UnaryExpression) expression.Body).Operand as MemberExpression;
            }

            if (memberExpression is null)
            {
                throw new InvalidOperationException("Invalid member expression.");
            }

            var propertyName = memberExpression.Member.Name.ToLowerInvariant();
            var modelType = model.GetType();
            var field = modelType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(x => x.Name.ToLowerInvariant().StartsWith($"<{propertyName}>"));
            if (field == null)
            {
                return model;
            }

            field.SetValue(model, value);

            return model;
        }

        public static Task Ok(this HttpResponse response, object data = null)
        {
            response.StatusCode = 200;
            if (!(data is null))
            {
                response.WriteJson(data);
            }

            return Task.CompletedTask;
        }

        public static Task Created(this HttpResponse response, string location = null)
        {
            response.StatusCode = 201;
            if (string.IsNullOrWhiteSpace(location))
            {
                return Task.CompletedTask;
            }

            if (!response.Headers.ContainsKey(LocationHeader))
            {
                response.Headers.Add(LocationHeader, location);
            }

            return Task.CompletedTask;
        }

        public static Task Accepted(this HttpResponse response)
        {
            response.StatusCode = 202;
            return Task.CompletedTask;
        }

        public static Task NoContent(this HttpResponse response)
        {
            response.StatusCode = 204;
            return Task.CompletedTask;
        }

        public static Task BadRequest(this HttpResponse response)
        {
            response.StatusCode = 400;
            return Task.CompletedTask;
        }

        public static Task NotFound(this HttpResponse response)
        {
            response.StatusCode = 404;
            return Task.CompletedTask;
        }

        public static Task InternalServerError(this HttpResponse response)
        {
            response.StatusCode = 500;
            return Task.CompletedTask;
        }

        public static void WriteJson<T>(this HttpResponse response, T obj)
        {
            response.ContentType = JsonContentType;
            using (var writer = new HttpResponseStreamWriter(response.Body, Encoding.UTF8))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.CloseOutput = false;
                    jsonWriter.AutoCompleteOnClose = false;
                    Serializer.Serialize(jsonWriter, obj);
                }
            }
        }

        public static T ReadJson<T>(this HttpContext httpContext)
        {
            if (httpContext.Request.Body is null)
            {
                httpContext.Response.StatusCode = 400;
                httpContext.Response.Body.Write(InvalidJsonRequestBytes, 0, InvalidJsonRequestBytes.Length);

                return default;
            }

            using (var streamReader = new StreamReader(httpContext.Request.Body))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                try
                {
                    var payload = Serializer.Deserialize<T>(jsonTextReader);
                    var results = new List<ValidationResult>();
                    if (Validator.TryValidateObject(payload, new ValidationContext(payload), results))
                    {
                        return payload;
                    }

                    httpContext.Response.StatusCode = 400;
                    httpContext.Response.WriteJson(results);

                    return default;
                }
                catch
                {
                    httpContext.Response.StatusCode = 400;
                    httpContext.Response.Body.Write(InvalidJsonRequestBytes, 0, InvalidJsonRequestBytes.Length);

                    return default;
                }
            }
        }

        public static T ReadQuery<T>(this HttpContext context) where T : class
        {
            var request = context.Request;
            RouteValueDictionary values = null;
            if (HasRouteData(request))
            {
                values = request.HttpContext.GetRouteData().Values;
            }

            if (HasQueryString(request))
            {
                var queryString = HttpUtility.ParseQueryString(request.HttpContext.Request.QueryString.Value);
                values = values ?? new RouteValueDictionary();
                foreach (var key in queryString.AllKeys)
                {
                    values.TryAdd(key, queryString[key]);
                }
            }

            if (values is null)
            {
                return JsonConvert.DeserializeObject<T>(EmptyJsonObject);
            }

            var serialized = JsonConvert.SerializeObject(values)
                .Replace("\\\"", "\"")
                .Replace("\"{", "{")
                .Replace("}\"", "}")
                .Replace("\"[", "[")
                .Replace("]\"", "]");

            return JsonConvert.DeserializeObject<T>(serialized);
        }

        private static bool HasQueryString(this HttpRequest request)
            => request.Query.Any();

        private static bool HasRouteData(this HttpRequest request)
            => request.HttpContext.GetRouteData().Values.Any();

        public static string Args(this HttpContext context, string key)
            => context.Args<string>(key);

        public static T Args<T>(this HttpContext context, string key)
        {
            if (!context.GetRouteData().Values.TryGetValue(key, out var value))
            {
                return default;
            }

            if (typeof(T) == typeof(string) && value is string)
            {
                return (T) value;
            }

            var data = value?.ToString();
            if (string.IsNullOrWhiteSpace(data))
            {
                return default;
            }

            return (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(data);
        }
    }
}