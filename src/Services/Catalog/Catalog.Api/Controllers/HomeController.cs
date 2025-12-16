using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.Common;
using Catalog.Service.Queries;
using Catalog.Service.Queries.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Catalog.Api.Controllers
{
    /// <summary>
    /// Controlador para la página Home con arquitectura híbrida.
    /// Ofrece endpoint agregador para carga inicial + endpoints individuales para actualizaciones parciales.
    /// </summary>
    [ApiController]
    [Route("v1/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IHomeQueryService _homeService;
        private readonly ILanguageContext _languageContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IHomeQueryService homeService,
            ILanguageContext languageContext,
            ILogger<HomeController> logger)
        {
            _homeService = homeService;
            _languageContext = languageContext;
            _logger = logger;
        }

        #region Endpoint Agregador (Carga Inicial)

        /// <summary>
        /// ENDPOINT AGREGADOR: Obtiene todos los datos para la página Home en una sola llamada.
        /// Usar para carga inicial de la página.
        /// </summary>
        /// <remarks>
        /// Retorna: Banners, Featured, Deals, Bestsellers, NewArrivals, TopRated, Categories
        /// 
        /// Cache: 5 minutos
        /// 
        /// Fallback: Si falla, el frontend puede usar endpoints individuales
        /// 
        /// Ejemplo de uso:
        /// 
        ///     GET /v1/home
        ///     GET /v1/home?productsPerSection=10
        ///     
        /// Headers:
        /// 
        ///     Accept-Language: es (Español)
        ///     Accept-Language: en (English)
        /// </remarks>
        /// <param name="productsPerSection">Cantidad de productos por sección. Default: 8</param>
        /// <response code="200">Retorna los datos completos de la página Home</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ResponseCache(Duration = 300, VaryByHeader = "Accept-Language", VaryByQueryKeys = new[] { "productsPerSection" })]
        [ProducesResponseType(typeof(HomePageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HomePageResponse>> GetHomePageData(
            [FromQuery] int productsPerSection = 8)
        {
            try
            {
                var language = _languageContext.CurrentLanguage;
                _logger.LogInformation("Fetching home page data for language: {Language}, productsPerSection: {ProductsPerSection}", 
                    language, productsPerSection);

                var response = await _homeService.GetHomePageDataAsync(language, productsPerSection);

                _logger.LogInformation("Home page data fetched successfully. FromCache: {FromCache}, ExecutionTime: {ExecutionTime}ms",
                    response.Metadata.FromCache, response.Metadata.QueryExecutionTimeMs);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching home page data");
                return StatusCode(500, new { error = "Error loading home page data", message = ex.Message });
            }
        }

        #endregion

        #region Endpoints Individuales (Actualizaciones Parciales)

        /// <summary>
        /// Obtiene los banners activos para el hero section.
        /// Cache más largo (10 min) porque banners cambian poco.
        /// </summary>
        /// <param name="position">Posición del banner (hero, sidebar, footer). Default: hero</param>
        /// <response code="200">Retorna la lista de banners activos</response>
        [HttpGet("banners")]
        [ResponseCache(Duration = 600, VaryByHeader = "Accept-Language", VaryByQueryKeys = new[] { "position" })]
        [ProducesResponseType(typeof(List<BannerDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BannerDto>>> GetBanners(
            [FromQuery] string position = "hero")
        {
            try
            {
                var language = _languageContext.CurrentLanguage;
                var banners = await _homeService.GetBannersAsync(position, language);
                _logger.LogDebug("Fetched {Count} banners for position: {Position}", banners.Count, position);
                return Ok(banners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching banners for position: {Position}", position);
                return StatusCode(500, new { error = "Error loading banners" });
            }
        }

        /// <summary>
        /// Obtiene productos destacados (IsFeatured = true).
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos. Default: 8</param>
        /// <response code="200">Retorna la lista de productos destacados</response>
        [HttpGet("featured")]
        [ResponseCache(Duration = 300, VaryByHeader = "Accept-Language", VaryByQueryKeys = new[] { "limit" })]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductDto>>> GetFeaturedProducts(
            [FromQuery] int limit = 8)
        {
            try
            {
                var language = _languageContext.CurrentLanguage;
                var products = await _homeService.GetFeaturedProductsAsync(limit, language);
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
        /// Cache más corto (1 min) porque ofertas cambian frecuentemente.
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos. Default: 8</param>
        /// <response code="200">Retorna la lista de productos en oferta</response>
        [HttpGet("deals")]
        [ResponseCache(Duration = 60, VaryByHeader = "Accept-Language", VaryByQueryKeys = new[] { "limit" })]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductDto>>> GetDeals(
            [FromQuery] int limit = 8)
        {
            try
            {
                var language = _languageContext.CurrentLanguage;
                var products = await _homeService.GetDealsAsync(limit, language);
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
        [ResponseCache(Duration = 300, VaryByHeader = "Accept-Language", VaryByQueryKeys = new[] { "limit" })]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductDto>>> GetBestSellers(
            [FromQuery] int limit = 8)
        {
            try
            {
                var language = _languageContext.CurrentLanguage;
                var products = await _homeService.GetBestSellersAsync(limit, language);
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
        [ResponseCache(Duration = 300, VaryByHeader = "Accept-Language", VaryByQueryKeys = new[] { "limit" })]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductDto>>> GetNewArrivals(
            [FromQuery] int limit = 8)
        {
            try
            {
                var language = _languageContext.CurrentLanguage;
                var products = await _homeService.GetNewArrivalsAsync(limit, language);
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
        [ResponseCache(Duration = 300, VaryByHeader = "Accept-Language", VaryByQueryKeys = new[] { "limit", "minRating" })]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductDto>>> GetTopRated(
            [FromQuery] int limit = 8,
            [FromQuery] decimal minRating = 4)
        {
            try
            {
                var language = _languageContext.CurrentLanguage;
                var products = await _homeService.GetTopRatedAsync(limit, minRating, language);
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
        [ResponseCache(Duration = 600, VaryByHeader = "Accept-Language", VaryByQueryKeys = new[] { "limit" })]
        [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CategoryDto>>> GetFeaturedCategories(
            [FromQuery] int limit = 8)
        {
            try
            {
                var language = _languageContext.CurrentLanguage;
                var categories = await _homeService.GetFeaturedCategoriesAsync(limit, language);
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
