using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Convey.Persistence.OpenStack.OCS.Http;

namespace Convey.Persistence.OpenStack.OCS.Auth
{
    internal class AuthManager : IAuthManager
    {
        private readonly OcsOptions _ocsOptions;
        private readonly HttpClient _httpClient;

        public AuthManager(OcsOptions ocsOptions, IHttpClientFactory httpClientFactory)
        {
            _ocsOptions = ocsOptions;
            _httpClient = httpClientFactory.CreateClient(ocsOptions.InternalHttpClientName);
        }

        public async Task<AuthData> Authenticate()
        {
            var authRequest = new AuthRequestBuilder()
                .WithMethod(_ocsOptions.AuthMethod)
                .WithProject(_ocsOptions.ProjectId)
                .WithUser(_ocsOptions.UserId, _ocsOptions.Password)
                .Build();

            var httpRequest = new HttpRequestBuilder()
                .WithRelativeUrl(_ocsOptions.AuthRelativeUrl)
                .WithMethod(HttpMethod.Post)
                .WithJsonContent(authRequest)
                .Build();

            var response = await _httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Something failed");
            }

            var authToken = response.Headers.FirstOrDefault(p => p.Key.Equals("X-Subject-Token")).Value.FirstOrDefault()?.ToString();
            return new AuthData(authToken);
        }
    }
}
