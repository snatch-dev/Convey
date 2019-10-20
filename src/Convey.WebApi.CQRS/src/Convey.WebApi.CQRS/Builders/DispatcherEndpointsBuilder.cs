using System;
using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Convey.CQRS.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Convey.WebApi.CQRS.Builders
{
    public class DispatcherEndpointsBuilder : IDispatcherEndpointsBuilder
    {
        private readonly IEndpointsBuilder _builder;

        public DispatcherEndpointsBuilder(IEndpointsBuilder builder)
        {
            _builder = builder;
        }

        public IDispatcherEndpointsBuilder Get(string path, Func<HttpContext, Task> context = null)
        {
            _builder.Get(path, context);

            return this;
        }

        public IDispatcherEndpointsBuilder Get<TQuery, TResult>(string path,
            Func<TQuery, HttpContext, Task> beforeDispatch = null,
            Func<TQuery, TResult, HttpContext, Task> afterDispatch = null) where TQuery : class, IQuery<TResult>
        {
            _builder.Get<TQuery>(path, async (req, ctx) =>
            {
                if (!(beforeDispatch is null))
                {
                    await beforeDispatch(req, ctx);
                }

                var dispatcher = ctx.RequestServices.GetService<IQueryDispatcher>();
                var result = await dispatcher.QueryAsync<TQuery, TResult>(req);
                if (afterDispatch is null)
                {
                    if (result == null)
                    {
                        ctx.Response.StatusCode = 404;
                        return;
                    }

                    await ctx.Response.WriteJsonAsync(result);
                    return;
                }

                await afterDispatch(req, result, ctx);
            });

            return this;
        }

        public IDispatcherEndpointsBuilder Post(string path, Func<HttpContext, Task> context = null)
        {
            _builder.Post(path, context);

            return this;
        }

        public IDispatcherEndpointsBuilder Post<T>(string path, Func<T, HttpContext, Task> beforeDispatch = null,
            Func<T, HttpContext, Task> afterDispatch = null)
            where T : class, ICommand
        {
            _builder.Post<T>(path, (req, ctx) => BuildCommandContext(req, ctx, beforeDispatch, afterDispatch));

            return this;
        }

        public IDispatcherEndpointsBuilder Put(string path, Func<HttpContext, Task> context = null)
        {
            _builder.Put(path, context);

            return this;
        }

        public IDispatcherEndpointsBuilder Put<T>(string path, Func<T, HttpContext, Task> beforeDispatch = null,
            Func<T, HttpContext, Task> afterDispatch = null)
            where T : class, ICommand
        {
            _builder.Put<T>(path, (req, ctx) => BuildCommandContext(req, ctx, beforeDispatch, afterDispatch));

            return this;
        }

        public IDispatcherEndpointsBuilder Delete(string path, Func<HttpContext, Task> context = null)
        {
            _builder.Delete(path, context);

            return this;
        }

        public IDispatcherEndpointsBuilder Delete<T>(string path, Func<T, HttpContext, Task> beforeDispatch = null,
            Func<T, HttpContext, Task> afterDispatch = null) where T : class, ICommand
        {
            _builder.Delete<T>(path, (req, ctx) => BuildCommandContext(req, ctx, beforeDispatch, afterDispatch));

            return this;
        }

        private static async Task BuildCommandContext<T>(T request, HttpContext context,
            Func<T, HttpContext, Task> beforeDispatch = null,
            Func<T, HttpContext, Task> afterDispatch = null) where T : class, ICommand
        {
            if (!(beforeDispatch is null))
            {
                await beforeDispatch(request, context);
            }

            var dispatcher = context.RequestServices.GetService<ICommandDispatcher>();
            await dispatcher.SendAsync(request);
            if (afterDispatch is null)
            {
                context.Response.StatusCode = 200;
                return;
            }

            await afterDispatch(request, context);
        }
    }
}