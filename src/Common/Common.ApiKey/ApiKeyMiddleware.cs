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
            if (IsExcludedEndpoint(context))
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

        /// <summary>
        /// Determina si el endpoint está excluido de la validación de API Key
        /// </summary>
        private static bool IsExcludedEndpoint(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            // Health checks y monitoreo
            if (IsHealthCheckEndpoint(path))
                return true;

            // Swagger/OpenAPI
            if (IsSwaggerEndpoint(path))
                return true;

            // Endpoints de autenticación públicos
            if (IsPublicAuthEndpoint(path, context.Request.Method))
                return true;

            return false;
        }

        private static bool IsHealthCheckEndpoint(string path)
        {
            return path.Contains("/hc") ||
                   path.Contains("/health") ||
                   path.Contains("/healthchecks-ui");
        }

        private static bool IsSwaggerEndpoint(string path)
        {
            return path.Contains("/swagger") ||
                   (path.EndsWith(".json") && path.Contains("swagger"));
        }

        private static bool IsPublicAuthEndpoint(string path, string method)
        {
            if (path.Contains("/identity/authentication"))
                return true;

            if (path.Contains("/identity/refresh-token"))
                return true;

            // POST a /v1/identity (registro, login) excepto revoke-token
            if (path.Contains("/v1/identity") && method == "POST" && !path.Contains("/revoke-token"))
                return true;

            return false;
        }
    }
}
