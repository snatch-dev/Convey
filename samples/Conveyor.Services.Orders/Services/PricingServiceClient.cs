using System;
using System.Threading.Tasks;
using Convey.HTTP;
using Conveyor.Services.Orders.DTO;

namespace Conveyor.Services.Orders.Services
{
    public class PricingServiceClient : IPricingServiceClient
    {
        private readonly IHttpClient _client;
        private readonly string _url;

        public PricingServiceClient(IHttpClient client, HttpClientOptions options)
        {
            _client = client;
            _url = options.Services["pricing"];
        }

        public Task<PricingDto> GetAsync(Guid orderId)
            => _client.GetAsync<PricingDto>($"{_url}/orders/{orderId}/pricing");
    }
}
