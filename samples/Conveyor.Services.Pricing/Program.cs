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
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Conveyor.Services.Pricing
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
                            {
                                var json = JsonConvert.SerializeObject(new PricingDto
                                {
                                    OrderId = query.OrderId, TotalAmount = 20.50m
                                });

                                return ctx.Response.WriteAsync(json);
                            })))
                    .UseLogging();
            });
    }
}
