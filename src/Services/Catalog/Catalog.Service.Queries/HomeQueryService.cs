using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Catalog.Domain;
using Catalog.Persistence.Database;
using Catalog.Service.Queries.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Catalog.Service.Queries
{
    /// <summary>
    /// Implementación del servicio de consultas para la página Home.
    /// Arquitectura híbrida: endpoint agregador + endpoints individuales con cache inteligente.
    /// </summary>
    public class HomeQueryService : IHomeQueryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly IProductQueryService _productService;
        private readonly ILogger<HomeQueryService> _logger;

        public HomeQueryService(
            ApplicationDbContext context,
            IDistributedCache cache,
            IProductQueryService productService,
            ILogger<HomeQueryService> logger)
        {
            _context = context;
            _cache = cache;
            _productService = productService;
            _logger = logger;
        }

        #region Endpoint Agregador

        /// <summary>
        /// Obtiene todos los datos de Home en paralelo con cache inteligente.
        /// Ejecuta 7 queries en paralelo para optimizar performance.
        /// </summary>
        public async Task<HomePageResponse> GetHomePageDataAsync(string language, int productsPerSection = 8)
        {
            var cacheKey = $"home:page:{language}:{productsPerSection}";

            // 1. Intentar obtener de cache
            var cached = await GetFromCacheAsync<HomePageResponse>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Home page data retrieved from cache for language: {Language}", language);
                cached.Metadata.FromCache = true;
                return cached;
            }

            // 2. Ejecutar todas las queries secuencialmente
            // Nota: No podemos usar Task.WhenAll porque DbContext no es thread-safe
            var stopwatch = Stopwatch.StartNew();

            var banners = await GetBannersAsync("hero", language);
            var featured = await GetFeaturedProductsAsync(productsPerSection, language);
            var deals = await GetDealsAsync(productsPerSection, language);
            var bestsellers = await GetBestSellersAsync(productsPerSection, language);
            var newArrivals = await GetNewArrivalsAsync(productsPerSection, language);
            var topRated = await GetTopRatedAsync(productsPerSection, 4, language);
            var categories = await GetFeaturedCategoriesAsync(productsPerSection, language);

            stopwatch.Stop();

            // 3. Componer respuesta
            var response = new HomePageResponse
            {
                Banners = banners,
                FeaturedProducts = featured,
                Deals = deals,
                BestSellers = bestsellers,
                NewArrivals = newArrivals,
                TopRated = topRated,
                FeaturedCategories = categories,
                Metadata = new HomeMetadata
                {
                    Language = language,
                    GeneratedAt = DateTime.UtcNow,
                    CacheDurationSeconds = 300,
                    QueryExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                    FromCache = false
                }
            };

            // 4. Guardar en cache por 5 minutos
            await SetCacheAsync(cacheKey, response, TimeSpan.FromMinutes(5));

            _logger.LogInformation(
                "Home page data generated in {ElapsedMs}ms for language {Language}",
                stopwatch.ElapsedMilliseconds,
                language);

            return response;
        }

        #endregion

        #region Endpoints Individuales

        /// <summary>
        /// Obtiene banners activos y vigentes para una posición específica.
        /// </summary>
        public async Task<List<BannerDto>> GetBannersAsync(string position, string language)
        {
            var cacheKey = $"home:banners:{position}:{language}";

            var cached = await GetFromCacheAsync<List<BannerDto>>(cacheKey);
            if (cached != null) return cached;

            var isSpanish = language.StartsWith("es", StringComparison.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;

            var banners = await _context.Banners
                .Where(b => b.IsActive)
                .Where(b => b.Position == position)
                .Where(b => !b.StartDate.HasValue || b.StartDate <= now)
                .Where(b => !b.EndDate.HasValue || b.EndDate >= now)
                .OrderBy(b => b.DisplayOrder)
                .Select(b => new BannerDto
                {
                    BannerId = b.BannerId,
                    Title = isSpanish ? b.TitleSpanish : b.TitleEnglish,
                    Subtitle = isSpanish ? b.SubtitleSpanish : b.SubtitleEnglish,
                    ImageUrl = b.ImageUrl,
                    ImageUrlMobile = b.ImageUrlMobile,
                    LinkUrl = b.LinkUrl,
                    ButtonText = isSpanish ? b.ButtonTextSpanish : b.ButtonTextEnglish,
                    DisplayOrder = b.DisplayOrder
                })
                .ToListAsync();

            await SetCacheAsync(cacheKey, banners, TimeSpan.FromMinutes(10));
            return banners;
        }

        /// <summary>
        /// Obtiene productos destacados (IsFeatured = true).
        /// </summary>
        public async Task<List<ProductDto>> GetFeaturedProductsAsync(int limit, string language)
        {
            var cacheKey = $"home:featured:{limit}:{language}";

            var cached = await GetFromCacheAsync<List<ProductDto>>(cacheKey);
            if (cached != null) return cached;

            var request = new ProductSearchRequest
            {
                IsFeatured = true,
                PageSize = limit,
                SortBy = ProductSortField.Relevance,
                SortOrder = SortOrder.Descending
            };

            var result = await _productService.SearchAsync(request);
            var products = result.Items.ToList();

            await SetCacheAsync(cacheKey, products, TimeSpan.FromMinutes(5));
            return products;
        }

        /// <summary>
        /// Obtiene ofertas del día (productos con descuento).
        /// </summary>
        public async Task<List<ProductDto>> GetDealsAsync(int limit, string language)
        {
            var cacheKey = $"home:deals:{limit}:{language}";

            var cached = await GetFromCacheAsync<List<ProductDto>>(cacheKey);
            if (cached != null) return cached;

            var request = new ProductSearchRequest
            {
                HasDiscount = true,
                PageSize = limit,
                SortBy = ProductSortField.Discount,
                SortOrder = SortOrder.Descending
            };

            var result = await _productService.SearchAsync(request);
            var products = result.Items.ToList();

            // Cache más corto para ofertas (1 minuto)
            await SetCacheAsync(cacheKey, products, TimeSpan.FromMinutes(1));
            return products;
        }

        /// <summary>
        /// Obtiene productos más vendidos (ordenados por TotalSold).
        /// </summary>
        public async Task<List<ProductDto>> GetBestSellersAsync(int limit, string language)
        {
            var cacheKey = $"home:bestsellers:{limit}:{language}";

            var cached = await GetFromCacheAsync<List<ProductDto>>(cacheKey);
            if (cached != null) return cached;

            var request = new ProductSearchRequest
            {
                PageSize = limit,
                SortBy = ProductSortField.Bestseller,
                SortOrder = SortOrder.Descending
            };

            var result = await _productService.SearchAsync(request);
            var products = result.Items.ToList();

            await SetCacheAsync(cacheKey, products, TimeSpan.FromMinutes(5));
            return products;
        }

        /// <summary>
        /// Obtiene novedades (productos recientes).
        /// </summary>
        public async Task<List<ProductDto>> GetNewArrivalsAsync(int limit, string language)
        {
            var cacheKey = $"home:new-arrivals:{limit}:{language}";

            var cached = await GetFromCacheAsync<List<ProductDto>>(cacheKey);
            if (cached != null) return cached;

            var request = new ProductSearchRequest
            {
                PageSize = limit,
                SortBy = ProductSortField.Newest,
                SortOrder = SortOrder.Descending
            };

            var result = await _productService.SearchAsync(request);
            var products = result.Items.ToList();

            await SetCacheAsync(cacheKey, products, TimeSpan.FromMinutes(5));
            return products;
        }

        /// <summary>
        /// Obtiene productos mejor valorados.
        /// </summary>
        public async Task<List<ProductDto>> GetTopRatedAsync(int limit, decimal minRating, string language)
        {
            var cacheKey = $"home:top-rated:{limit}:{minRating}:{language}";

            var cached = await GetFromCacheAsync<List<ProductDto>>(cacheKey);
            if (cached != null) return cached;

            var request = new ProductSearchRequest
            {
                PageSize = limit,
                MinRating = minRating,
                SortBy = ProductSortField.Rating,
                SortOrder = SortOrder.Descending
            };

            var result = await _productService.SearchAsync(request);
            var products = result.Items.ToList();

            await SetCacheAsync(cacheKey, products, TimeSpan.FromMinutes(5));
            return products;
        }

        /// <summary>
        /// Obtiene categorías destacadas para mostrar en Home.
        /// </summary>
        public async Task<List<CategoryDto>> GetFeaturedCategoriesAsync(int limit, string language)
        {
            var cacheKey = $"home:categories:{limit}:{language}";

            var cached = await GetFromCacheAsync<List<CategoryDto>>(cacheKey);
            if (cached != null) return cached;

            var isSpanish = language.StartsWith("es", StringComparison.OrdinalIgnoreCase);

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .Where(c => c.IsFeatured) // Filtrar por categorías destacadas
                .OrderBy(c => c.DisplayOrder)
                .Take(limit)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = isSpanish ? c.NameSpanish : c.NameEnglish,
                    Slug = c.Slug,
                    ImageUrl = c.ImageUrl,
                    ProductCount = c.ProductCategories.Count,
                    IsFeatured = c.IsFeatured
                })
                .ToListAsync();

            await SetCacheAsync(cacheKey, categories, TimeSpan.FromMinutes(10));
            return categories;
        }

        #endregion

        #region Cache Helpers

        /// <summary>
        /// Obtiene un objeto del cache distribuido (Redis).
        /// </summary>
        private async Task<T?> GetFromCacheAsync<T>(string key) where T : class
        {
            try
            {
                var cached = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cached))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<T>(cached);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading from cache: {Key}", key);
                return null;
            }
        }

        /// <summary>
        /// Guarda un objeto en el cache distribuido (Redis).
        /// </summary>
        private async Task SetCacheAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };

                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, options);

                _logger.LogDebug("Cached data for key: {Key} with expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error writing to cache: {Key}", key);
                // No lanzar excepción, solo loguear el error
            }
        }

        #endregion
    }
}
