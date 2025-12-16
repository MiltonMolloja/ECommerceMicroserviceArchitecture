using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Caching
{
    public static class RedisCacheExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            // Verificar si el cache está deshabilitado
            var cacheDisabled = configuration.GetValue<bool>("CacheSettings:Disabled", false);
            
            if (cacheDisabled)
            {
                // Usar NoCacheService cuando está deshabilitado
                // Pero siempre registrar IDistributedCache (en memoria) para servicios que lo necesiten
                services.AddDistributedMemoryCache();
                services.AddSingleton<ICacheService, NoCacheService>();
                return services;
            }

            var redisConnection = configuration.GetValue<string>("Redis:ConnectionString");

            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = configuration.GetValue<string>("Redis:InstanceName") ?? "ECommerce_";
                });

                services.AddSingleton<ICacheService, RedisCacheService>();
            }
            else
            {
                // Si no hay Redis configurado, usar implementación en memoria
                services.AddDistributedMemoryCache();
                services.AddSingleton<ICacheService, RedisCacheService>();
            }

            return services;
        }
    }
}
