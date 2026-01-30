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

        public async Task<ProductSearchResponse> SearchAsync(ProductSearchRequest request)
        {
            // Construir query string
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Query))
                queryParams.Add($"query={Uri.EscapeDataString(request.Query)}");

            queryParams.Add($"page={request.Page}");
            queryParams.Add($"pageSize={request.PageSize}");
            queryParams.Add($"sortBy={request.SortBy}");
            queryParams.Add($"sortOrder={request.SortOrder}");

            if (request.CategoryId.HasValue)
                queryParams.Add($"categoryId={request.CategoryId.Value}");

            if (!string.IsNullOrWhiteSpace(request.BrandIds))
                queryParams.Add($"brandIds={Uri.EscapeDataString(request.BrandIds)}");

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
            var url = $"{_apiGatewayUrl}products/search?{queryString}";

            // DEBUG: Log de la URL completa
            _logger.LogInformation($"🌐 ProductProxy - URL generada: {url}");
            _logger.LogInformation($"🔍 ProductProxy - HasDiscount en request: {request.HasDiscount}");

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
    }
}
