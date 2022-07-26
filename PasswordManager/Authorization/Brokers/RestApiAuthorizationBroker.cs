using Microsoft.Extensions.Options;
using PasswordManager.Authorization.Helpers;
using PasswordManager.Authorization.Holders;
using System.Net.Http;
using System.Web;

namespace PasswordManager.Authorization.Brokers
{
    public class RestApiConfig
    {
        public string baseUrl { get; init; }
        public string token { get; init; }
    }
    public class RestApiAuthorizationBroker
    {
        private readonly RestApiConfig _config;

        public RestApiAuthorizationBroker(
            IOptions<RestApiConfig> options,
            string tokenHolder,
            IHttpClientFactory httpClientFactory)
        {
            _config = options.Value;
        }

    }
}
