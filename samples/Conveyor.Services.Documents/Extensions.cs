using Convey;
using Conveyor.Services.Documents.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Conveyor.Services.Documents
{
    public static class Extensions
    {
        public static IConveyBuilder AddServices(this IConveyBuilder builder)
        {
            builder.Services.AddSingleton<IInvoiceService, InvoiceService>();
            return builder;
        }
    }
}