using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace Common.RateLimiting
{
    public static class RateLimitingExtensions
    {
        // Flag estático para saber si el rate limiting está habilitado
        private static bool _rateLimitingEnabled = false;

        public static IServiceCollection AddCustomRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var settings = new RateLimitingSettings();
            configuration.GetSection("RateLimiting").Bind(settings);

            if (!settings.EnableRateLimiting)
            {
                _rateLimitingEnabled = false;
                return services;
            }

            _rateLimitingEnabled = true;

            services.AddRateLimiter(options =>
            {
                // Política para endpoints de autenticación (más restrictiva)
                options.AddFixedWindowLimiter("authentication", opt =>
                {
                    opt.PermitLimit = settings.Authentication.PermitLimit;
                    opt.Window = TimeSpan.FromSeconds(settings.Authentication.WindowInSeconds);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 2;
                });

                // Política para endpoints generales
                options.AddFixedWindowLimiter("general", opt =>
                {
                    opt.PermitLimit = settings.General.PermitLimit;
                    opt.Window = TimeSpan.FromSeconds(settings.General.WindowInSeconds);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 5;
                });

                // Política para endpoints de lectura (GET)
                options.AddFixedWindowLimiter("read", opt =>
                {
                    opt.PermitLimit = settings.Read.PermitLimit;
                    opt.Window = TimeSpan.FromSeconds(settings.Read.WindowInSeconds);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 10;
                });

                // Política para endpoints de escritura (POST, PUT, DELETE)
                options.AddFixedWindowLimiter("write", opt =>
                {
                    opt.PermitLimit = settings.Write.PermitLimit;
                    opt.Window = TimeSpan.FromSeconds(settings.Write.WindowInSeconds);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 5;
                });

                // Política por IP con Sliding Window (más sofisticada)
                options.AddSlidingWindowLimiter("sliding", opt =>
                {
                    opt.PermitLimit = 100;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.SegmentsPerWindow = 6; // Divide la ventana en 6 segmentos de 10 segundos
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 5;
                });

                // Política concurrente para operaciones costosas
                options.AddConcurrencyLimiter("concurrent", opt =>
                {
                    opt.PermitLimit = 10;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 5;
                });

                // Configuración global: rechazar solicitudes que excedan el límite
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    // Particionar por IP del cliente
                    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 500,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });

                // Personalizar respuesta cuando se excede el límite
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsJsonAsync(new
                        {
                            error = "Too many requests",
                            message = "Rate limit exceeded. Please try again later.",
                            retryAfter = retryAfter.TotalSeconds
                        }, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsJsonAsync(new
                        {
                            error = "Too many requests",
                            message = "Rate limit exceeded. Please try again later."
                        }, cancellationToken: cancellationToken);
                    }
                };
            });

            return services;
        }

        public static IApplicationBuilder UseCustomRateLimiting(this IApplicationBuilder app)
        {
            // Solo usar rate limiter si está habilitado
            if (_rateLimitingEnabled)
            {
                app.UseRateLimiter();
            }
            return app;
        }
    }
}
