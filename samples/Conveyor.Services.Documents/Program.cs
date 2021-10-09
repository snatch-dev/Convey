using Convey;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.Discovery.Consul;
using Convey.LoadBalancing.Fabio;
using Convey.Logging;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Metrics.Prometheus;
using Convey.Persistence.Fs.Seaweed;
using Convey.Persistence.Redis;
using Convey.Tracing.Jaeger;
using Convey.Tracing.Jaeger.RabbitMQ;
using Convey.WebApi;
using Convey.WebApi.CQRS;
using Conveyor.Services.Documents.Events.External;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Conveyor.Services.Documents
{
    public class Program
    {
        public static Task Main(string[] args)
            => CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services => services
                        .AddConvey()
                        .AddServices()
                        .AddErrorHandler<ExceptionToResponseMapper>()
                        .AddConsul()
                        .AddFabio()
                        .AddJaeger()
                        .AddSeaweed()
                        .AddEventHandlers()
                        .AddRedis()
                        .AddRabbitMq(plugins: p => p.AddJaegerRabbitMqPlugin())
                        .AddPrometheus()
                        .AddWebApi()
                        .Build())
                    .Configure(app => app
                        .UseConvey()
                        .UsePrometheus()
                        .UseErrorHandler()
                        .UseEndpoints(endpoints => endpoints
                            .Get("", ctx => ctx.Response.WriteAsync("Documents Service"))
                            .Get("ping", ctx => ctx.Response.WriteAsync("pong")))
                        .UseJaeger()
                        .UseRabbitMq()
                        .SubscribeEvent<OrderCreated>())
                    .UseLogging();
            });
    }
}
