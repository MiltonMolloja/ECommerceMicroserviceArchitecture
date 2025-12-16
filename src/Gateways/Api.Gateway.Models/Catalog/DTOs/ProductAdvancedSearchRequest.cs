using System.Collections.Generic;

namespace Api.Gateway.Models.Catalog.DTOs
{
    public class ProductAdvancedSearchRequest : ProductSearchRequest
    {
        // Filtros de categorías múltiples (sobrescribe CategoryId del padre)
        public List<int> CategoryIds { get; set; } = new List<int>();

        // Filtros de marcas múltiples (sobrescribe BrandIds string del padre)
        public new List<int> BrandIds { get; set; } = new List<int>();

        // Filtros de atributos dinámicos
        public Dictionary<string, List<string>> Attributes { get; set; } = new Dictionary<string, List<string>>();
        // Ej: { "ScreenSize": ["50-59", "60-69"], "Resolution": ["4K", "8K"] }

        // Filtros de rating
        public decimal? MinAverageRating { get; set; }
        public int? MinReviewCount { get; set; }

        // Rangos de atributos numéricos
        public Dictionary<string, NumericRangeDto> AttributeRanges { get; set; } = new Dictionary<string, NumericRangeDto>();
        // Ej: { "ScreenSize": { Min: 50, Max: 70 } }

        // Filtros de disponibilidad
        public bool? IsPreOrder { get; set; }
        public bool? ShipsInternational { get; set; }

        // Filtros de descuento
        public decimal? MinDiscountPercentage { get; set; }

        // Solicitar facetas específicas
        public bool IncludeBrandFacets { get; set; } = true;
        public bool IncludePriceFacets { get; set; } = true;
        public bool IncludeAttributeFacets { get; set; } = true;
        public bool IncludeCategoryFacets { get; set; } = true;
        public bool IncludeRatingFacets { get; set; } = true;
    }
}
