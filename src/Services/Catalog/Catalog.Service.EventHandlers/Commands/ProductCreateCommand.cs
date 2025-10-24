using MediatR;

namespace Catalog.Service.EventHandlers.Commands
{
    public class ProductCreateCommand : INotification
    {
        // Multiidioma
        public string NameSpanish { get; set; }
        public string NameEnglish { get; set; }
        public string DescriptionSpanish { get; set; }
        public string DescriptionEnglish { get; set; }

        // Identificación
        public string SKU { get; set; }
        public string Brand { get; set; }
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
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
    }
}
