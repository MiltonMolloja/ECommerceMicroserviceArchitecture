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
    }
}
