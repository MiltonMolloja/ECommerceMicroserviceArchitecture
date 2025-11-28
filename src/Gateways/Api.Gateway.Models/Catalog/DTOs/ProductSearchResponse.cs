using Api.Gateway.Models;
using System.Collections.Generic;

namespace Api.Gateway.Models.Catalog.DTOs
{
    /// <summary>
    /// Respuesta de búsqueda de productos con metadata
    /// </summary>
    public class ProductSearchResponse : DataCollection<ProductDto>
    {
        /// <summary>
        /// Metadata adicional de la búsqueda
        /// </summary>
        public SearchMetadata Metadata { get; set; }
    }

    /// <summary>
    /// Metadata de la búsqueda
    /// </summary>
    public class SearchMetadata
    {
        /// <summary>
        /// Término de búsqueda utilizado
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Tiempo de ejecución de la consulta en milisegundos
        /// </summary>
        public long ExecutionTime { get; set; }

        /// <summary>
        /// Filtros aplicados en la búsqueda
        /// </summary>
        public AppliedFilters AppliedFilters { get; set; }

        /// <summary>
        /// Marcas disponibles en los resultados con conteo
        /// </summary>
        public List<BrandCount> AvailableBrands { get; set; }

        /// <summary>
        /// Rango de precios de los resultados
        /// </summary>
        public PriceRange PriceRange { get; set; }

        /// <summary>
        /// Distribución de productos por categoría
        /// </summary>
        public List<CategoryCount> CategoryDistribution { get; set; }

        /// <summary>
        /// Sugerencia de búsqueda alternativa (¿Quisiste decir...?)
        /// </summary>
        public string DidYouMean { get; set; }

        /// <summary>
        /// Búsquedas relacionadas sugeridas
        /// </summary>
        public List<string> RelatedSearches { get; set; }
    }

    public class AppliedFilters
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? HasDiscount { get; set; }
        public int? CategoryId { get; set; }
        public List<string> Brands { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
    }

    public class BrandCount
    {
        public string Brand { get; set; }
        public int Count { get; set; }
    }

    public class PriceRange
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }

    public class CategoryCount
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
