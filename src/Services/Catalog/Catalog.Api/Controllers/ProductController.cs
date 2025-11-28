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
            keyBuilder.Append($"discount={request.HasDiscount?.ToString() ?? "all"}");

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
    }
}
