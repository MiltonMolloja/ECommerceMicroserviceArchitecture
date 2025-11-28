using Api.Gateway.Models;
using Api.Gateway.Models.Order.DTOs;
using Api.Gateway.Models.Orders.Commands;
using Api.Gateway.Proxies.Config;
using Api.Gateway.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Gateway.Proxies
{
    public interface IOrderProxy
    {
        Task<DataCollection<OrderDto>> GetAllAsync(int page, int take);
        Task<OrderDto> GetAsync(int id);
        Task<OrderCreateResponse> CreateAsync(OrderCreateCommand command);
    }

    public class OrderCreateResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public int OrderId { get; set; }
    }

    public class OrderProxy : IOrderProxy
    {
        private readonly ApiUrls _apiUrls;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public OrderProxy(
            HttpClient httpClient,
            IOptions<ApiUrls> apiUrls,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrls = apiUrls.Value;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        private HttpRequestMessage CreateRequestWithHeaders(HttpMethod method, string uri)
        {
            var request = new HttpRequestMessage(method, uri);

            // Add API Key
            var apiKey = _configuration.GetValue<string>("ApiKey:ApiKey");
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.TryAddWithoutValidation("X-Api-Key", apiKey);
            }

            // Add Bearer Token from current request
            if (_httpContextAccessor?.HttpContext != null &&
                _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated &&
                _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", token);
                }
            }

            return request;
        }

        public async Task<DataCollection<OrderDto>> GetAllAsync(int page, int take)
        {
            var request = CreateRequestWithHeaders(HttpMethod.Get, $"{_apiUrls.OrderUrl}v1/orders?page={page}&take={take}");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<DataCollection<OrderDto>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<OrderDto> GetAsync(int id)
        {
            var request = CreateRequestWithHeaders(HttpMethod.Get, $"{_apiUrls.OrderUrl}v1/orders/{id}");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<OrderDto>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<OrderCreateResponse> CreateAsync(OrderCreateCommand command)
        {
            var request = CreateRequestWithHeaders(HttpMethod.Post, $"{_apiUrls.OrderUrl}v1/orders");
            request.Content = new StringContent(
                JsonSerializer.Serialize(command),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<OrderCreateResponse>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }
    }
}
