using Api.Gateway.Models.Payment.Commands;
using Api.Gateway.Models.Payment.DTOs;
using Api.Gateway.Proxies.Config;
using Api.Gateway.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Gateway.Proxies
{
    public interface IPaymentProxy
    {
        Task<PaymentProcessingResult> ProcessPaymentAsync(ProcessPaymentCommand command);
        Task<PaymentDto> GetPaymentByIdAsync(int id);
        Task<PaymentDto> GetPaymentByOrderIdAsync(int orderId);
        Task<List<PaymentDto>> GetPaymentHistoryAsync(int page, int pageSize);
        Task ProcessRefundAsync(ProcessRefundCommand command);
        Task<List<PaymentTransactionDto>> GetPaymentTransactionsAsync(int paymentId);
    }

    public class PaymentProxy : IPaymentProxy
    {
        private readonly ApiUrls _apiUrls;
        private readonly HttpClient _httpClient;

        public PaymentProxy(
            HttpClient httpClient,
            IOptions<ApiUrls> apiUrls,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            httpClient.AddBearerToken(httpContextAccessor);
            httpClient.AddApiKey(configuration);

            _httpClient = httpClient;
            _apiUrls = apiUrls.Value;
        }

        public async Task<PaymentProcessingResult> ProcessPaymentAsync(ProcessPaymentCommand command)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(command),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_apiUrls.PaymentUrl}api/Payment/process", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Manejar tanto respuestas 200 OK como 400 Bad Request
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return JsonSerializer.Deserialize<PaymentProcessingResult>(
                    responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );
            }
            else
            {
                // Para otros errores (500, etc.), lanzar excepción
                response.EnsureSuccessStatusCode();
                return null; // Nunca se alcanzará
            }
        }

        public async Task<PaymentDto> GetPaymentByIdAsync(int id)
        {
            var request = await _httpClient.GetAsync($"{_apiUrls.PaymentUrl}api/Payment/{id}");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PaymentDto>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<PaymentDto> GetPaymentByOrderIdAsync(int orderId)
        {
            var request = await _httpClient.GetAsync($"{_apiUrls.PaymentUrl}api/Payment/order/{orderId}");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PaymentDto>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<List<PaymentDto>> GetPaymentHistoryAsync(int page, int pageSize)
        {
            var request = await _httpClient.GetAsync($"{_apiUrls.PaymentUrl}api/Payment/history?page={page}&pageSize={pageSize}");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<List<PaymentDto>>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task ProcessRefundAsync(ProcessRefundCommand command)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(command),
                Encoding.UTF8,
                "application/json"
            );

            var request = await _httpClient.PostAsync($"{_apiUrls.PaymentUrl}api/Payment/{command.PaymentId}/refund", content);
            request.EnsureSuccessStatusCode();
        }

        public async Task<List<PaymentTransactionDto>> GetPaymentTransactionsAsync(int paymentId)
        {
            var request = await _httpClient.GetAsync($"{_apiUrls.PaymentUrl}api/Payment/{paymentId}/transactions");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<List<PaymentTransactionDto>>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }
    }
}
