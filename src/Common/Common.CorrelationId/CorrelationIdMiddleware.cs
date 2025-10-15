using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;

namespace Common.CorrelationId
{
    /// <summary>
    /// Middleware que maneja el Correlation ID para rastrear requests a través de microservicios
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId;

            // Verificar si ya existe un Correlation ID en los headers de entrada
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationIdFromHeader))
            {
                correlationId = correlationIdFromHeader.FirstOrDefault() ?? GenerateCorrelationId();
                _logger.LogDebug("Using existing Correlation ID: {CorrelationId}", correlationId);
            }
            else
            {
                // Generar nuevo Correlation ID si no existe
                correlationId = GenerateCorrelationId();
                _logger.LogDebug("Generated new Correlation ID: {CorrelationId}", correlationId);
            }

            // Agregar Correlation ID al contexto para que esté disponible en toda la aplicación
            context.Items[CorrelationIdHeaderName] = correlationId;

            // Agregar Correlation ID a los headers de respuesta
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
                {
                    context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
                }
                return Task.CompletedTask;
            });

            // Agregar Correlation ID al Activity (para distributed tracing)
            Activity.Current?.SetTag("correlation.id", correlationId);

            // Usar LogScope para incluir Correlation ID en todos los logs de este request
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method
            }))
            {
                await _next(context);
            }
        }

        private static string GenerateCorrelationId()
        {
            // Formato: timestamp-guid (más legible y único)
            return $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
        }
    }
}
