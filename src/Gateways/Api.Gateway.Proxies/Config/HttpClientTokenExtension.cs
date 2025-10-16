using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net.Http;

namespace Api.Gateway.Proxies.Config
{
    public static class HttpClientTokenExtension
    {
        private const string ApiKeyHeaderName = "X-Api-Key";

        public static void AddBearerToken(this HttpClient client, IHttpContextAccessor context)
        {
            if (context?.HttpContext != null &&
                context.HttpContext.User.Identity.IsAuthenticated &&
                context.HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                var token = context.HttpContext.Request.Headers["Authorization"].ToString();

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
                }
            }
        }

        public static void AddApiKey(this HttpClient client, IConfiguration configuration)
        {
            var apiKey = configuration.GetValue<string>("ApiKey:ApiKey");

            if (!string.IsNullOrEmpty(apiKey))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(ApiKeyHeaderName, apiKey);
            }
        }
    }
}
