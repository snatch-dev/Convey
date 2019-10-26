using System;
using System.Threading.Tasks;
using Convey;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.Discovery.Consul;
using Convey.HTTP;
using Convey.LoadBalancing.Fabio;
using Convey.Logging;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.Outbox;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Metrics.AppMetrics;
using Convey.Persistence.MongoDB;
using Convey.Persistence.Redis;
using Convey.Tracing.Jaeger;
using Convey.Tracing.Jaeger.RabbitMQ;
using Convey.WebApi;
using Convey.WebApi.CQRS;
using Conveyor.Services.Orders.Commands;
using Conveyor.Services.Orders.Domain;
using Conveyor.Services.Orders.DTO;
using Conveyor.Services.Orders.Events.External;
using Conveyor.Services.Orders.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Conveyor.Services.Orders
{
    public class Program
    {
        public static Task Main(string[] args)
            => CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services => services
                        .AddOpenTracing()
                        .AddConvey()
                        .AddServices()
                        .AddHttpClient()
                        .AddConsul()
                        .AddFabio()
                        .AddJaeger()
                        .AddMongo()
                        .AddMongoRepository<Order, Guid>("orders")
                        .AddCommandHandlers()
                        .AddEventHandlers()
                        .AddQueryHandlers()
                        .AddInMemoryCommandDispatcher()
                        .AddInMemoryEventDispatcher()
                        .AddInMemoryQueryDispatcher()
                        .AddRedis()
                        .AddRabbitMq(plugins: p => p.AddJaegerRabbitMqPlugin())
                        .AddMessageOutbox()
                        .AddMetrics()
                        .AddWebApi()
                        .Build())
                    .Configure(app => app
                        .UseErrorHandler()
                        .UseInitializers()
                        .UseRouting()
                        .UseEndpoints(r => r.MapControllers())
                        .UseDispatcherEndpoints(endpoints => endpoints
                            .Get("", ctx => ctx.Response.WriteAsync("Orders Service"))
                            .Get("ping", ctx => ctx.Response.WriteAsync("pong"))
                            .Get<GetOrder, OrderDto>("orders/{orderId}")
                            .Post<CreateOrder>("orders",
                                afterDispatch: (cmd, ctx) => ctx.Response.Created($"orders/{cmd.OrderId}")))
                        .UseJaeger()
                        .UseInitializers()
                        .UseMetrics()
                        .UseRabbitMq()
                        .SubscribeEvent<DeliveryStarted>())
                    .UseLogging();
            });
    }
}
