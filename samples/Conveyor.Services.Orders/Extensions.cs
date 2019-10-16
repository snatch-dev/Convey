using Convey;
using Conveyor.Services.Orders.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Conveyor.Services.Orders
{
    public static class Extensions
    {
        public static IConveyBuilder AddServices(this IConveyBuilder builder)
        {
            builder.Services.AddSingleton<IPricingServiceClient, PricingServiceClient>();
            return builder;
        }
    }
}
