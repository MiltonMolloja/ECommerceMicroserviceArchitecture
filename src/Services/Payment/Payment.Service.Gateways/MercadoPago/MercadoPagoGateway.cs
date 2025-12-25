using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Payment.Service.Gateways.MercadoPago
{
    public class MercadoPagoGateway : IPaymentGateway
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MercadoPagoGateway> _logger;
        private readonly HttpClient _httpClient;

        public MercadoPagoGateway(
            IConfiguration configuration,
            ILogger<MercadoPagoGateway> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;

            // Configurar HttpClient
            var accessToken = _configuration["MercadoPago:AccessToken"];
            _httpClient.BaseAddress = new Uri("https://api.mercadopago.com");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing payment with MercadoPago for amount {Amount}", request.Amount);

                // ============================================================================
                // DEMO ONLY: Monto hardcodeado a 20 ARS para propósitos demostrativos.
                // En producción, usar request.Amount para cobrar el monto real del carrito.
                // TODO: Remover este hardcode antes de ir a producción.
                // ============================================================================
                const decimal DEMO_AMOUNT = 20.00m;
                _logger.LogWarning("DEMO MODE: Using hardcoded amount {DemoAmount} ARS instead of {RealAmount}", DEMO_AMOUNT, request.Amount);

                var paymentData = new MercadoPagoPaymentRequest
                {
                    TransactionAmount = DEMO_AMOUNT, // DEMO: Usar monto fijo de 20 ARS
                    Token = request.PaymentToken,
                    Description = request.Description,
                    Installments = request.Installments,
                    PaymentMethodId = request.PaymentMethodId,
                    Payer = new MercadoPagoPayer
                    {
                        Email = request.PayerEmail
                    }
                };

                // MercadoPago requiere X-Idempotency-Key para evitar pagos duplicados
                var idempotencyKey = Guid.NewGuid().ToString();
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/payments");
                httpRequest.Headers.Add("X-Idempotency-Key", idempotencyKey);
                httpRequest.Content = JsonContent.Create(paymentData);
                
                var response = await _httpClient.SendAsync(httpRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("MercadoPago response status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("MercadoPago response: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var paymentResponse = JsonSerializer.Deserialize<MercadoPagoPaymentResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (paymentResponse?.Status == "approved")
                    {
                        return new PaymentResult
                        {
                            Success = true,
                            TransactionId = paymentResponse.Id.ToString(),
                            Gateway = "MercadoPago",
                            CardLast4 = paymentResponse.Card?.LastFourDigits,
                            CardBrand = paymentResponse.PaymentMethodId
                        };
                    }
                    else
                    {
                        return new PaymentResult
                        {
                            Success = false,
                            ErrorMessage = $"Payment failed with status: {paymentResponse?.Status}. Message: {paymentResponse?.StatusDetail}",
                            ErrorCode = paymentResponse?.StatusDetail,
                            CardLast4 = paymentResponse?.Card?.LastFourDigits,
                            CardBrand = paymentResponse?.PaymentMethodId
                        };
                    }
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<MercadoPagoErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new PaymentResult
                    {
                        Success = false,
                        ErrorMessage = $"MercadoPago API error: {errorResponse?.Message ?? responseContent}",
                        ErrorCode = errorResponse?.Error ?? "API_ERROR"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment with MercadoPago");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = $"Payment processing error: {ex.Message}",
                    ErrorCode = "PROCESSING_ERROR"
                };
            }
        }

        public async Task<RefundResult> ProcessRefundAsync(RefundRequest request)
        {
            try
            {
                _logger.LogInformation("Processing refund with MercadoPago for transaction {TransactionId}", request.TransactionId);

                var refundData = new
                {
                    amount = request.Amount
                };

                // MercadoPago requiere X-Idempotency-Key para evitar reembolsos duplicados
                var idempotencyKey = Guid.NewGuid().ToString();
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/v1/payments/{request.TransactionId}/refunds");
                httpRequest.Headers.Add("X-Idempotency-Key", idempotencyKey);
                httpRequest.Content = JsonContent.Create(refundData);
                
                var response = await _httpClient.SendAsync(httpRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("MercadoPago refund response status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var refundResponse = JsonSerializer.Deserialize<MercadoPagoRefundResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new RefundResult
                    {
                        Success = true,
                        RefundId = refundResponse?.Id.ToString(),
                        ErrorMessage = null
                    };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<MercadoPagoErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new RefundResult
                    {
                        Success = false,
                        ErrorMessage = $"Refund failed: {errorResponse?.Message ?? responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund with MercadoPago");
                return new RefundResult
                {
                    Success = false,
                    ErrorMessage = $"Refund processing error: {ex.Message}"
                };
            }
        }
    }

    // DTOs for MercadoPago API
    public class MercadoPagoPaymentRequest
    {
        [JsonPropertyName("transaction_amount")]
        public decimal TransactionAmount { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("installments")]
        public int Installments { get; set; }

        [JsonPropertyName("payment_method_id")]
        public string PaymentMethodId { get; set; }

        [JsonPropertyName("payer")]
        public MercadoPagoPayer Payer { get; set; }
    }

    public class MercadoPagoPayer
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }

    public class MercadoPagoPaymentResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("status_detail")]
        public string StatusDetail { get; set; }

        [JsonPropertyName("payment_method_id")]
        public string PaymentMethodId { get; set; }

        [JsonPropertyName("card")]
        public MercadoPagoCard Card { get; set; }
    }

    public class MercadoPagoCard
    {
        [JsonPropertyName("last_four_digits")]
        public string LastFourDigits { get; set; }
    }

    public class MercadoPagoRefundResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class MercadoPagoErrorResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }
}
