using Api.Gateway.Models;
using Common.Caching;
using Api.Gateway.Models.Catalog.DTOs;
using Api.Gateway.Models.Home.DTOs;
using Api.Gateway.Proxies;
using Common.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Gateway.WebClient.Controllers
{
    /// <summary>
    /// Controlador Gateway para la página Home.
    /// Provee endpoint agregador y endpoints individuales con cache inteligente.
    /// </summary>
    [ApiController]
    [Route("home")]
    public class HomeController : ControllerBase
    {
        private readonly IHomeProxy _homeProxy;
        private readonly ICacheService _cacheService;
        private readonly ILogger<HomeController> _logger;
        private readonly ILanguageAwareCacheKeyProvider _cacheKeyProvider;

        public HomeController(
            IHomeProxy homeProxy,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings,
            ILogger<HomeController> logger,
            ILanguageAwareCacheKeyProvider cacheKeyProvider)
        {
            _ = cacheSettings; // Settings accessed via cacheKeyProvider
            _homeProxy = homeProxy;
            _cacheService = cacheService;
            _logger = logger;
            _cacheKeyProvider = cacheKeyProvider;
        }

        #region Endpoint Agregador

        /// <summary>
        /// ENDPOINT AGREGADOR: Obtiene todos los datos para la página Home en una sola llamada.
        /// Usar para carga inicial de la página.
        /// </summary>
        /// <remarks>
        /// Retorna: Banners, Featured, Deals, Bestsellers, NewArrivals, TopRated, Categories
        /// 
        /// Cache: 5 minutos (Gateway) + 5 minutos (Catalog API)
        /// 
        /// Ejemplo de uso:
        /// 
        ///     GET /home
        ///     GET /home?productsPerSection=10
        /// </remarks>
        /// <param name="productsPerSection">Cantidad de productos por sección. Default: 8</param>
        /// <response code="200">Retorna los datos completos de la página Home</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(HomePageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HomePageResponse>> GetHomePageData([FromQuery] int productsPerSection = 8)
        {
            try
            {
                var baseCacheKey = $"gateway:home:page:products:{productsPerSection}";
                var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

                // Intentar obtener del cache
                var cached = await _cacheService.GetAsync<HomePageResponse>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Home page data retrieved from gateway cache: {CacheKey}", cacheKey);
                    cached.Metadata.FromCache = true;
                    return Ok(cached);
                }

                // Llamar al Catalog API
                var response = await _homeProxy.GetHomePageDataAsync(productsPerSection);

                // Guardar en cache del Gateway (5 minutos)
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
                _logger.LogInformation("Home page data cached at gateway: {CacheKey}", cacheKey);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching home page data");
                return StatusCode(500, new { error = "Error loading home page data", message = ex.Message });
            }
        }

        #endregion

        #region Endpoints Individuales

        /// <summary>
        /// Obtiene los banners activos para el hero section.
        /// </summary>
        /// <param name="position">Posición del banner (hero, sidebar, footer). Default: hero</param>
        /// <response code="200">Retorna la lista de banners activos</response>
        [HttpGet("banners")]
        [ProducesResponseType(typeof(List<BannerDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BannerDto>>> GetBanners([FromQuery] string position = "hero")
        {
            try
            {
                var baseCacheKey = $"gateway:home:banners:{position}";
                var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

                var cached = await _cacheService.GetAsync<List<BannerDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Banners retrieved from gateway cache");
                    return Ok(cached);
                }

                var banners = await _homeProxy.GetBannersAsync(position);
                await _cacheService.SetAsync(cacheKey, banners, TimeSpan.FromMinutes(10));

                return Ok(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching banners");
                return StatusCode(500, new { error = "Error loading banners" });
            }
        }

        /// <summary>
        /// Obtiene productos destacados (IsFeatured = true).
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos. Default: 8</param>
        /// <response code="200">Retorna la lista de productos destacados</response>
        [HttpGet("featured")]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductDto>>> GetFeaturedProducts([FromQuery] int limit = 8)
        {
            try
            {
                var baseCacheKey = $"gateway:home:featured:{limit}";
                var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

                var cached = await _cacheService.GetAsync<List<ProductDto>>(cacheKey);
                if (cached != null)
                {
                    return Ok(cached);
                }

                var products = await _homeProxy.GetFeaturedProductsAsync(limit);
                await _cacheService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(5));

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching featured products");
                return StatusCode(500, new { error = "Error loading featured products" });
            }
        }

        /// <summary>
        /// Obtiene ofertas del día (productos con descuento).
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos. Default: 8</param>
        /// <response code="200">Retorna la lista de productos en oferta</response>
        [HttpGet("deals")]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductDto>>> GetDeals([FromQuery] int limit = 8)
        {
            try
            {
                var baseCacheKey = $"gateway:home:deals:{limit}";
                var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

                var cached = await _cacheService.GetAsync<List<ProductDto>>(cacheKey);
                if (cached != null)
                {
                    return Ok(cached);
                }

                var products = await _homeProxy.GetDealsAsync(limit);
                // Cache más corto para ofertas (1 minuto)
                await _cacheService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(1));

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching deals");
                return StatusCode(500, new { error = "Error loading deals" });
            }
        }

        /// <summary>
        /// Obtiene productos más vendidos (ordenados por TotalSold).
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos. Default: 8</param>
        /// <response code="200">Retorna la lista de productos más vendidos</response>
        [HttpGet("bestsellers")]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductDto>>> GetBestSellers([FromQuery] int limit = 8)
        {
            try
            {
                var baseCacheKey = $"gateway:home:bestsellers:{limit}";
                var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

                var cached = await _cacheService.GetAsync<List<ProductDto>>(cacheKey);
                if (cached != null)
                {
                    return Ok(cached);
                }

                var products = await _homeProxy.GetBestSellersAsync(limit);
                await _cacheService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(5));

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bestsellers");
                return StatusCode(500, new { error = "Error loading bestsellers" });
            }
        }

        /// <summary>
        /// Obtiene novedades (productos recientes).
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos. Default: 8</param>
        /// <response code="200">Retorna la lista de productos nuevos</response>
        [HttpGet("new-arrivals")]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductDto>>> GetNewArrivals([FromQuery] int limit = 8)
        {
            try
            {
                var baseCacheKey = $"gateway:home:new-arrivals:{limit}";
                var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

                var cached = await _cacheService.GetAsync<List<ProductDto>>(cacheKey);
                if (cached != null)
                {
                    return Ok(cached);
                }

                var products = await _homeProxy.GetNewArrivalsAsync(limit);
                await _cacheService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(5));

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching new arrivals");
                return StatusCode(500, new { error = "Error loading new arrivals" });
            }
        }

        /// <summary>
        /// Obtiene productos mejor valorados.
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos. Default: 8</param>
        /// <param name="minRating">Rating mínimo. Default: 4.0</param>
        /// <response code="200">Retorna la lista de productos mejor valorados</response>
        [HttpGet("top-rated")]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductDto>>> GetTopRated(
            [FromQuery] int limit = 8,
            [FromQuery] decimal minRating = 4)
        {
            try
            {
                var baseCacheKey = $"gateway:home:top-rated:{limit}:{minRating}";
                var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

                var cached = await _cacheService.GetAsync<List<ProductDto>>(cacheKey);
                if (cached != null)
                {
                    return Ok(cached);
                }

                var products = await _homeProxy.GetTopRatedAsync(limit, minRating);
                await _cacheService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(5));

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top rated products");
                return StatusCode(500, new { error = "Error loading top rated products" });
            }
        }

        /// <summary>
        /// Obtiene categorías destacadas para mostrar en Home.
        /// </summary>
        /// <param name="limit">Cantidad máxima de categorías. Default: 8</param>
        /// <response code="200">Retorna la lista de categorías destacadas</response>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CategoryDto>>> GetFeaturedCategories([FromQuery] int limit = 8)
        {
            try
            {
                var baseCacheKey = $"gateway:home:categories:{limit}";
                var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

                var cached = await _cacheService.GetAsync<List<CategoryDto>>(cacheKey);
                if (cached != null)
                {
                    return Ok(cached);
                }

                var categories = await _homeProxy.GetFeaturedCategoriesAsync(limit);
                await _cacheService.SetAsync(cacheKey, categories, TimeSpan.FromMinutes(10));

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching featured categories");
                return StatusCode(500, new { error = "Error loading featured categories" });
            }
        }

        #endregion
    }
}
