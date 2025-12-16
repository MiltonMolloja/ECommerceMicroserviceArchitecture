using Api.Gateway.Models;
using Api.Gateway.Models.Catalog.DTOs;
using Api.Gateway.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Gateway.Proxies
{
    public interface ICategoryProxy
    {
        Task<DataCollection<CategoryDetailDto>> GetAllAsync(int page, int take, string language);
        Task<CategoryDetailDto> GetAsync(int id, string language);
        Task<CategoryDetailDto> GetBySlugAsync(string slug, string language);
        Task<List<CategoryTreeDto>> GetCategoryTreeAsync(string language);
        Task<List<CategoryDto>> GetRootCategoriesAsync(string language);
        Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId, string language);
        Task<List<CategoryBreadcrumbDto>> GetBreadcrumbsAsync(int categoryId, string language);
        Task<List<CategoryBreadcrumbDto>> GetBreadcrumbsBySlugAsync(string slug, string language);
    }

    public class CategoryProxy : ICategoryProxy
    {
        private readonly HttpClient _httpClient;
        private readonly ApiUrls _apiUrls;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CategoryProxy(
            HttpClient httpClient,
            IOptions<ApiUrls> apiUrls,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _apiUrls = apiUrls.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DataCollection<CategoryDetailDto>> GetAllAsync(int page, int take, string language)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_apiUrls.CatalogUrl}/v1/categories?page={page}&take={take}");
            
            request.Headers.Add("Accept-Language", language);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DataCollection<CategoryDetailDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<CategoryDetailDto> GetAsync(int id, string language)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_apiUrls.CatalogUrl}/v1/categories/{id}");
            
            request.Headers.Add("Accept-Language", language);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CategoryDetailDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<CategoryDetailDto> GetBySlugAsync(string slug, string language)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_apiUrls.CatalogUrl}/v1/categories/by-slug/{slug}");
            
            request.Headers.Add("Accept-Language", language);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CategoryDetailDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<List<CategoryTreeDto>> GetCategoryTreeAsync(string language)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_apiUrls.CatalogUrl}/v1/categories/tree");
            
            request.Headers.Add("Accept-Language", language);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CategoryTreeDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<List<CategoryDto>> GetRootCategoriesAsync(string language)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_apiUrls.CatalogUrl}/v1/categories/root");
            
            request.Headers.Add("Accept-Language", language);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CategoryDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId, string language)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_apiUrls.CatalogUrl}/v1/categories/{parentId}/subcategories");
            
            request.Headers.Add("Accept-Language", language);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CategoryDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<List<CategoryBreadcrumbDto>> GetBreadcrumbsAsync(int categoryId, string language)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_apiUrls.CatalogUrl}/v1/categories/{categoryId}/breadcrumbs");
            
            request.Headers.Add("Accept-Language", language);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CategoryBreadcrumbDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<List<CategoryBreadcrumbDto>> GetBreadcrumbsBySlugAsync(string slug, string language)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_apiUrls.CatalogUrl}/v1/categories/by-slug/{slug}/breadcrumbs");
            
            request.Headers.Add("Accept-Language", language);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CategoryBreadcrumbDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
