using System;
using System.Threading.Tasks;
using Convey;
using Convey.Configurations.Vault;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.Discovery.Consul;
using Convey.HTTP;
using Convey.LoadBalancing.Fabio;
using Convey.Logging;
using Convey.MessageBrokers;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Metrics.AppMetrics;
using Convey.Persistence.MongoDB;
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
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Conveyor.Services.Orders
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services => services
                        .Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; })
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
                        .AddRabbitMq(plugins: p => p.AddJaegerRabbitMqPlugin())
                        .AddMetrics()
                        .AddWebApi()
                        .Build())
                    .Configure(app => app
                        .UseDispatcherEndpoints(endpoints => endpoints
                            .Get("", ctx => ctx.Response.WriteAsync("Orders Service"))
                            .Get<GetOrder, OrderDto>("orders/{orderId}")
                            .Post<CreateOrder>("orders",
                                afterDispatch: (cmd, ctx) => ctx.Response.Created($"orders/{cmd.OrderId}")))
                        .UseConsul()
                        .UseJaeger()
                        .UseInitializers()
                        .UseMetrics()
                        .UseErrorHandler()
                        .UseRouting()
                        .UseEndpoints(b => b.MapControllers())
                        .UseRabbitMq()
                        .SubscribeEvent<DeliveryStarted>())
                    .UseVault()
                    .UseLogging();
            });
    }
}
