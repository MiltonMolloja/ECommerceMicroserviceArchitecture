using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Payment.Service.Proxies.Notification
{
    public class NotificationProxy : INotificationProxy
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationProxy(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiUrls:NotificationUrl"];
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthorizationHeader()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", authHeader.Substring(7));
            }
        }

        public async Task SendPaymentConfirmationAsync(int userId, int paymentId)
        {
            try
            {
                SetAuthorizationHeader();
                await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/api/notification/payment-confirmation",
                    new { userId, paymentId });
            }
            catch
            {
                // Log error but don't throw
            }
        }

        public async Task SendPaymentFailedAsync(PaymentFailedNotification notification)
        {
            try
            {
                SetAuthorizationHeader();
                await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/api/notification/payment-failed",
                    notification);
            }
            catch
            {
                // Log error but don't throw
            }
        }

        public async Task SendRefundProcessedAsync(int userId, int paymentId)
        {
            try
            {
                SetAuthorizationHeader();
                await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/api/notification/refund-processed",
                    new { userId, paymentId });
            }
            catch
            {
                // Log error but don't throw
            }
        }

        public async Task SendOrderPlacedNotificationAsync(OrderPlacedNotification notification)
        {
            try
            {
                SetAuthorizationHeader();
                await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/api/notification/order-placed",
                    notification);
            }
            catch
            {
                // Log error but don't throw
            }
        }
    }
}
