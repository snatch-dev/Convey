using System;
using System.Threading.Tasks;
using Convey;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.Discovery.Consul;
using Convey.Docs.Swagger;
using Convey.HTTP;
using Convey.LoadBalancing.Fabio;
using Convey.Logging;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.Outbox;
using Convey.MessageBrokers.Outbox.Mongo;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Metrics.Prometheus;
using Convey.Persistence.MongoDB;
using Convey.Persistence.Redis;
using Convey.Secrets.Vault;
using Convey.Tracing.Jaeger;
using Convey.Tracing.Jaeger.RabbitMQ;
using Convey.WebApi;
using Convey.WebApi.CQRS;
using Convey.WebApi.Security;
using Convey.WebApi.Swagger;
using Conveyor.Services.Orders.Commands;
using Conveyor.Services.Orders.Domain;
using Conveyor.Services.Orders.DTO;
using Conveyor.Services.Orders.Events.External;
using Conveyor.Services.Orders.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Conveyor.Services.Orders;

public class Program
{
    public static Task Main(string[] args)
        => CreateHostBuilder(args).Build().RunAsync();

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.ConfigureServices(services => services
                    .AddConvey()
                    .AddErrorHandler<ExceptionToResponseMapper>()
                    .AddServices()
                    .AddHttpClient()
                    .AddCorrelationContextLogging()
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
                    .AddPrometheus()
                    .AddRedis()
                    .AddRabbitMq(plugins: p => p.AddJaegerRabbitMqPlugin())
                    .AddMessageOutbox(o => o.AddMongo())
                    .AddWebApi()
                    .AddSwaggerDocs()
                    .AddWebApiSwaggerDocs()
                    .Build())
                .Configure(app => app
                    .UseConvey()
                    .UserCorrelationContextLogging()
                    .UseErrorHandler()
                    .UsePrometheus()
                    .UseRouting()
                    .UseCertificateAuthentication()
                    .UseEndpoints(r => r.MapControllers())
                    .UseDispatcherEndpoints(endpoints => endpoints
                        .Get("", ctx => ctx.Response.WriteAsync("Orders Service"))
                        .Get("ping", ctx => ctx.Response.WriteAsync("pong"))
                        .Get<GetOrder, OrderDto>("orders/{orderId}")
                        .Post<CreateOrder>("orders",
                            afterDispatch: (cmd, ctx) => ctx.Response.Created($"orders/{cmd.OrderId}")))
                    .UseJaeger()
                    .UseSwaggerDocs()
                    .UseRabbitMq()
                    .SubscribeEvent<DeliveryStarted>())
                .UseLogging()
                .UseVault();
        });
}