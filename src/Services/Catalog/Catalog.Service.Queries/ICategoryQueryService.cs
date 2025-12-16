using Catalog.Service.Queries.DTOs;
using Service.Common.Collection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.Service.Queries
{
    public interface ICategoryQueryService
    {
        /// <summary>
        /// Obtiene todas las categorías (con paginación opcional)
        /// </summary>
        Task<DataCollection<CategoryDto>> GetAllAsync(int page = 1, int take = 10);

        /// <summary>
        /// Obtiene una categoría por ID con sus subcategorías
        /// </summary>
        Task<CategoryDto> GetAsync(int id);

        /// <summary>
        /// Obtiene una categoría por Slug con sus subcategorías
        /// </summary>
        Task<CategoryDto> GetBySlugAsync(string slug);

        /// <summary>
        /// Obtiene el árbol completo de categorías activas (solo raíces y sus hijos)
        /// Optimizado para menús de navegación
        /// </summary>
        Task<List<CategoryTreeDto>> GetCategoryTreeAsync();

        /// <summary>
        /// Obtiene solo las categorías raíz (sin padre) activas
        /// </summary>
        Task<List<CategoryDto>> GetRootCategoriesAsync();

        /// <summary>
        /// Obtiene las subcategorías de una categoría específica
        /// </summary>
        Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId);

        /// <summary>
        /// Obtiene los breadcrumbs (ruta de navegación) de una categoría
        /// Ejemplo: Electrónica > Computadoras > Laptops
        /// </summary>
        Task<List<CategoryBreadcrumbDto>> GetBreadcrumbsAsync(int categoryId);

        /// <summary>
        /// Obtiene los breadcrumbs por slug
        /// </summary>
        Task<List<CategoryBreadcrumbDto>> GetBreadcrumbsBySlugAsync(string slug);
    }
}
