using Api.Gateway.Models.Cart.DTOs;
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
    public interface ICartProxy
    {
        Task<CartDto> GetByClientIdAsync(int clientId);
        Task<CartDto> GetBySessionIdAsync(string sessionId);
        Task<object> AddItemAsync(object command);
        Task<object> UpdateQuantityAsync(int cartId, object command);
        Task<object> RemoveItemAsync(int cartId, int productId);
        Task<object> ClearAsync(int cartId);
        Task<object> ApplyCouponAsync(int cartId, object command);
        Task<object> RemoveCouponAsync(int cartId);
    }

    public class CartProxy : ICartProxy
    {
        private readonly ApiUrls _apiUrls;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartProxy(
            HttpClient httpClient,
            IOptions<ApiUrls> apiUrls,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            httpClient.AddBearerToken(httpContextAccessor);
            httpClient.AddApiKey(configuration);

            _httpClient = httpClient;
            _apiUrls = apiUrls.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CartDto> GetByClientIdAsync(int clientId)
        {
            var request = await _httpClient.GetAsync($"{_apiUrls.CartUrl}v1/cart/client/{clientId}");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<CartDto>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<CartDto> GetBySessionIdAsync(string sessionId)
        {
            var request = await _httpClient.GetAsync($"{_apiUrls.CartUrl}v1/cart/session/{sessionId}");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<CartDto>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<object> AddItemAsync(object command)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(command),
                Encoding.UTF8,
                "application/json"
            );

            var request = await _httpClient.PostAsync($"{_apiUrls.CartUrl}v1/cart/add-item", content);
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<object>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<object> UpdateQuantityAsync(int cartId, object command)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(command),
                Encoding.UTF8,
                "application/json"
            );

            var request = await _httpClient.PutAsync($"{_apiUrls.CartUrl}v1/cart/{cartId}/update-quantity", content);
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<object>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<object> RemoveItemAsync(int cartId, int productId)
        {
            var request = await _httpClient.DeleteAsync($"{_apiUrls.CartUrl}v1/cart/{cartId}/remove-item/{productId}");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<object>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<object> ClearAsync(int cartId)
        {
            var request = await _httpClient.DeleteAsync($"{_apiUrls.CartUrl}v1/cart/{cartId}/clear");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<object>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<object> ApplyCouponAsync(int cartId, object command)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(command),
                Encoding.UTF8,
                "application/json"
            );

            var request = await _httpClient.PostAsync($"{_apiUrls.CartUrl}v1/cart/{cartId}/apply-coupon", content);
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<object>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<object> RemoveCouponAsync(int cartId)
        {
            var request = await _httpClient.DeleteAsync($"{_apiUrls.CartUrl}v1/cart/{cartId}/remove-coupon");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<object>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }
    }
}
