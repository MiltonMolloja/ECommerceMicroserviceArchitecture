using Api.Gateway.Models;
using Api.Gateway.Models.Catalog.DTOs;
using Api.Gateway.Proxies;
using Common.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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
            keyBuilder.Append($"discount={request.HasDiscount?.ToString() ?? "all"}");

            return keyBuilder.ToString();
        }
    }
}
