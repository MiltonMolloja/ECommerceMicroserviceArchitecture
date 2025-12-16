using Catalog.Common;
using Catalog.Service.EventHandlers.Commands;
using Catalog.Service.Queries;
using Catalog.Service.Queries.DTOs;
using Common.Caching;
using Common.Validation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Common.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Api.Controllers
{
    
    [ApiController]
    [Route("v1/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductQueryService _productQueryService;
        private readonly ILogger<ProductController> _logger;
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;
        private readonly ILanguageAwareCacheKeyProvider _cacheKeyProvider;

        public ProductController(
            ILogger<ProductController> logger,
            IMediator mediator,
            IProductQueryService productQueryService,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings,
            ILanguageAwareCacheKeyProvider cacheKeyProvider)
        {
            _logger = logger;
            _mediator = mediator;
            _productQueryService = productQueryService;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings.Value;
            _cacheKeyProvider = cacheKeyProvider;
        }

        [HttpGet]
        public async Task<DataCollection<ProductDto>> GetAll(int page = 1, int take = 10, string ids = null)
        {
            // Generate language-aware cache key
            var baseCacheKey = $"products:all:page:{page}:take:{take}:ids:{ids ?? "all"}";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            // Intentar obtener del caché
            var cachedProducts = await _cacheService.GetAsync<DataCollection<ProductDto>>(cacheKey);
            if (cachedProducts != null)
            {
                _logger.LogInformation($"Products retrieved from cache: {cacheKey}");
                return cachedProducts;
            }

            IEnumerable<int> products = null;

            if (!string.IsNullOrEmpty(ids))
            {
                products = ids.Split(',').Select(x => Convert.ToInt32(x));
            }

            var result = await _productQueryService.GetAllAsync(page, take, products);

            // Guardar en caché usando configuración de appsettings
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Products cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ProductDto> Get(int id)
        {
            // Generate language-aware cache key
            var baseCacheKey = $"products:id:{id}";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            // Intentar obtener del caché
            var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
            if (cachedProduct != null)
            {
                _logger.LogInformation($"Product retrieved from cache: {cacheKey}");
                return cachedProduct;
            }

            // Si no está en caché, obtener de la base de datos
            var product = await _productQueryService.GetAsync(id);

            // Guardar en caché usando configuración de appsettings
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
                    _logger.LogInformation($"Search results retrieved from cache: {cacheKey}");
                    return Ok(cachedResult);
                }

                // Ejecutar búsqueda
                var startTime = DateTime.UtcNow;
                var result = await _productQueryService.SearchAsync(request);
                var executionTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

                // Agregar metadata de tiempo de ejecución
                if (result.Metadata != null)
                {
                    result.Metadata.ExecutionTime = executionTime;
                }

                // Guardar en caché
                await _cacheService.SetAsync(
                    cacheKey,
                    result,
                    TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes)
                );

                _logger.LogInformation(
                    $"Search executed in {executionTime}ms and cached: {cacheKey}"
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with request: {@Request}", request);
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
                        _logger.LogInformation($"Advanced search results retrieved from cache: {cacheKey}");
                        cachedResult.Metadata.Performance.CacheHit = true;
                        return Ok(cachedResult);
                    }
                }

                // Ejecutar búsqueda avanzada
                var result = await _productQueryService.SearchAdvancedAsync(request);

                // Guardar en caché (con TTL más corto para búsquedas con facetas)
                var cacheDuration = (request.IncludeBrandFacets || request.IncludeCategoryFacets ||
                                    request.IncludePriceFacets || request.IncludeRatingFacets ||
                                    request.IncludeAttributeFacets)
                    ? TimeSpan.FromMinutes(2) // Facetas cambian más frecuentemente
                    : TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes);

                await _cacheService.SetAsync(cacheKey, result, cacheDuration);

                _logger.LogInformation(
                    $"Advanced search executed in {result.Metadata.Performance.TotalExecutionTime}ms " +
                    $"(Query: {result.Metadata.Performance.QueryExecutionTime}ms, " +
                    $"Facets: {result.Metadata.Performance.FacetCalculationTime}ms) " +
                    $"and cached: {cacheKey}"
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced search with request: {@Request}", request);
                return StatusCode(500, new
                {
                    message = "Error executing advanced search",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Genera una clave de caché única basada en los parámetros de búsqueda
        /// </summary>
        private string GenerateSearchCacheKey(ProductSearchRequest request)
        {
            var keyBuilder = new StringBuilder("products:search:");
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
            var keyBuilder = new StringBuilder("products:search:advanced:");
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateCommand notification)
        {
            try
            {
                // Crear el producto
                await _mediator.Publish(notification);

                // Invalidar caché de listado de productos y búsquedas
                await InvalidateProductCaches();

                _logger.LogInformation("Product created successfully and cache invalidated");
                return Ok(new { message = "Product created successfully", success = true });
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation failed for product creation");
                var errors = vex.GetErrorsDictionary();
                return BadRequest(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "One or more validation errors occurred.",
                    status = 400,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "Error creating product", error = ex.Message });
            }
        }

        /// <summary>
        /// Invalida todos los cachés relacionados con productos
        /// </summary>
        private async Task InvalidateProductCaches()
        {
            var pageSizes = new[] { 10, 20, 50, 100 };
            var languages = new[] { "es", "en" };

            foreach (var lang in languages)
            {
                // Invalidar cache de listado
                foreach (var pageSize in pageSizes)
                {
                    for (int page = 1; page <= 20; page++)
                    {
                        var baseCacheKey = $"products:all:page:{page}:take:{pageSize}:ids:all";
                        var cacheKeyToRemove = $"{baseCacheKey}_lang={lang}";
                        await _cacheService.RemoveAsync(cacheKeyToRemove);
                    }
                }

                // Invalidar cache de búsquedas (usando patrón)
                var searchCachePattern = $"products:search:*_lang={lang}";
                // Nota: Esta funcionalidad requiere que ICacheService tenga un método RemoveByPatternAsync
                // Por ahora solo invalidamos algunos casos comunes
                for (int page = 1; page <= 10; page++)
                {
                    var baseCacheKey = $"products:search:q=all:page={page}";
                    var cacheKeyToRemove = $"{baseCacheKey}*_lang={lang}";
                    await _cacheService.RemoveAsync(cacheKeyToRemove);
                }
            }
        }

        /// <summary>
        /// ENDPOINT TEMPORAL - Limpia todo el caché de búsquedas
        /// </summary>
        [HttpPost("admin/clear-search-cache")]
        public async Task<IActionResult> ClearSearchCache()
        {
            try
            {
                _logger.LogInformation("🗑️ Limpiando caché de búsquedas...");

                // Intentar limpiar cachés comunes
                var languages = new[] { "en", "es" };
                var cleared = 0;

                foreach (var lang in languages)
                {
                    // Limpiar todas las páginas hasta 20
                    for (int page = 1; page <= 20; page++)
                    {
                        for (int size = 1; size <= 100; size += 20)
                        {
                            // Sin rating
                            var keyNoRating = $"products:search:q=tv:page={page}:size={size}:sort=0:0:cat=all:brands=all:price=0-max:stock=all:featured=all:discount=all:rating=all_lang={lang}";
                            await _cacheService.RemoveAsync(keyNoRating);

                            // Con rating 1-5
                            for (int rating = 1; rating <= 5; rating++)
                            {
                                var keyWithRating = $"products:search:q=tv:page={page}:size={size}:sort=0:0:cat=all:brands=all:price=0-max:stock=all:featured=all:discount=all:rating={rating}_lang={lang}";
                                await _cacheService.RemoveAsync(keyWithRating);
                                cleared++;
                            }
                        }
                    }
                }

                _logger.LogInformation($"✅ Caché limpiado. Intentos de limpieza: {cleared}");
                return Ok(new { message = $"Cache cleared. Attempts: {cleared}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing search cache");
                return StatusCode(500, new { message = "Error clearing cache", error = ex.Message });
            }
        }
    }
}
