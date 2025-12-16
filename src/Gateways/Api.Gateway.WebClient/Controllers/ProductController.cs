using Api.Gateway.Models;
using Common.Caching;
using Api.Gateway.Models.Catalog.DTOs;
using Api.Gateway.Proxies;
using Common.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Gateway.WebClient.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductController : ControllerBase
    {
        private readonly ICatalogProxy _catalogProxy;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;
        private readonly ILogger<ProductController> _logger;
        private readonly ILanguageAwareCacheKeyProvider _cacheKeyProvider;

        public ProductController(
            ICatalogProxy catalogProxy,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings,
            ILogger<ProductController> logger,
            ILanguageAwareCacheKeyProvider cacheKeyProvider
        )
        {
            _catalogProxy = catalogProxy;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
            _cacheKeyProvider = cacheKeyProvider;
        }

        [HttpGet]
        public async Task<DataCollection<ProductDto>> GetAll(int page, int take)
        {
            // Generate language-aware cache key
            var baseCacheKey = $"gateway:products:all:page:{page}:take:{take}";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            // Intentar obtener del caché
            var cachedProducts = await _cacheService.GetAsync<DataCollection<ProductDto>>(cacheKey);
            if (cachedProducts != null)
            {
                _logger.LogInformation($"Products retrieved from cache: {cacheKey}");
                return cachedProducts;
            }

            // Si no está en caché, llamar al servicio
            var products = await _catalogProxy.GetAllAsync(page, take);

            // Guardar en caché
            await _cacheService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Products cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return products;
        }

        [HttpGet("{id}")]
        public async Task<ProductDto> Get(int id)
        {
            // Generate language-aware cache key
            var baseCacheKey = $"gateway:products:id:{id}";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            // Intentar obtener del caché
            var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
            if (cachedProduct != null)
            {
                _logger.LogInformation($"Product retrieved from cache: {cacheKey}");
                return cachedProduct;
            }

            // Si no está en caché, llamar al servicio
            var product = await _catalogProxy.GetAsync(id);

            // Guardar en caché
            if (product != null)
            {
                await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
                _logger.LogInformation($"Product cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");
            }

            return product;
        }

        /// <summary>
        /// Busca productos con filtros y ordenamiento
        /// </summary>
        /// <param name="request">Parámetros de búsqueda</param>
        /// <returns>Colección paginada de productos con metadata</returns>
        /// <response code="200">Búsqueda exitosa</response>
        /// <response code="400">Parámetros inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ProductSearchResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProductSearchResponse>> Search([FromQuery] ProductSearchRequest request)
        {
            try
            {
                // Verificar si hay filtros de atributos en la query string (filter_attr_X)
                var attributeFilters = ParseAttributeFiltersFromQuery();

                // Si hay filtros de atributos, usar búsqueda avanzada
                if (attributeFilters.Any())
                {
                    _logger.LogInformation($"Gateway: Detected {attributeFilters.Count} attribute filters, using advanced search");
                    
                    // Convertir ProductSearchRequest a ProductAdvancedSearchRequest
                    var advancedRequest = new ProductAdvancedSearchRequest
                    {
                        Query = request.Query,
                        Page = request.Page,
                        PageSize = request.PageSize,
                        SortBy = request.SortBy,
                        SortOrder = request.SortOrder,
                        MinPrice = request.MinPrice,
                        MaxPrice = request.MaxPrice,
                        InStock = request.InStock,
                        IsFeatured = request.IsFeatured,
                        HasDiscount = request.HasDiscount,
                        MinAverageRating = request.MinRating,
                        Attributes = attributeFilters,
                        IncludeBrandFacets = true,
                        IncludeCategoryFacets = true,
                        IncludeAttributeFacets = true,
                        IncludePriceFacets = true,
                        IncludeRatingFacets = true
                    };

                    // Agregar CategoryId si existe
                    if (request.CategoryId.HasValue)
                    {
                        advancedRequest.CategoryIds = new List<int> { request.CategoryId.Value };
                    }

                    // Generar clave de caché language-aware para búsqueda avanzada
                    var advancedCacheKey = _cacheKeyProvider.GenerateKey(GenerateAdvancedSearchCacheKey(advancedRequest));

                    // Intentar obtener del caché
                    var cachedAdvancedResult = await _cacheService.GetAsync<ProductAdvancedSearchResponse>(advancedCacheKey);
                    if (cachedAdvancedResult != null)
                    {
                        _logger.LogInformation($"Gateway: Advanced search with attributes retrieved from cache: {advancedCacheKey}");
                        
                        // Convertir resultado avanzado cacheado a resultado simple
                        var cachedSimpleResult = new ProductSearchResponse
                        {
                            Items = cachedAdvancedResult.Items,
                            Total = cachedAdvancedResult.Total,
                            Page = cachedAdvancedResult.Page,
                            Pages = cachedAdvancedResult.PageCount,
                            Metadata = cachedAdvancedResult.Metadata != null ? new SearchMetadata
                            {
                                Query = cachedAdvancedResult.Metadata.Query,
                                ExecutionTime = cachedAdvancedResult.Metadata.Performance?.TotalExecutionTime ?? 0,
                                AppliedFilters = new AppliedFilters
                                {
                                    MinPrice = request.MinPrice,
                                    MaxPrice = request.MaxPrice,
                                    InStock = request.InStock,
                                    IsFeatured = request.IsFeatured,
                                    HasDiscount = request.HasDiscount,
                                    CategoryId = request.CategoryId,
                                    SortBy = request.SortBy.ToString(),
                                    SortOrder = request.SortOrder.ToString()
                                }
                            } : null
                        };
                        return Ok(cachedSimpleResult);
                    }

                    // Ejecutar búsqueda avanzada a través del proxy
                    var startTimeAdv = DateTime.UtcNow;
                    var advancedResult = await _catalogProxy.SearchAdvancedAsync(advancedRequest);
                    var executionTimeAdv = (long)(DateTime.UtcNow - startTimeAdv).TotalMilliseconds;

                    // Guardar en caché
                    await _cacheService.SetAsync(
                        advancedCacheKey,
                        advancedResult,
                        TimeSpan.FromMinutes(2) // Facetas cambian más frecuentemente
                    );

                    _logger.LogInformation(
                        $"Gateway: Advanced search with attributes executed in {executionTimeAdv}ms and cached: {advancedCacheKey}"
                    );
                    
                    // Convertir resultado avanzado a resultado simple
                    var simpleResult = new ProductSearchResponse
                    {
                        Items = advancedResult.Items,
                        Total = advancedResult.Total,
                        Page = advancedResult.Page,
                        Pages = advancedResult.PageCount,
                        Metadata = advancedResult.Metadata != null ? new SearchMetadata
                        {
                            Query = advancedResult.Metadata.Query,
                            ExecutionTime = advancedResult.Metadata.Performance?.TotalExecutionTime ?? 0,
                            AppliedFilters = new AppliedFilters
                            {
                                MinPrice = request.MinPrice,
                                MaxPrice = request.MaxPrice,
                                InStock = request.InStock,
                                IsFeatured = request.IsFeatured,
                                HasDiscount = request.HasDiscount,
                                CategoryId = request.CategoryId,
                                SortBy = request.SortBy.ToString(),
                                SortOrder = request.SortOrder.ToString()
                            }
                        } : null
                    };
                    return Ok(simpleResult);
                }

                // Búsqueda simple sin filtros de atributos
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validaciones adicionales
                if (request.MinPrice.HasValue && request.MaxPrice.HasValue &&
                    request.MinPrice > request.MaxPrice)
                {
                    ModelState.AddModelError("MaxPrice", "MaxPrice must be greater than MinPrice");
                    return BadRequest(ModelState);
                }

                // Generar clave de caché language-aware
                var baseCacheKey = GenerateSearchCacheKey(request);
                var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

                // Intentar obtener del caché
                var cachedResult = await _cacheService.GetAsync<ProductSearchResponse>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation($"Gateway: Search results retrieved from cache: {cacheKey}");
                    return Ok(cachedResult);
                }

                // Ejecutar búsqueda a través del proxy
                var startTime = DateTime.UtcNow;
                var result = await _catalogProxy.SearchAsync(request);
                var executionTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

                // Guardar en caché
                await _cacheService.SetAsync(
                    cacheKey,
                    result,
                    TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes)
                );

                _logger.LogInformation(
                    $"Gateway: Search executed in {executionTime}ms and cached: {cacheKey}"
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gateway: Error searching products with request: {@Request}", request);
                return StatusCode(500, new
                {
                    message = "Error searching products",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Busca productos con filtros avanzados, facetas dinámicas y Full-Text Search
        /// </summary>
        /// <param name="request">Parámetros de búsqueda avanzada</param>
        /// <returns>Resultados de búsqueda con facetas y metadata</returns>
        /// <response code="200">Búsqueda exitosa con facetas</response>
        /// <response code="400">Parámetros inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("search/advanced")]
        [ProducesResponseType(typeof(ProductAdvancedSearchResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProductAdvancedSearchResponse>> SearchAdvanced([FromBody] ProductAdvancedSearchRequest request)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validaciones adicionales
                if (request.MinPrice.HasValue && request.MaxPrice.HasValue &&
                    request.MinPrice > request.MaxPrice)
                {
                    ModelState.AddModelError("MaxPrice", "MaxPrice must be greater than MinPrice");
                    return BadRequest(ModelState);
                }

                if (request.MinAverageRating.HasValue &&
                    (request.MinAverageRating < 0 || request.MinAverageRating > 5))
                {
                    ModelState.AddModelError("MinAverageRating", "MinAverageRating must be between 0 and 5");
                    return BadRequest(ModelState);
                }

                // Generar clave de caché language-aware
                var baseCacheKey = GenerateAdvancedSearchCacheKey(request);
                var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

                // Intentar obtener del caché (solo si no se solicitan facetas, ya que pueden cambiar frecuentemente)
                if (!request.IncludeBrandFacets && !request.IncludeCategoryFacets &&
                    !request.IncludePriceFacets && !request.IncludeRatingFacets &&
                    !request.IncludeAttributeFacets)
                {
                    var cachedResult = await _cacheService.GetAsync<ProductAdvancedSearchResponse>(cacheKey);
                    if (cachedResult != null)
                    {
                        _logger.LogInformation($"Gateway: Advanced search results retrieved from cache: {cacheKey}");
                        if (cachedResult.Metadata?.Performance != null)
                        {
                            cachedResult.Metadata.Performance.CacheHit = true;
                        }
                        return Ok(cachedResult);
                    }
                }

                // Ejecutar búsqueda avanzada a través del proxy
                var startTime = DateTime.UtcNow;
                var result = await _catalogProxy.SearchAdvancedAsync(request);
                var executionTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

                // Guardar en caché (con TTL más corto para búsquedas con facetas)
                var cacheDuration = (request.IncludeBrandFacets || request.IncludeCategoryFacets ||
                                    request.IncludePriceFacets || request.IncludeRatingFacets ||
                                    request.IncludeAttributeFacets)
                    ? TimeSpan.FromMinutes(2) // Facetas cambian más frecuentemente
                    : TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes);

                await _cacheService.SetAsync(cacheKey, result, cacheDuration);

                _logger.LogInformation(
                    $"Gateway: Advanced search executed in {executionTime}ms (Total: {result.Metadata?.Performance?.TotalExecutionTime}ms) " +
                    $"and cached: {cacheKey}"
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gateway: Error in advanced search with request: {@Request}", request);
                return StatusCode(500, new
                {
                    message = "Error executing advanced search",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Parsea los filtros de atributos desde la query string (filter_attr_X=valueId)
        /// </summary>
        /// <returns>Diccionario de atributos con sus valores</returns>
        private Dictionary<string, List<string>> ParseAttributeFiltersFromQuery()
        {
            var attributeFilters = new Dictionary<string, List<string>>();

            // Iterar sobre todos los query parameters
            foreach (var queryParam in Request.Query)
            {
                // Buscar parámetros que empiecen con "filter_attr_"
                if (queryParam.Key.StartsWith("filter_attr_", StringComparison.OrdinalIgnoreCase))
                {
                    // Extraer el ID del atributo (ej: "filter_attr_107" -> "107")
                    var attributeIdStr = queryParam.Key.Substring("filter_attr_".Length);
                    
                    if (int.TryParse(attributeIdStr, out var attributeId))
                    {
                        // Usar el attributeId como clave
                        var attributeKey = attributeId.ToString();
                        
                        if (!attributeFilters.ContainsKey(attributeKey))
                        {
                            attributeFilters[attributeKey] = new List<string>();
                        }

                        // Agregar todos los valores (puede haber múltiples: filter_attr_107=1056&filter_attr_107=1036)
                        foreach (var value in queryParam.Value)
                        {
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                attributeFilters[attributeKey].Add(value);
                                _logger.LogInformation($"Parsed attribute filter: attr_{attributeId} = {value}");
                            }
                        }
                    }
                }
            }

            return attributeFilters;
        }

        /// <summary>
        /// Genera una clave de caché única basada en los parámetros de búsqueda
        /// </summary>
        private string GenerateSearchCacheKey(ProductSearchRequest request)
        {
            var keyBuilder = new StringBuilder("gateway:products:search:");
            keyBuilder.Append($"q={request.Query ?? "all"}:");
            keyBuilder.Append($"page={request.Page}:");
            keyBuilder.Append($"size={request.PageSize}:");
            keyBuilder.Append($"sort={request.SortBy}:{request.SortOrder}:");
            keyBuilder.Append($"cat={request.CategoryId?.ToString() ?? "all"}:");
            keyBuilder.Append($"brands={request.BrandIds ?? "all"}:");
            keyBuilder.Append($"price={request.MinPrice?.ToString() ?? "0"}-{request.MaxPrice?.ToString() ?? "max"}:");
            keyBuilder.Append($"stock={request.InStock?.ToString() ?? "all"}:");
            keyBuilder.Append($"featured={request.IsFeatured?.ToString() ?? "all"}:");
            keyBuilder.Append($"discount={request.HasDiscount?.ToString() ?? "all"}:");
            keyBuilder.Append($"rating={request.MinRating?.ToString() ?? "all"}");

            return keyBuilder.ToString();
        }

        /// <summary>
        /// Genera una clave de caché única para búsqueda avanzada
        /// </summary>
        private string GenerateAdvancedSearchCacheKey(ProductAdvancedSearchRequest request)
        {
            var keyBuilder = new StringBuilder("gateway:products:search:advanced:");
            keyBuilder.Append($"q={request.Query ?? "all"}:");
            keyBuilder.Append($"page={request.Page}:");
            keyBuilder.Append($"size={request.PageSize}:");
            keyBuilder.Append($"sort={request.SortBy}:{request.SortOrder}:");

            // Categorías
            keyBuilder.Append($"cats={(request.CategoryIds != null && request.CategoryIds.Any() ? string.Join("-", request.CategoryIds.OrderBy(x => x)) : "all")}:");

            // Marcas
            keyBuilder.Append($"brands={(request.BrandIds != null && request.BrandIds.Any() ? string.Join("-", request.BrandIds.OrderBy(x => x)) : "all")}:");

            // Precio
            keyBuilder.Append($"price={request.MinPrice?.ToString() ?? "0"}-{request.MaxPrice?.ToString() ?? "max"}:");

            // Rating
            keyBuilder.Append($"rating={request.MinAverageRating?.ToString() ?? "0"}:");
            keyBuilder.Append($"reviews={request.MinReviewCount?.ToString() ?? "0"}:");

            // Stock y flags
            keyBuilder.Append($"stock={request.InStock?.ToString() ?? "all"}:");
            keyBuilder.Append($"featured={request.IsFeatured?.ToString() ?? "all"}:");
            keyBuilder.Append($"discount={request.HasDiscount?.ToString() ?? "all"}:");

            // Atributos
            if (request.Attributes != null && request.Attributes.Any())
            {
                keyBuilder.Append("attrs=");
                foreach (var attr in request.Attributes.OrderBy(x => x.Key))
                {
                    keyBuilder.Append($"{attr.Key}:{string.Join(",", attr.Value.OrderBy(v => v))};");
                }
            }

            // Facetas solicitadas
            keyBuilder.Append($"facets={request.IncludeBrandFacets},{request.IncludeCategoryFacets},{request.IncludePriceFacets},{request.IncludeRatingFacets},{request.IncludeAttributeFacets}");

            return keyBuilder.ToString();
        }

        /// <summary>
        /// Obtiene las reviews de un producto
        /// </summary>
        [HttpGet("{productId}/reviews")]
        public async Task<IActionResult> GetProductReviews(
            int productId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "helpful",
            [FromQuery] bool? verifiedOnly = null)
        {
            try
            {
                var result = await _catalogProxy.GetProductReviewsAsync(productId, page, pageSize, sortBy, verifiedOnly);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting reviews for product {productId}");
                return StatusCode(500, new { message = "Error retrieving product reviews", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el resumen de ratings de un producto
        /// </summary>
        [HttpGet("{productId}/reviews/summary")]
        public async Task<IActionResult> GetProductRatingSummary(int productId)
        {
            try
            {
                var result = await _catalogProxy.GetProductRatingSummaryAsync(productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting rating summary for product {productId}");
                return StatusCode(500, new { message = "Error retrieving rating summary", error = ex.Message });
            }
        }
    }
}
