using Catalog.Common;
using Catalog.Service.Queries;
using Catalog.Service.Queries.DTOs;
using Common.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Common.Collection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.Api.Controllers
{
    /// <summary>
    /// API para gestión y navegación de categorías de productos
    /// </summary>
    [ApiController]
    [Route("v1/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryQueryService _categoryQueryService;
        private readonly ILogger<CategoryController> _logger;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;
        private readonly ILanguageAwareCacheKeyProvider _cacheKeyProvider;

        public CategoryController(
            ILogger<CategoryController> logger,
            ICategoryQueryService categoryQueryService,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings,
            ILanguageAwareCacheKeyProvider cacheKeyProvider)
        {
            _logger = logger;
            _categoryQueryService = categoryQueryService;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings.Value;
            _cacheKeyProvider = cacheKeyProvider;
        }

        /// <summary>
        /// Obtiene todas las categorías con paginación
        /// </summary>
        /// <param name="page">Número de página (por defecto 1)</param>
        /// <param name="take">Elementos por página (por defecto 10)</param>
        /// <returns>Colección paginada de categorías</returns>
        [HttpGet]
        [ProducesResponseType(typeof(DataCollection<CategoryDto>), 200)]
        public async Task<ActionResult<DataCollection<CategoryDto>>> GetAll(int page = 1, int take = 10)
        {
            var baseCacheKey = $"categories:all:page:{page}:take:{take}";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            var cachedCategories = await _cacheService.GetAsync<DataCollection<CategoryDto>>(cacheKey);
            if (cachedCategories != null)
            {
                _logger.LogInformation($"Categories retrieved from cache: {cacheKey}");
                return Ok(cachedCategories);
            }

            var result = await _categoryQueryService.GetAllAsync(page, take);

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Categories cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return Ok(result);
        }

        /// <summary>
        /// Obtiene una categoría por ID con sus subcategorías
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <returns>Detalles de la categoría</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CategoryDto>> Get(int id)
        {
            var baseCacheKey = $"categories:id:{id}";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            var cachedCategory = await _cacheService.GetAsync<CategoryDto>(cacheKey);
            if (cachedCategory != null)
            {
                _logger.LogInformation($"Category retrieved from cache: {cacheKey}");
                return Ok(cachedCategory);
            }

            var category = await _categoryQueryService.GetAsync(id);
            if (category == null)
            {
                _logger.LogWarning($"Category with ID {id} not found");
                return NotFound(new { message = $"Category with ID {id} not found" });
            }

            await _cacheService.SetAsync(cacheKey, category, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Category cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return Ok(category);
        }

        /// <summary>
        /// Obtiene una categoría por Slug (URL amigable)
        /// </summary>
        /// <param name="slug">Slug de la categoría (ej: "electronica", "computadoras")</param>
        /// <returns>Detalles de la categoría</returns>
        [HttpGet("by-slug/{slug}")]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CategoryDto>> GetBySlug(string slug)
        {
            var baseCacheKey = $"categories:slug:{slug}";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            var cachedCategory = await _cacheService.GetAsync<CategoryDto>(cacheKey);
            if (cachedCategory != null)
            {
                _logger.LogInformation($"Category retrieved from cache: {cacheKey}");
                return Ok(cachedCategory);
            }

            var category = await _categoryQueryService.GetBySlugAsync(slug);
            if (category == null)
            {
                _logger.LogWarning($"Category with slug '{slug}' not found");
                return NotFound(new { message = $"Category with slug '{slug}' not found" });
            }

            await _cacheService.SetAsync(cacheKey, category, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Category cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return Ok(category);
        }

        /// <summary>
        /// Obtiene el árbol completo de categorías activas para navegación
        /// Optimizado para menús y filtros de categorías
        /// </summary>
        /// <returns>Árbol jerárquico de categorías</returns>
        [HttpGet("tree")]
        [ProducesResponseType(typeof(List<CategoryTreeDto>), 200)]
        public async Task<ActionResult<List<CategoryTreeDto>>> GetTree()
        {
            var baseCacheKey = "categories:tree";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            var cachedTree = await _cacheService.GetAsync<List<CategoryTreeDto>>(cacheKey);
            if (cachedTree != null)
            {
                _logger.LogInformation($"Category tree retrieved from cache: {cacheKey}");
                return Ok(cachedTree);
            }

            var tree = await _categoryQueryService.GetCategoryTreeAsync();

            // Cache por más tiempo ya que el árbol cambia raramente
            await _cacheService.SetAsync(cacheKey, tree, TimeSpan.FromHours(1));
            _logger.LogInformation($"Category tree cached: {cacheKey} for 1 hour");

            return Ok(tree);
        }

        /// <summary>
        /// Obtiene solo las categorías raíz (sin padre) activas
        /// </summary>
        /// <returns>Lista de categorías raíz</returns>
        [HttpGet("root")]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        public async Task<ActionResult<List<CategoryDto>>> GetRootCategories()
        {
            var baseCacheKey = "categories:root";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            var cachedRoots = await _cacheService.GetAsync<List<CategoryDto>>(cacheKey);
            if (cachedRoots != null)
            {
                _logger.LogInformation($"Root categories retrieved from cache: {cacheKey}");
                return Ok(cachedRoots);
            }

            var roots = await _categoryQueryService.GetRootCategoriesAsync();

            await _cacheService.SetAsync(cacheKey, roots, TimeSpan.FromHours(1));
            _logger.LogInformation($"Root categories cached: {cacheKey} for 1 hour");

            return Ok(roots);
        }

        /// <summary>
        /// Obtiene las subcategorías de una categoría específica
        /// </summary>
        /// <param name="parentId">ID de la categoría padre</param>
        /// <returns>Lista de subcategorías</returns>
        [HttpGet("{parentId}/subcategories")]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        public async Task<ActionResult<List<CategoryDto>>> GetSubCategories(int parentId)
        {
            var baseCacheKey = $"categories:parent:{parentId}:subcategories";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            var cachedSubs = await _cacheService.GetAsync<List<CategoryDto>>(cacheKey);
            if (cachedSubs != null)
            {
                _logger.LogInformation($"Subcategories retrieved from cache: {cacheKey}");
                return Ok(cachedSubs);
            }

            var subcategories = await _categoryQueryService.GetSubCategoriesAsync(parentId);

            await _cacheService.SetAsync(cacheKey, subcategories, TimeSpan.FromHours(1));
            _logger.LogInformation($"Subcategories cached: {cacheKey} for 1 hour");

            return Ok(subcategories);
        }

        /// <summary>
        /// Obtiene los breadcrumbs (ruta de navegación) de una categoría por ID
        /// Ejemplo: Electrónica > Computadoras > Laptops
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <returns>Lista ordenada de breadcrumbs desde la raíz hasta la categoría</returns>
        [HttpGet("{id}/breadcrumbs")]
        [ProducesResponseType(typeof(List<CategoryBreadcrumbDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<CategoryBreadcrumbDto>>> GetBreadcrumbs(int id)
        {
            var baseCacheKey = $"categories:id:{id}:breadcrumbs";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            var cachedBreadcrumbs = await _cacheService.GetAsync<List<CategoryBreadcrumbDto>>(cacheKey);
            if (cachedBreadcrumbs != null)
            {
                _logger.LogInformation($"Breadcrumbs retrieved from cache: {cacheKey}");
                return Ok(cachedBreadcrumbs);
            }

            var breadcrumbs = await _categoryQueryService.GetBreadcrumbsAsync(id);
            if (breadcrumbs == null || breadcrumbs.Count == 0)
            {
                _logger.LogWarning($"Category with ID {id} not found for breadcrumbs");
                return NotFound(new { message = $"Category with ID {id} not found" });
            }

            await _cacheService.SetAsync(cacheKey, breadcrumbs, TimeSpan.FromHours(1));
            _logger.LogInformation($"Breadcrumbs cached: {cacheKey} for 1 hour");

            return Ok(breadcrumbs);
        }

        /// <summary>
        /// Obtiene los breadcrumbs por Slug
        /// </summary>
        /// <param name="slug">Slug de la categoría</param>
        /// <returns>Lista ordenada de breadcrumbs desde la raíz hasta la categoría</returns>
        [HttpGet("by-slug/{slug}/breadcrumbs")]
        [ProducesResponseType(typeof(List<CategoryBreadcrumbDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<CategoryBreadcrumbDto>>> GetBreadcrumbsBySlug(string slug)
        {
            var baseCacheKey = $"categories:slug:{slug}:breadcrumbs";
            var cacheKey = _cacheKeyProvider.GenerateKey(baseCacheKey);

            var cachedBreadcrumbs = await _cacheService.GetAsync<List<CategoryBreadcrumbDto>>(cacheKey);
            if (cachedBreadcrumbs != null)
            {
                _logger.LogInformation($"Breadcrumbs retrieved from cache: {cacheKey}");
                return Ok(cachedBreadcrumbs);
            }

            var breadcrumbs = await _categoryQueryService.GetBreadcrumbsBySlugAsync(slug);
            if (breadcrumbs == null || breadcrumbs.Count == 0)
            {
                _logger.LogWarning($"Category with slug '{slug}' not found for breadcrumbs");
                return NotFound(new { message = $"Category with slug '{slug}' not found" });
            }

            await _cacheService.SetAsync(cacheKey, breadcrumbs, TimeSpan.FromHours(1));
            _logger.LogInformation($"Breadcrumbs cached: {cacheKey} for 1 hour");

            return Ok(breadcrumbs);
        }
    }
}
