using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.ApiKey
{
    public static class ApiKeyExtensions
    {
        public static IServiceCollection AddApiKeyAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configurar settings desde appsettings.json
            services.Configure<ApiKeySettings>(configuration.GetSection("ApiKey"));

            return services;
        }

        public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
        {
            app.UseMiddleware<ApiKeyMiddleware>();
            return app;
        }
    }
}
