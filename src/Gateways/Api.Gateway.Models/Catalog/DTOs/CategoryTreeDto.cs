using System.Collections.Generic;

namespace Api.Gateway.Models.Catalog.DTOs
{
    /// <summary>
    /// DTO para árbol de categorías simplificado (solo activas)
    /// </summary>
    public class CategoryTreeDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryTreeDto> SubCategories { get; set; } = new List<CategoryTreeDto>();
    }

    /// <summary>
    /// DTO para breadcrumbs de navegación
    /// </summary>
    public class CategoryBreadcrumbDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
    }

    /// <summary>
    /// DTO completo de categoría con subcategorías y breadcrumbs
    /// </summary>
    public class CategoryDetailDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new List<CategoryDto>();
        public int Level { get; set; }
        public List<CategoryBreadcrumbDto> Breadcrumbs { get; set; } = new List<CategoryBreadcrumbDto>();
    }
}
