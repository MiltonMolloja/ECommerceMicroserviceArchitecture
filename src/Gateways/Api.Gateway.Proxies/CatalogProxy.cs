using Api.Gateway.Models;
using Api.Gateway.Models.Catalog.DTOs;
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
    public interface ICatalogProxy
    {
        Task<DataCollection<ProductDto>> GetAllAsync(int page, int take, IEnumerable<int> clients = null);
        Task<ProductDto> GetAsync(int id);
        Task<ProductSearchResponse> SearchAsync(ProductSearchRequest request);
        Task<ProductAdvancedSearchResponse> SearchAdvancedAsync(ProductAdvancedSearchRequest request);
        Task<object> GetProductReviewsAsync(int productId, int page, int pageSize, string sortBy, bool? verifiedOnly);
        Task<object> GetProductRatingSummaryAsync(int productId);
    }

    public class CatalogProxy : ICatalogProxy
    {
        private readonly ApiUrls _apiUrls;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CatalogProxy(
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

        private void AddAcceptLanguageHeader()
        {
            var acceptLanguage = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", acceptLanguage);
            }
        }

        public async Task<DataCollection<ProductDto>> GetAllAsync(int page, int take, IEnumerable<int> clients = null)
        {
            AddAcceptLanguageHeader();

            var ids = string.Join(',', clients ?? new List<int>());

            var request = await _httpClient.GetAsync($"{_apiUrls.CatalogUrl}v1/products?page={page}&take={take}&ids={ids}");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<DataCollection<ProductDto>>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<ProductDto> GetAsync(int id)
        {
            AddAcceptLanguageHeader();

            var request = await _httpClient.GetAsync($"{_apiUrls.CatalogUrl}v1/products/{id}");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<ProductDto>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<ProductSearchResponse> SearchAsync(ProductSearchRequest request)
        {
            AddAcceptLanguageHeader();

            // Construir query string
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Query))
                queryParams.Add($"query={System.Uri.EscapeDataString(request.Query)}");

            queryParams.Add($"page={request.Page}");
            queryParams.Add($"pageSize={request.PageSize}");
            queryParams.Add($"sortBy={request.SortBy}");
            queryParams.Add($"sortOrder={request.SortOrder}");

            if (request.CategoryId.HasValue)
                queryParams.Add($"categoryId={request.CategoryId.Value}");

            if (!string.IsNullOrWhiteSpace(request.BrandIds))
                queryParams.Add($"brandIds={System.Uri.EscapeDataString(request.BrandIds)}");

            if (request.MinPrice.HasValue)
                queryParams.Add($"minPrice={request.MinPrice.Value}");

            if (request.MaxPrice.HasValue)
                queryParams.Add($"maxPrice={request.MaxPrice.Value}");

            if (request.InStock.HasValue)
                queryParams.Add($"inStock={request.InStock.Value}");

            if (request.IsFeatured.HasValue)
                queryParams.Add($"isFeatured={request.IsFeatured.Value}");

            if (request.HasDiscount.HasValue)
                queryParams.Add($"hasDiscount={request.HasDiscount.Value}");

            if (request.MinRating.HasValue)
                queryParams.Add($"minRating={request.MinRating.Value}");

            var queryString = string.Join("&", queryParams);
            var url = $"{_apiUrls.CatalogUrl}v1/products/search?{queryString}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<ProductSearchResponse>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<ProductAdvancedSearchResponse> SearchAdvancedAsync(ProductAdvancedSearchRequest request)
        {
            AddAcceptLanguageHeader();

            // Serializar el request a JSON
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{_apiUrls.CatalogUrl}v1/products/search/advanced";

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<ProductAdvancedSearchResponse>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<object> GetProductReviewsAsync(int productId, int page, int pageSize, string sortBy, bool? verifiedOnly)
        {
            AddAcceptLanguageHeader();

            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrEmpty(sortBy))
                queryParams.Add($"sortBy={sortBy}");

            if (verifiedOnly.HasValue)
                queryParams.Add($"verifiedOnly={verifiedOnly.Value}");

            var queryString = string.Join("&", queryParams);
            var url = $"{_apiUrls.CatalogUrl}v1/products/{productId}/reviews?{queryString}";

            var request = await _httpClient.GetAsync(url);
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<object>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<object> GetProductRatingSummaryAsync(int productId)
        {
            AddAcceptLanguageHeader();

            var url = $"{_apiUrls.CatalogUrl}v1/products/{productId}/reviews/summary";

            var request = await _httpClient.GetAsync(url);
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
