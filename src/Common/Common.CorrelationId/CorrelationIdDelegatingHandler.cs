using Microsoft.Extensions.Logging;

namespace Common.CorrelationId
{
    /// <summary>
    /// DelegatingHandler que propaga el Correlation ID a requests salientes de HttpClient
    /// </summary>
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly ICorrelationIdAccessor _correlationIdAccessor;
        private readonly ILogger<CorrelationIdDelegatingHandler> _logger;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdDelegatingHandler(
            ICorrelationIdAccessor correlationIdAccessor,
            ILogger<CorrelationIdDelegatingHandler> logger)
        {
            _correlationIdAccessor = correlationIdAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var correlationId = _correlationIdAccessor.GetCorrelationId();

            if (!string.IsNullOrEmpty(correlationId))
            {
                // Agregar Correlation ID al request saliente si no existe
                if (!request.Headers.Contains(CorrelationIdHeaderName))
                {
                    request.Headers.Add(CorrelationIdHeaderName, correlationId);
                    _logger.LogDebug(
                        "Added Correlation ID {CorrelationId} to outgoing request to {RequestUri}",
                        correlationId,
                        request.RequestUri);
                }
            }
            else
            {
                _logger.LogWarning(
                    "No Correlation ID available for outgoing request to {RequestUri}",
                    request.RequestUri);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
