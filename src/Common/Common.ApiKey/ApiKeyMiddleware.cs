using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Common.ApiKey
{
    public class ApiKeyMiddleware
    {
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyMiddleware> _logger;
        private readonly ApiKeySettings _settings;

        public ApiKeyMiddleware(
            RequestDelegate next,
            ILogger<ApiKeyMiddleware> logger,
            IOptions<ApiKeySettings> settings)
        {
            _next = next;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Si la validación está deshabilitada, continuar
            if (!_settings.EnableApiKeyValidation)
            {
                await _next(context);
                return;
            }

            // Endpoints que no requieren API Key (health checks, swagger, autenticación, etc.)
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
            if (path.Contains("/hc") ||
                path.Contains("/swagger") ||
                path.Contains("/healthchecks-ui") ||
                path.Contains("/health") ||
                path.EndsWith(".json") && path.Contains("swagger") ||
                // Endpoints de autenticación públicos (no requieren API Key)
                path.Contains("/identity/authentication") ||
                path.Contains("/identity/refresh-token") ||
                (path.Contains("/v1/identity") && context.Request.Method == "POST" && !path.Contains("/revoke-token")))
            {
                await _next(context);
                return;
            }

            // Verificar si el header X-Api-Key existe
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                _logger.LogWarning("Request from {IpAddress} to {Path} rejected: Missing API Key",
                    context.Connection.RemoteIpAddress,
                    context.Request.Path);

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized",
                    message = "API Key is required for service-to-service communication"
                });
                return;
            }

            // Validar el API Key
            var apiKeyValue = extractedApiKey.ToString();
            var isValid = _settings.ValidApiKeys.Contains(apiKeyValue);

            if (!isValid)
            {
                _logger.LogWarning("Request from {IpAddress} to {Path} rejected: Invalid API Key",
                    context.Connection.RemoteIpAddress,
                    context.Request.Path);

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized",
                    message = "Invalid API Key"
                });
                return;
            }

            // API Key válido, continuar con el pipeline
            _logger.LogDebug("Valid API Key from {IpAddress} to {Path}",
                context.Connection.RemoteIpAddress,
                context.Request.Path);

            await _next(context);
        }
    }
}
