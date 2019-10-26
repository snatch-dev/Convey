using System.Threading.Tasks;
using Convey.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
                webBuilder.ConfigureServices(services =>
                    {
                        services.AddMvcCore()
                            .AddNewtonsoftJson();
                    })
                    .Configure(app => app
                        .UseRouting()
                        .UseEndpoints(r => r.MapControllers()))
                    .UseLogging();
            });
    }
}