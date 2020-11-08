using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Convey.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Convey.WebApi
{
    public class EndpointsBuilder : IEndpointsBuilder
    {
        private readonly WebApiEndpointDefinitions _definitions;
        private readonly IEndpointRouteBuilder _routeBuilder;

        public EndpointsBuilder(IEndpointRouteBuilder routeBuilder, WebApiEndpointDefinitions definitions)
        {
            _routeBuilder = routeBuilder;
            _definitions = definitions;
        }

        public IEndpointsBuilder Get(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
        {
            var builder = _routeBuilder.MapGet(path, ctx => context?.Invoke(ctx));
            endpoint?.Invoke(builder);
            ApplyAuthRolesAndPolicies(builder, auth, roles, policies);
            AddEndpointDefinition(HttpMethods.Get, path);

            return this;
        }

        public IEndpointsBuilder Get<T>(string path, Func<T, HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
            where T : class
        {
            var builder = _routeBuilder.MapGet(path, ctx => BuildQueryContext(ctx, context));
            endpoint?.Invoke(builder);
            ApplyAuthRolesAndPolicies(builder, auth, roles, policies);
            AddEndpointDefinition<T>(HttpMethods.Get, path);

            return this;
        }

        public IEndpointsBuilder Get<TRequest, TResult>(string path, Func<TRequest, HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
            where TRequest : class
        {
            var builder = _routeBuilder.MapGet(path, ctx => BuildQueryContext(ctx, context));
            endpoint?.Invoke(builder);
            ApplyAuthRolesAndPolicies(builder, auth,  roles, policies);
            AddEndpointDefinition<TRequest, TResult>(HttpMethods.Get, path);

            return this;
        }

        public IEndpointsBuilder Post(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
        {
            var builder = _routeBuilder.MapPost(path, ctx => context?.Invoke(ctx));
            endpoint?.Invoke(builder);
            ApplyAuthRolesAndPolicies(builder, auth,  roles, policies);
            AddEndpointDefinition(HttpMethods.Post, path);

            return this;
        }

        public IEndpointsBuilder Post<T>(string path, Func<T, HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
            where T : class
        {
            var builder = _routeBuilder.MapPost(path, ctx => BuildRequestContext(ctx, context));
            endpoint?.Invoke(builder);
            ApplyAuthRolesAndPolicies(builder, auth,  roles, policies);
            AddEndpointDefinition<T>(HttpMethods.Post, path);

            return this;
        }

        public IEndpointsBuilder Put(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
        {
            var builder = _routeBuilder.MapPut(path, ctx => context?.Invoke(ctx));
            endpoint?.Invoke(builder);
            ApplyAuthRolesAndPolicies(builder, auth,  roles, policies);
            AddEndpointDefinition(HttpMethods.Put, path);

            return this;
        }

        public IEndpointsBuilder Put<T>(string path, Func<T, HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
            where T : class
        {
            var builder = _routeBuilder.MapPut(path, ctx => BuildRequestContext(ctx, context));
            endpoint?.Invoke(builder);
            ApplyAuthRolesAndPolicies(builder, auth,  roles, policies);
            AddEndpointDefinition<T>(HttpMethods.Put, path);

            return this;
        }

        public IEndpointsBuilder Delete(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
        {
            var builder = _routeBuilder.MapDelete(path, ctx => context?.Invoke(ctx));
            endpoint?.Invoke(builder);
            ApplyAuthRolesAndPolicies(builder, auth,  roles, policies);
            AddEndpointDefinition(HttpMethods.Delete, path);

            return this;
        }

        public IEndpointsBuilder Delete<T>(string path, Func<T, HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
            where T : class
        {
            var builder = _routeBuilder.MapDelete(path, ctx => BuildQueryContext(ctx, context));
            endpoint?.Invoke(builder);
            ApplyAuthRolesAndPolicies(builder, auth,  roles, policies);
            AddEndpointDefinition<T>(HttpMethods.Delete, path);

            return this;
        }

        private static void ApplyAuthRolesAndPolicies(IEndpointConventionBuilder builder, bool auth, string roles,
            params string[] policies)
        {
            if (policies is {} && policies.Any())
            {
                builder.RequireAuthorization(policies);
                return;
            }

            var hasRoles = !string.IsNullOrWhiteSpace(roles);
            var authorize = new AuthorizeAttribute();
            if (hasRoles)
            {
                authorize.Roles = roles;
            }

            if (auth || hasRoles)
            {
                builder.RequireAuthorization(authorize);
            }
        }

        private static async Task BuildRequestContext<T>(HttpContext httpContext,
            Func<T, HttpContext, Task> context = null)
            where T : class
        {
            var request = await httpContext.ReadJsonAsync<T>();
            if (request is null || context is null)
            {
                return;
            }

            await context.Invoke(request, httpContext);
        }

        private static async Task BuildQueryContext<T>(HttpContext httpContext,
            Func<T, HttpContext, Task> context = null)
            where T : class
        {
            var request = httpContext.ReadQuery<T>();
            if (request is null || context is null)
            {
                return;
            }

            await context.Invoke(request, httpContext);
        }

        private void AddEndpointDefinition(string method, string path)
        {
            _definitions.Add(new WebApiEndpointDefinition
            {
                Method = method,
                Path = path,
                Responses = new List<WebApiEndpointResponse>
                {
                    new WebApiEndpointResponse
                    {
                        StatusCode = 200
                    }
                }
            });
        }

        private void AddEndpointDefinition<T>(string method, string path)
            => AddEndpointDefinition(method, path, typeof(T), null);

        private void AddEndpointDefinition<T, U>(string method, string path)
            => AddEndpointDefinition(method, path, typeof(T), typeof(U));

        private void AddEndpointDefinition(string method, string path, Type input, Type output)
        {
            if (_definitions.Exists(d => d.Path == path))
            {
                return;
            }

            _definitions.Add(new WebApiEndpointDefinition
            {
                Method = method,
                Path = path,
                Parameters = new List<WebApiEndpointParameter>
                {
                    new WebApiEndpointParameter
                    {
                        In = method == HttpMethods.Get ? "query" : "body",
                        Name = input?.Name,
                        Type = input?.Name,
                        Example = input is null
                            ? null
                            : FormatterServices.GetUninitializedObject(input).SetDefaultInstanceProperties()
                    }
                },
                Responses = new List<WebApiEndpointResponse>
                {
                    new WebApiEndpointResponse
                    {
                        StatusCode = method == HttpMethods.Get ? 200 : 202,
                        Type = output?.Name,
                        Example = output is null
                            ? null
                            : FormatterServices.GetUninitializedObject(output).SetDefaultInstanceProperties()
                    }
                }
            });
        }
    }
}