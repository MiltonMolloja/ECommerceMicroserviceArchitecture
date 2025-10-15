using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Common.CorrelationId
{
    public static class CorrelationIdExtensions
    {
        /// <summary>
        /// Registra los servicios de Correlation ID
        /// </summary>
        public static IServiceCollection AddCorrelationId(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
            services.AddTransient<CorrelationIdDelegatingHandler>();

            return services;
        }

        /// <summary>
        /// Agrega el middleware de Correlation ID al pipeline
        /// </summary>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
            return app;
        }

        /// <summary>
        /// Configura un HttpClient para propagar Correlation IDs
        /// </summary>
        public static IHttpClientBuilder AddCorrelationIdPropagation(this IHttpClientBuilder builder)
        {
            builder.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
            return builder;
        }
    }
}
