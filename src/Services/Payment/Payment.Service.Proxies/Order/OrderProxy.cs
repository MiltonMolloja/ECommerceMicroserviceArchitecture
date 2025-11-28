using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Payment.Service.Proxies.Order
{
    public class OrderProxy : IOrderProxy
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderProxy(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiUrls:OrderUrl"];
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OrderDto> GetOrderByIdAsync(int orderId)
        {
            try
            {
                // Obtener el token JWT del request actual y reenviarlo
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", authHeader.Substring(7));
                }

                var response = await _httpClient.GetFromJsonAsync<OrderDto>(
                    $"{_baseUrl}/v1/orders/{orderId}");
                return response;
            }
            catch
            {
                // Si falla, retornamos null y manejamos en el handler
                return null;
            }
        }

        public async Task UpdateOrderPaymentStatusAsync(int orderId, string status, string transactionId = null, string gateway = null)
        {
            try
            {
                // Obtener el token JWT del request actual y reenviarlo
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", authHeader.Substring(7));
                }

                // Mapear status string a OrderStatus enum
                int statusValue = status.ToLower() switch
                {
                    "paid" => 3,              // OrderStatus.Paid
                    "paymentfailed" => 2,     // OrderStatus.PaymentFailed
                    "paymentprocessing" => 1, // OrderStatus.PaymentProcessing
                    "cancelled" => 10,        // OrderStatus.Cancelled
                    _ => 1                    // Default: PaymentProcessing
                };

                var requestBody = new
                {
                    status = statusValue,
                    paymentTransactionId = transactionId,
                    paymentGateway = gateway
                };

                await _httpClient.PutAsJsonAsync(
                    $"{_baseUrl}/v1/orders/{orderId}/status",
                    requestBody);
            }
            catch
            {
                // Log error but don't throw
            }
        }
    }
}
