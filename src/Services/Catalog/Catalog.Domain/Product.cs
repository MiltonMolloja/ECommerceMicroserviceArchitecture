using System;
using System.Collections.Generic;
using System.Linq;

namespace Catalog.Domain
{
    public class Product
    {
        #region Properties - Persistidas

        public int ProductId { get; set; }

        // Multiidioma
        public string NameSpanish { get; set; }
        public string NameEnglish { get; set; }
        public string DescriptionSpanish { get; set; }
        public string DescriptionEnglish { get; set; }

        // Identificación
        public string SKU { get; set; }
        public string Brand { get; set; } // Mantener por compatibilidad
        public int? BrandId { get; set; } // Nueva relación con tabla Brands
        public string Slug { get; set; }

        // Pricing
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxRate { get; set; }

        // Media
        public string Images { get; set; }

        // SEO
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }

        // Flags
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }

        // Ventas
        /// <summary>
        /// Cantidad total vendida (para ordenamiento por bestseller)
        /// </summary>
        public int TotalSold { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navegación
        public ProductInStock Stock { get; set; }
        public Brand BrandNavigation { get; set; }
        public ProductRating ProductRating { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        public ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

        #endregion

        #region Computed Properties - NO persistidas

        /// <summary>
        /// Precio final después de aplicar descuento
        /// </summary>
        public decimal FinalPrice => DiscountPercentage > 0
            ? Price * (1 - DiscountPercentage / 100)
            : Price;

        /// <summary>
        /// Indica si el producto tiene descuento
        /// </summary>
        public bool HasDiscount => DiscountPercentage > 0;

        /// <summary>
        /// Precio con impuestos incluidos
        /// </summary>
        public decimal PriceWithTax => Price * (1 + TaxRate / 100);

        /// <summary>
        /// Array de URLs de imágenes
        /// </summary>
        public string[] ImageUrls => string.IsNullOrEmpty(Images)
            ? Array.Empty<string>()
            : Images.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();

        /// <summary>
        /// URL de la primera imagen (imagen principal)
        /// </summary>
        public string PrimaryImageUrl => ImageUrls.FirstOrDefault();

        #endregion

        #region Business Methods

        public void ApplyDiscount(decimal percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentException("Discount must be between 0 and 100");

            OriginalPrice = OriginalPrice ?? Price;
            DiscountPercentage = percentage;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveDiscount()
        {
            DiscountPercentage = 0;
            OriginalPrice = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
