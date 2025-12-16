using System.Collections.Generic;

namespace Api.Gateway.Models.Catalog.DTOs
{
    public class SearchFacetsDto
    {
        // Facetas de marca con contadores
        public List<FacetItemDto> Brands { get; set; } = new List<FacetItemDto>();

        // Facetas de categoría con contadores
        public List<FacetItemDto> Categories { get; set; } = new List<FacetItemDto>();

        // Rangos de precio con distribución
        public PriceFacetDto PriceRanges { get; set; }

        // Facetas de rating
        public RatingFacetDto Ratings { get; set; }

        // Facetas de atributos dinámicos
        public Dictionary<string, AttributeFacetDto> Attributes { get; set; } = new Dictionary<string, AttributeFacetDto>();
    }
}
