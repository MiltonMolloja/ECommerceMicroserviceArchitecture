using Catalog.Common;
using Catalog.Domain;
using Catalog.Persistence.Database;
using Catalog.Service.Queries.DTOs;
using Microsoft.EntityFrameworkCore;
using Service.Common.Collection;
using Service.Common.Paging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Service.Queries
{
    public class CategoryQueryService : ICategoryQueryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILanguageContext _languageContext;

        public CategoryQueryService(
            ApplicationDbContext context,
            ILanguageContext languageContext)
        {
            _context = context;
            _languageContext = languageContext;
        }

        /// <summary>
        /// Obtiene todas las categorías con paginación
        /// </summary>
        public async Task<DataCollection<CategoryDto>> GetAllAsync(int page = 1, int take = 10)
        {
            var collection = await _context.Categories
                .AsNoTracking()
                .Include(c => c.ProductCategories)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryId)
                .GetPagedAsync(page, take);

            return new DataCollection<CategoryDto>
            {
                Items = collection.Items.Select(c => MapToCategoryDto(c)).ToList(),
                Total = collection.Total,
                Page = collection.Page,
                Pages = collection.Pages
            };
        }

        /// <summary>
        /// Obtiene una categoría por ID
        /// </summary>
        public async Task<CategoryDto> GetAsync(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.ProductCategories)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.ProductCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return null;

            var dto = MapToCategoryDto(category, includeSubCategories: true);
            dto.Breadcrumbs = await GetBreadcrumbsAsync(id);
            
            return dto;
        }

        /// <summary>
        /// Obtiene una categoría por Slug
        /// </summary>
        public async Task<CategoryDto> GetBySlugAsync(string slug)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.ProductCategories)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.ProductCategories)
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (category == null)
                return null;

            var dto = MapToCategoryDto(category, includeSubCategories: true);
            dto.Breadcrumbs = await GetBreadcrumbsAsync(category.CategoryId);
            
            return dto;
        }

        /// <summary>
        /// Obtiene el árbol completo de categorías activas
        /// </summary>
        public async Task<List<CategoryTreeDto>> GetCategoryTreeAsync()
        {
            // Obtener todas las categorías activas de una vez
            var allCategories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.ProductCategories)
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // Construir el árbol en memoria (más eficiente que múltiples queries)
            var rootCategories = allCategories
                .Where(c => c.ParentCategoryId == null)
                .ToList();

            var tree = new List<CategoryTreeDto>();

            foreach (var root in rootCategories)
            {
                tree.Add(BuildCategoryTree(root, allCategories));
            }

            return tree;
        }

        /// <summary>
        /// Obtiene solo las categorías raíz activas
        /// </summary>
        public async Task<List<CategoryDto>> GetRootCategoriesAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.ProductCategories)
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return categories.Select(c => MapToCategoryDto(c)).ToList();
        }

        /// <summary>
        /// Obtiene las subcategorías de una categoría
        /// </summary>
        public async Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.ProductCategories)
                .Where(c => c.ParentCategoryId == parentId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return categories.Select(c => MapToCategoryDto(c)).ToList();
        }

        /// <summary>
        /// Obtiene los breadcrumbs de una categoría
        /// </summary>
        public async Task<List<CategoryBreadcrumbDto>> GetBreadcrumbsAsync(int categoryId)
        {
            var breadcrumbs = new List<CategoryBreadcrumbDto>();
            var currentId = (int?)categoryId;

            // Navegar hacia arriba hasta llegar a la raíz
            while (currentId.HasValue)
            {
                var category = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CategoryId == currentId.Value);

                if (category == null)
                    break;

                breadcrumbs.Insert(0, new CategoryBreadcrumbDto
                {
                    CategoryId = category.CategoryId,
                    Name = GetLocalizedName(category),
                    Slug = category.Slug
                });

                currentId = category.ParentCategoryId;
            }

            return breadcrumbs;
        }

        /// <summary>
        /// Obtiene los breadcrumbs por slug
        /// </summary>
        public async Task<List<CategoryBreadcrumbDto>> GetBreadcrumbsBySlugAsync(string slug)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (category == null)
                return new List<CategoryBreadcrumbDto>();

            return await GetBreadcrumbsAsync(category.CategoryId);
        }

        #region Private Helper Methods

        /// <summary>
        /// Mapea Category a CategoryDto
        /// </summary>
        private CategoryDto MapToCategoryDto(Category category, bool includeSubCategories = false, int level = 0)
        {
            var dto = new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = GetLocalizedName(category),
                Description = GetLocalizedDescription(category),
                Slug = category.Slug,
                ParentCategoryId = category.ParentCategoryId,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder,
                ProductCount = category.ProductCategories?.Count ?? 0,
                Level = level
            };

            if (includeSubCategories && category.SubCategories?.Any() == true)
            {
                dto.SubCategories = category.SubCategories
                    .OrderBy(sc => sc.DisplayOrder)
                    .Select(sc => MapToCategoryDto(sc, includeSubCategories: true, level: level + 1))
                    .ToList();
            }

            return dto;
        }

        /// <summary>
        /// Construye el árbol de categorías recursivamente
        /// </summary>
        private CategoryTreeDto BuildCategoryTree(Category category, List<Category> allCategories)
        {
            var dto = new CategoryTreeDto
            {
                CategoryId = category.CategoryId,
                Name = GetLocalizedName(category),
                Slug = category.Slug,
                ProductCount = category.ProductCategories?.Count ?? 0
            };

            // Buscar hijos en la lista completa
            var children = allCategories
                .Where(c => c.ParentCategoryId == category.CategoryId)
                .OrderBy(c => c.DisplayOrder)
                .ToList();

            foreach (var child in children)
            {
                dto.SubCategories.Add(BuildCategoryTree(child, allCategories));
            }

            return dto;
        }

        /// <summary>
        /// Obtiene el nombre localizado según el idioma actual
        /// </summary>
        private string GetLocalizedName(Category category)
        {
            return _languageContext.IsSpanish ? category.NameSpanish : category.NameEnglish;
        }

        /// <summary>
        /// Obtiene la descripción localizada según el idioma actual
        /// </summary>
        private string GetLocalizedDescription(Category category)
        {
            return _languageContext.IsSpanish ? category.DescriptionSpanish : category.DescriptionEnglish;
        }

        #endregion
    }
}
