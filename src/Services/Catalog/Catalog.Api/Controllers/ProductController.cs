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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateCommand notification)
        {
            try
            {
                // Crear el producto
                await _mediator.Publish(notification);

                // Invalidar caché de listado de productos para ambos idiomas
                var pageSizes = new[] { 10, 20, 50, 100 };
                var languages = new[] { "es", "en" };

                foreach (var lang in languages)
                {
                    foreach (var pageSize in pageSizes)
                    {
                        for (int page = 1; page <= 20; page++)
                        {
                            var baseCacheKey = $"products:all:page:{page}:take:{pageSize}:ids:all";
                            var cacheKeyToRemove = $"{baseCacheKey}_lang={lang}";
                            await _cacheService.RemoveAsync(cacheKeyToRemove);
                        }
                    }
                }

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
    }
}
