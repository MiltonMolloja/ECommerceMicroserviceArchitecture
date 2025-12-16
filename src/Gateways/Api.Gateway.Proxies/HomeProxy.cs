using Api.Gateway.Models.Catalog.DTOs;
using Api.Gateway.Models.Home.DTOs;
using Api.Gateway.Proxies.Config;
using Api.Gateway.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Gateway.Proxies
{
    public interface IHomeProxy
    {
        /// <summary>
        /// Obtiene todos los datos de la página Home en una sola llamada
        /// </summary>
        Task<HomePageResponse> GetHomePageDataAsync(int productsPerSection = 8);

        /// <summary>
        /// Obtiene los banners activos
        /// </summary>
        Task<List<BannerDto>> GetBannersAsync(string position = "hero");

        /// <summary>
        /// Obtiene productos destacados
        /// </summary>
        Task<List<ProductDto>> GetFeaturedProductsAsync(int limit = 8);

        /// <summary>
        /// Obtiene ofertas del día
        /// </summary>
        Task<List<ProductDto>> GetDealsAsync(int limit = 8);

        /// <summary>
        /// Obtiene productos más vendidos
        /// </summary>
        Task<List<ProductDto>> GetBestSellersAsync(int limit = 8);

        /// <summary>
        /// Obtiene novedades
        /// </summary>
        Task<List<ProductDto>> GetNewArrivalsAsync(int limit = 8);

        /// <summary>
        /// Obtiene productos mejor valorados
        /// </summary>
        Task<List<ProductDto>> GetTopRatedAsync(int limit = 8, decimal minRating = 4);

        /// <summary>
        /// Obtiene categorías destacadas
        /// </summary>
        Task<List<CategoryDto>> GetFeaturedCategoriesAsync(int limit = 8);
    }

    public class HomeProxy : IHomeProxy
    {
        private readonly ApiUrls _apiUrls;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public HomeProxy(
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

        public async Task<HomePageResponse> GetHomePageDataAsync(int productsPerSection = 8)
        {
            AddAcceptLanguageHeader();

            var url = $"{_apiUrls.CatalogUrl}v1/Home?productsPerSection={productsPerSection}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<HomePageResponse>(
                await response.Content.ReadAsStringAsync(),
                _jsonOptions
            );
        }

        public async Task<List<BannerDto>> GetBannersAsync(string position = "hero")
        {
            AddAcceptLanguageHeader();

            var url = $"{_apiUrls.CatalogUrl}v1/Home/banners?position={position}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<List<BannerDto>>(
                await response.Content.ReadAsStringAsync(),
                _jsonOptions
            );
        }

        public async Task<List<ProductDto>> GetFeaturedProductsAsync(int limit = 8)
        {
            AddAcceptLanguageHeader();

            var url = $"{_apiUrls.CatalogUrl}v1/Home/featured?limit={limit}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<List<ProductDto>>(
                await response.Content.ReadAsStringAsync(),
                _jsonOptions
            );
        }

        public async Task<List<ProductDto>> GetDealsAsync(int limit = 8)
        {
            AddAcceptLanguageHeader();

            var url = $"{_apiUrls.CatalogUrl}v1/Home/deals?limit={limit}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<List<ProductDto>>(
                await response.Content.ReadAsStringAsync(),
                _jsonOptions
            );
        }

        public async Task<List<ProductDto>> GetBestSellersAsync(int limit = 8)
        {
            AddAcceptLanguageHeader();

            var url = $"{_apiUrls.CatalogUrl}v1/Home/bestsellers?limit={limit}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<List<ProductDto>>(
                await response.Content.ReadAsStringAsync(),
                _jsonOptions
            );
        }

        public async Task<List<ProductDto>> GetNewArrivalsAsync(int limit = 8)
        {
            AddAcceptLanguageHeader();

            var url = $"{_apiUrls.CatalogUrl}v1/Home/new-arrivals?limit={limit}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<List<ProductDto>>(
                await response.Content.ReadAsStringAsync(),
                _jsonOptions
            );
        }

        public async Task<List<ProductDto>> GetTopRatedAsync(int limit = 8, decimal minRating = 4)
        {
            AddAcceptLanguageHeader();

            var url = $"{_apiUrls.CatalogUrl}v1/Home/top-rated?limit={limit}&minRating={minRating}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<List<ProductDto>>(
                await response.Content.ReadAsStringAsync(),
                _jsonOptions
            );
        }

        public async Task<List<CategoryDto>> GetFeaturedCategoriesAsync(int limit = 8)
        {
            AddAcceptLanguageHeader();

            var url = $"{_apiUrls.CatalogUrl}v1/Home/categories?limit={limit}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<List<CategoryDto>>(
                await response.Content.ReadAsStringAsync(),
                _jsonOptions
            );
        }
    }
}
