using Api.Gateway.Models;
using Common.Caching;
using Api.Gateway.Models.Catalog.DTOs;
using Api.Gateway.Proxies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Gateway.WebClient.Controllers
{
    [ApiController]
    [Route("catalog/v1/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryProxy _categoryProxy;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(
            ICategoryProxy categoryProxy,
            ILogger<CategoryController> logger)
        {
            _categoryProxy = categoryProxy;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el idioma del header Accept-Language
        /// </summary>
        private string GetLanguage()
        {
            var acceptLanguage = Request.Headers["Accept-Language"].ToString();
            return !string.IsNullOrEmpty(acceptLanguage) && acceptLanguage.StartsWith("en") ? "en" : "es";
        }

        /// <summary>
        /// Obtiene todas las categorías con paginación
        /// </summary>
        [HttpGet]
        public async Task<DataCollection<CategoryDetailDto>> GetAll(int page = 1, int take = 10)
        {
            var language = GetLanguage();
            return await _categoryProxy.GetAllAsync(page, take, language);
        }

        /// <summary>
        /// Obtiene una categoría por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<CategoryDetailDto> Get(int id)
        {
            var language = GetLanguage();
            return await _categoryProxy.GetAsync(id, language);
        }

        /// <summary>
        /// Obtiene una categoría por Slug
        /// </summary>
        [HttpGet("by-slug/{slug}")]
        public async Task<CategoryDetailDto> GetBySlug(string slug)
        {
            var language = GetLanguage();
            return await _categoryProxy.GetBySlugAsync(slug, language);
        }

        /// <summary>
        /// Obtiene el árbol completo de categorías activas
        /// </summary>
        [HttpGet("tree")]
        public async Task<List<CategoryTreeDto>> GetTree()
        {
            var language = GetLanguage();
            return await _categoryProxy.GetCategoryTreeAsync(language);
        }

        /// <summary>
        /// Obtiene solo las categorías raíz
        /// </summary>
        [HttpGet("root")]
        public async Task<List<CategoryDto>> GetRootCategories()
        {
            var language = GetLanguage();
            return await _categoryProxy.GetRootCategoriesAsync(language);
        }

        /// <summary>
        /// Obtiene las subcategorías de una categoría
        /// </summary>
        [HttpGet("{parentId}/subcategories")]
        public async Task<List<CategoryDto>> GetSubCategories(int parentId)
        {
            var language = GetLanguage();
            return await _categoryProxy.GetSubCategoriesAsync(parentId, language);
        }

        /// <summary>
        /// Obtiene los breadcrumbs de una categoría por ID
        /// </summary>
        [HttpGet("{id}/breadcrumbs")]
        public async Task<List<CategoryBreadcrumbDto>> GetBreadcrumbs(int id)
        {
            var language = GetLanguage();
            return await _categoryProxy.GetBreadcrumbsAsync(id, language);
        }

        /// <summary>
        /// Obtiene los breadcrumbs por slug
        /// </summary>
        [HttpGet("by-slug/{slug}/breadcrumbs")]
        public async Task<List<CategoryBreadcrumbDto>> GetBreadcrumbsBySlug(string slug)
        {
            var language = GetLanguage();
            return await _categoryProxy.GetBreadcrumbsBySlugAsync(slug, language);
        }
    }
}
