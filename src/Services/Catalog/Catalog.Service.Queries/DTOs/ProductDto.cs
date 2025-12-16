using System;
using System.Collections.Generic;

namespace Catalog.Service.Queries.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }

        // Localized fields (based on Accept-Language header)
        // These fields are populated from NameSpanish/NameEnglish in the database
        public string Name { get; set; }
        public string Description { get; set; }

        // Identificación
        public string SKU { get; set; }
        public int? BrandId { get; set; }
        public string Brand { get; set; }
        public string Slug { get; set; }

        // Pricing
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxRate { get; set; }

        // Calculated Properties
        public decimal FinalPrice { get; set; }
        public bool HasDiscount { get; set; }
        public decimal PriceWithTax { get; set; }

        // Media
        public string Images { get; set; }
        public List<string> ImageUrls { get; set; }
        public string PrimaryImageUrl { get; set; }

        // SEO
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }

        // Flags
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Stock
        public ProductInStockDto Stock { get; set; }

        // Rating
        public decimal? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        public int? Rating5Star { get; set; }
        public int? Rating4Star { get; set; }
        public int? Rating3Star { get; set; }
        public int? Rating2Star { get; set; }
        public int? Rating1Star { get; set; }

        // Categories
        public List<CategoryDto> Categories { get; set; }
        public CategoryDto PrimaryCategory { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryId { get; set; }

        // Localized fields (based on Accept-Language header)
        // These fields are populated from NameSpanish/NameEnglish in the database
        public string Name { get; set; }
        public string Description { get; set; }

        public string Slug { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }

        // Home page fields
        public string ImageUrl { get; set; }
        public bool IsFeatured { get; set; }

        // Propiedades adicionales para navegación y árbol de categorías
        public int ProductCount { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new List<CategoryDto>();
        public int Level { get; set; }
        public List<CategoryBreadcrumbDto> Breadcrumbs { get; set; } = new List<CategoryBreadcrumbDto>();
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
}
