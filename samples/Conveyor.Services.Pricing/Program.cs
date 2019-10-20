using Convey;
using Convey.Discovery.Consul;
using Convey.LoadBalancing.Fabio;
using Convey.Logging;
using Convey.Metrics.AppMetrics;
using Convey.Tracing.Jaeger;
using Convey.WebApi;
using Conveyor.Services.Pricing.DTO;
using Conveyor.Services.Pricing.Queries;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Conveyor.Services.Pricing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services => services
                        .AddOpenTracing()
                        .AddConvey()
                        .AddConsul()
                        .AddFabio()
                        .AddJaeger()
                        .AddMetrics()
                        .AddWebApi()
                        .Build())
                    .Configure(app => app
                        .UseConsul()
                        .UseJaeger()
                        .UseMetrics()
                        .UseEndpoints(endpoints => endpoints
                            .Get("", ctx => ctx.Response.WriteAsync("Pricing Service"))
                            .Get<GetOrderPricing>("orders/{orderId}/pricing", (query, ctx) =>
                                JsonSerializer.SerializeAsync(ctx.Response.Body, new PricingDto
                                {
                                    OrderId = query.OrderId, TotalAmount = 20.50m
                                }, StandardResolver.CamelCase))))
                    .UseLogging();
            });
    }
}
