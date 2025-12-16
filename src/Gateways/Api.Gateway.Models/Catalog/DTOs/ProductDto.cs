using System;
using System.Collections.Generic;

namespace Api.Gateway.Models.Catalog.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }

        // Localized fields (populated based on Accept-Language header)
        // Source data comes from NameSpanish/NameEnglish in the database
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

        // Calculated Properties (from service)
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

        // Stock (from ProductInStock)
        public ProductStockDto Stock { get; set; }

        // Rating (from ProductRating)
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

    public class ProductStockDto
    {
        public int ProductInStockId { get; set; }
        public int ProductId { get; set; }
        public int Stock { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsOutOfStock { get; set; }
        public bool IsOverStock { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryId { get; set; }

        // Localized fields (populated based on Accept-Language header)
        // Source data comes from NameSpanish/NameEnglish in the database
        public string Name { get; set; }
        public string Description { get; set; }

        public string Slug { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }

        // Home page fields
        public string ImageUrl { get; set; }
        public bool IsFeatured { get; set; }
        public int ProductCount { get; set; }
    }
}
