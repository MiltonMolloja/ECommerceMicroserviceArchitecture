using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace Common.ApiKey
{
    public static class HttpClientApiKeyExtension
    {
        private const string ApiKeyHeaderName = "X-Api-Key";

        public static void AddApiKey(this HttpClient client, IConfiguration configuration)
        {
            var apiKey = configuration.GetValue<string>("ApiKey:ApiKey");

            if (!string.IsNullOrEmpty(apiKey))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(ApiKeyHeaderName, apiKey);
            }
        }

        public static void AddApiKey(this HttpClient client, string apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(ApiKeyHeaderName, apiKey);
            }
        }
    }
}
