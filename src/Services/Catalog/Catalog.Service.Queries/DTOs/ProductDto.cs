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
    }
}
