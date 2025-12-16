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

        public async Task<ProductSearchResponse> SearchAsync(ProductSearchRequest searchRequest)
        {
            AddAcceptLanguageHeader();

            // Construir query string
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(searchRequest.Query))
                queryParams.Add($"query={System.Uri.EscapeDataString(searchRequest.Query)}");

            queryParams.Add($"page={searchRequest.Page}");
            queryParams.Add($"pageSize={searchRequest.PageSize}");
            queryParams.Add($"sortBy={searchRequest.SortBy}");
            queryParams.Add($"sortOrder={searchRequest.SortOrder}");

            if (searchRequest.CategoryId.HasValue)
                queryParams.Add($"categoryId={searchRequest.CategoryId.Value}");

            if (!string.IsNullOrWhiteSpace(searchRequest.BrandIds))
                queryParams.Add($"brandIds={System.Uri.EscapeDataString(searchRequest.BrandIds)}");

            if (searchRequest.MinPrice.HasValue)
                queryParams.Add($"minPrice={searchRequest.MinPrice.Value}");

            if (searchRequest.MaxPrice.HasValue)
                queryParams.Add($"maxPrice={searchRequest.MaxPrice.Value}");

            if (searchRequest.InStock.HasValue)
                queryParams.Add($"inStock={searchRequest.InStock.Value}");

            if (searchRequest.IsFeatured.HasValue)
                queryParams.Add($"isFeatured={searchRequest.IsFeatured.Value}");

            if (searchRequest.HasDiscount.HasValue)
                queryParams.Add($"hasDiscount={searchRequest.HasDiscount.Value}");

            if (searchRequest.MinRating.HasValue)
                queryParams.Add($"minRating={searchRequest.MinRating.Value}");

            var queryString = string.Join("&", queryParams);
            var url = $"{_apiUrls.CatalogUrl}v1/products/search?{queryString}";

            var request = await _httpClient.GetAsync(url);
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<ProductSearchResponse>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<ProductAdvancedSearchResponse> SearchAdvancedAsync(ProductAdvancedSearchRequest searchRequest)
        {
            AddAcceptLanguageHeader();

            // Serializar el request a JSON
            var json = JsonSerializer.Serialize(searchRequest, new JsonSerializerOptions
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
