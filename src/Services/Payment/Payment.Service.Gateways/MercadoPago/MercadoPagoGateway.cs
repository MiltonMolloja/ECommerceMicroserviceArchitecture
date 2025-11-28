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

                var paymentData = new MercadoPagoPaymentRequest
                {
                    TransactionAmount = request.Amount,
                    Token = request.PaymentToken,
                    Description = request.Description,
                    Installments = request.Installments,
                    PaymentMethodId = request.PaymentMethodId,
                    Payer = new MercadoPagoPayer
                    {
                        Email = request.PayerEmail
                    }
                };

                var response = await _httpClient.PostAsJsonAsync("/v1/payments", paymentData);
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

                var response = await _httpClient.PostAsJsonAsync($"/v1/payments/{request.TransactionId}/refunds", refundData);
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
