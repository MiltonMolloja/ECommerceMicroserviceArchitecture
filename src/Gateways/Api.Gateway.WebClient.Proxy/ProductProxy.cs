using Api.Gateway.Models;
using Api.Gateway.Models.Catalog.DTOs;
using Api.Gateway.WebClient.Proxy.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Gateway.WebClient.Proxy
{
    public interface IProductProxy
    {
        Task<DataCollection<ProductDto>> GetAllAsync(int page, int take);
        Task<ProductSearchResponse> SearchAsync(ProductSearchRequest request);
    }

    public class ProductProxy : IProductProxy
    {
        private readonly string _apiGatewayUrl;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductProxy> _logger;

        public ProductProxy(
            HttpClient httpClient,
            ApiGatewayUrl apiGatewayUrl,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProductProxy> logger)
        {
            httpClient.AddBearerToken(httpContextAccessor);
            _httpClient = httpClient;
            _apiGatewayUrl = apiGatewayUrl.Value;
            _logger = logger;
        }

        public async Task<DataCollection<ProductDto>> GetAllAsync(int page, int take)
        {
            var request = await _httpClient.GetAsync($"{_apiGatewayUrl}products?page={page}&take={take}");
            request.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<DataCollection<ProductDto>>(
                await request.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

        public async Task<ProductSearchResponse> SearchAsync(ProductSearchRequest searchRequest)
        {
            // Construir query string
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(searchRequest.Query))
                queryParams.Add($"query={Uri.EscapeDataString(searchRequest.Query)}");

            queryParams.Add($"page={searchRequest.Page}");
            queryParams.Add($"pageSize={searchRequest.PageSize}");
            queryParams.Add($"sortBy={searchRequest.SortBy}");
            queryParams.Add($"sortOrder={searchRequest.SortOrder}");

            if (searchRequest.CategoryId.HasValue)
                queryParams.Add($"categoryId={searchRequest.CategoryId.Value}");

            if (!string.IsNullOrWhiteSpace(searchRequest.BrandIds))
                queryParams.Add($"brandIds={Uri.EscapeDataString(searchRequest.BrandIds)}");

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
            var url = $"{_apiGatewayUrl}products/search?{queryString}";

            // DEBUG: Log de la URL completa
            _logger.LogInformation($"🌐 ProductProxy - URL generada: {url}");
            _logger.LogInformation($"🔍 ProductProxy - HasDiscount en request: {searchRequest.HasDiscount}");

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
    }
}
