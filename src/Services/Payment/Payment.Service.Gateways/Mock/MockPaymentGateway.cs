using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Payment.Service.Gateways.Mock
{
    public class MockPaymentGateway : IPaymentGateway
    {
        private readonly ILogger<MockPaymentGateway> _logger;
        private readonly bool _simulateDelay;
        private readonly int _delayMilliseconds;

        // Simulación de estados de MercadoPago basados en el nombre del titular
        // Documentación: https://www.mercadopago.com.ar/developers/es/docs/your-integrations/test/cards
        private static readonly Dictionary<string, (string Status, string StatusDetail, string Message)> TestCardholderNames = new()
        {
            // Aprobados
            { "APRO", ("approved", "accredited", "Pago aprobado") },
            
            // Rechazados - Llamar para autorizar
            { "CALL", ("rejected", "cc_rejected_call_for_authorize", "Rechazado - Llamar para autorizar") },
            
            // Rechazados - Fondos insuficientes
            { "FUND", ("rejected", "cc_rejected_insufficient_amount", "Rechazado por monto insuficiente") },
            
            // Rechazados - Código de seguridad
            { "SECU", ("rejected", "cc_rejected_bad_filled_security_code", "Rechazado por código de seguridad inválido") },
            
            // Rechazados - Fecha de expiración
            { "EXPI", ("rejected", "cc_rejected_bad_filled_date", "Rechazado por fecha de expiración inválida") },
            
            // Rechazados - Error en formulario
            { "FORM", ("rejected", "cc_rejected_bad_filled_other", "Rechazado por error en formulario") },
            
            // Rechazados - Tarjeta deshabilitada
            { "BLAC", ("rejected", "cc_rejected_blacklist", "Rechazado - Tarjeta deshabilitada") },
            
            // Rechazados - Tarjeta inválida
            { "CARD", ("rejected", "cc_rejected_invalid_installments", "Rechazado - Cuotas inválidas") },
            
            // Rechazados - Duplicado
            { "DUPL", ("rejected", "cc_rejected_duplicated_payment", "Rechazado - Pago duplicado") },
            
            // Rechazados - Monto máximo excedido
            { "HIGH", ("rejected", "cc_rejected_high_risk", "Rechazado - Alto riesgo") },
            
            // Rechazados - Otros
            { "OTHE", ("rejected", "cc_rejected_other_reason", "Rechazado por error general") },
            
            // Pendientes
            { "CONT", ("pending", "pending_contingency", "Pago pendiente de revisión") },
            { "PCONT", ("pending", "pending_contingency", "Pago pendiente de revisión") },
        };

        public MockPaymentGateway(ILogger<MockPaymentGateway> logger, IConfiguration configuration)
        {
            _logger = logger;
            _simulateDelay = configuration.GetValue<bool>("PaymentGateway:MockSettings:SimulateDelay", true);
            _delayMilliseconds = configuration.GetValue<int>("PaymentGateway:MockSettings:DelayMilliseconds", 1500);
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            _logger.LogInformation($"[MOCK GATEWAY] Processing payment for amount {request.Amount} {request.Currency}");
            _logger.LogInformation($"[MOCK GATEWAY] CardholderName: {request.CardholderName}, {request.IdentificationType}: {request.IdentificationNumber}");

            // Simular latencia de red
            if (_simulateDelay)
            {
                _logger.LogDebug($"[MOCK GATEWAY] Simulating network delay of {_delayMilliseconds}ms");
                await Task.Delay(_delayMilliseconds);
            }

            // Generar TransactionID simulado
            var transactionId = $"MOCK_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            // Simular información de tarjeta
            var cardLast4 = GenerateCardLast4(request.PaymentToken);
            var cardBrand = DetermineCardBrand(request.PaymentMethodId ?? request.PaymentToken);

            // ============================================
            // SIMULACIÓN DE ESTADOS DE MERCADOPAGO
            // Basado en el nombre del titular de la tarjeta
            // ============================================
            var cardholderName = request.CardholderName?.Trim().ToUpperInvariant();
            
            if (!string.IsNullOrEmpty(cardholderName))
            {
                if (TestCardholderNames.TryGetValue(cardholderName, out var testResult))
                {
                    _logger.LogInformation($"[MOCK GATEWAY] ✓ Test cardholder name detected: '{cardholderName}' -> Status: {testResult.Status}");

                    if (testResult.Status == "approved")
                    {
                        _logger.LogInformation($"[MOCK GATEWAY] ✓ Payment APPROVED - {testResult.Message}");
                        return new PaymentResult
                        {
                            Success = true,
                            TransactionId = transactionId,
                            Gateway = "Mock (MercadoPago Simulation)",
                            CardLast4 = cardLast4,
                            CardBrand = cardBrand
                        };
                    }
                    else if (testResult.Status == "rejected")
                    {
                        _logger.LogWarning($"[MOCK GATEWAY] ✗ Payment REJECTED - {testResult.Message} (Code: {testResult.StatusDetail})");
                        return new PaymentResult
                        {
                            Success = false,
                            ErrorMessage = testResult.Message,
                            ErrorCode = testResult.StatusDetail,
                            Gateway = "Mock (MercadoPago Simulation)",
                            CardLast4 = cardLast4,
                            CardBrand = cardBrand
                        };
                    }
                    else if (testResult.Status == "pending")
                    {
                        _logger.LogInformation($"[MOCK GATEWAY] ⏳ Payment PENDING - {testResult.Message}");
                        return new PaymentResult
                        {
                            Success = true,
                            TransactionId = transactionId,
                            Gateway = "Mock (MercadoPago Simulation)",
                            CardLast4 = cardLast4,
                            CardBrand = cardBrand
                        };
                    }
                }
                else
                {
                    _logger.LogWarning($"[MOCK GATEWAY] ⚠ Cardholder name '{cardholderName}' is NOT a test name. Valid test names: APRO, CALL, FUND, SECU, EXPI, FORM, BLAC, CARD, DUPL, HIGH, OTHE, CONT, PCONT");
                    _logger.LogInformation($"[MOCK GATEWAY] Proceeding with default approval logic (non-test mode)");
                }
            }
            else
            {
                _logger.LogWarning($"[MOCK GATEWAY] ⚠ No cardholder name provided. Proceeding with default approval logic");
            }

            // ============================================
            // LÓGICA LEGACY (mantener compatibilidad)
            // ============================================
            
            if (request.PaymentToken == "MOCK_FAIL_TOKEN")
            {
                _logger.LogWarning($"[MOCK GATEWAY] Payment failed - Test token for failure scenario");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = "Test failure: Invalid payment token (MOCK_FAIL_TOKEN)",
                    ErrorCode = "invalid_token",
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
                    ErrorCode = "amount_exceeded",
                    Gateway = "Mock"
                };
            }

            // Por defecto: pago exitoso
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
