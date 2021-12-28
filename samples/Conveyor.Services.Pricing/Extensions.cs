using Convey;
using Convey.WebApi.Security;

namespace Conveyor.Services.Pricing;

public static class Extensions
{
    public static IConveyBuilder AddServices(this IConveyBuilder builder)
    {
        builder.AddCertificateAuthentication();
        return builder;
    }
}