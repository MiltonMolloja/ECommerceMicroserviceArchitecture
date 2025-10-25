using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Payment.Service.Gateways.Mock
{
    public class MockPaymentGateway : IPaymentGateway
    {
        private readonly ILogger<MockPaymentGateway> _logger;
        private readonly bool _simulateDelay;
        private readonly int _delayMilliseconds;

        public MockPaymentGateway(ILogger<MockPaymentGateway> logger, IConfiguration configuration)
        {
            _logger = logger;
            _simulateDelay = configuration.GetValue<bool>("PaymentGateway:MockSettings:SimulateDelay", true);
            _delayMilliseconds = configuration.GetValue<int>("PaymentGateway:MockSettings:DelayMilliseconds", 1500);
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            _logger.LogInformation($"[MOCK GATEWAY] Processing payment for amount {request.Amount} {request.Currency}");

            // Simular latencia de red
            if (_simulateDelay)
            {
                _logger.LogDebug($"[MOCK GATEWAY] Simulating network delay of {_delayMilliseconds}ms");
                await Task.Delay(_delayMilliseconds);
            }

            // Generar TransactionID simulado
            var transactionId = $"MOCK_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            // Lógica de simulación:
            // - Montos < 9999: Éxito
            // - Montos >= 9999: Fallo (para probar flujos de error)
            // - Tokens específicos para casos especiales

            if (request.PaymentToken == "MOCK_FAIL_TOKEN")
            {
                _logger.LogWarning($"[MOCK GATEWAY] Payment failed - Test token for failure scenario");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = "Test failure: Invalid payment token (MOCK_FAIL_TOKEN)",
                    Gateway = "Mock"
                };
            }

            if (request.Amount >= 9999)
            {
                _logger.LogWarning($"[MOCK GATEWAY] Payment failed - Amount exceeds test limit");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = "Payment amount exceeds test limit (9999). Use lower amount for successful test.",
                    Gateway = "Mock"
                };
            }

            // Simular información de tarjeta
            var cardLast4 = GenerateCardLast4(request.PaymentToken);
            var cardBrand = DetermineCardBrand(request.PaymentToken);

            _logger.LogInformation($"[MOCK GATEWAY] Payment succeeded - TransactionID: {transactionId}");

            return new PaymentResult
            {
                Success = true,
                TransactionId = transactionId,
                Gateway = "Mock",
                CardLast4 = cardLast4,
                CardBrand = cardBrand
            };
        }

        public async Task<RefundResult> ProcessRefundAsync(RefundRequest request)
        {
            _logger.LogInformation($"[MOCK GATEWAY] Processing refund for transaction {request.TransactionId}, amount {request.Amount}");

            // Simular latencia de red
            if (_simulateDelay)
            {
                await Task.Delay(_delayMilliseconds / 2); // Reembolsos más rápidos
            }

            // Validar que sea un transaction ID de mock
            if (!request.TransactionId.StartsWith("MOCK_"))
            {
                _logger.LogWarning($"[MOCK GATEWAY] Refund failed - Invalid transaction ID format");
                return new RefundResult
                {
                    Success = false,
                    ErrorMessage = "Invalid transaction ID - not from Mock gateway"
                };
            }

            var refundId = $"REFUND_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            _logger.LogInformation($"[MOCK GATEWAY] Refund succeeded - RefundID: {refundId}");

            return new RefundResult
            {
                Success = true,
                RefundId = refundId,
                ErrorMessage = null
            };
        }

        private string GenerateCardLast4(string paymentToken)
        {
            // Generar últimos 4 dígitos basados en el hash del token
            var hash = paymentToken.GetHashCode();
            var last4 = Math.Abs(hash % 10000).ToString("D4");
            return last4;
        }

        private string DetermineCardBrand(string paymentToken)
        {
            // Determinar marca de tarjeta basado en el token
            if (paymentToken.Contains("visa", StringComparison.OrdinalIgnoreCase))
                return "Visa";
            if (paymentToken.Contains("master", StringComparison.OrdinalIgnoreCase))
                return "Mastercard";
            if (paymentToken.Contains("amex", StringComparison.OrdinalIgnoreCase))
                return "American Express";

            // Por defecto, usar el hash para variar entre marcas comunes
            var hash = Math.Abs(paymentToken.GetHashCode());
            var brands = new[] { "Visa", "Mastercard", "American Express", "Discover" };
            return brands[hash % brands.Length];
        }
    }
}
