using Microsoft.AspNetCore.Http;

namespace Common.CorrelationId
{
    /// <summary>
    /// Implementación de ICorrelationIdAccessor que obtiene el Correlation ID del HttpContext
    /// </summary>
    public class CorrelationIdAccessor : ICorrelationIdAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCorrelationId()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return null;
            }

            // Intentar obtener del contexto
            if (httpContext.Items.TryGetValue(CorrelationIdHeaderName, out var correlationId))
            {
                return correlationId?.ToString();
            }

            // Intentar obtener de los headers
            if (httpContext.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue))
            {
                return headerValue.FirstOrDefault();
            }

            return null;
        }
    }
}
