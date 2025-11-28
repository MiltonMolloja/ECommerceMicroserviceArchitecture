using System.ComponentModel.DataAnnotations;

namespace Catalog.Service.Queries.DTOs
{
    /// <summary>
    /// Parámetros de búsqueda para productos
    /// </summary>
    public class ProductSearchRequest
    {
        /// <summary>
        /// Término de búsqueda (nombre, descripción, SKU, marca)
        /// </summary>
        [MaxLength(200)]
        public string Query { get; set; }

        /// <summary>
        /// Número de página (mínimo: 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Cantidad de items por página (1-100)
        /// </summary>
        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize { get; set; } = 24;

        /// <summary>
        /// Campo por el cual ordenar los resultados
        /// </summary>
        public ProductSortField SortBy { get; set; } = ProductSortField.Relevance;

        /// <summary>
        /// Dirección del ordenamiento (asc/desc)
        /// </summary>
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

        /// <summary>
        /// ID de categoría para filtrar
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Lista de marcas separadas por coma (ej: "HP,Dell,Lenovo")
        /// </summary>
        public string BrandIds { get; set; }

        /// <summary>
        /// Precio mínimo
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "MinPrice cannot be negative")]
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Precio máximo
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "MaxPrice cannot be negative")]
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Solo productos en stock
        /// </summary>
        public bool? InStock { get; set; }

        /// <summary>
        /// Solo productos destacados
        /// </summary>
        public bool? IsFeatured { get; set; }

        /// <summary>
        /// Solo productos con descuento
        /// </summary>
        public bool? HasDiscount { get; set; }

        /// <summary>
        /// Rating mínimo (para implementación futura)
        /// </summary>
        [Range(0, 5, ErrorMessage = "MinRating must be between 0 and 5")]
        public decimal? MinRating { get; set; }
    }

    /// <summary>
    /// Campos disponibles para ordenamiento
    /// </summary>
    public enum ProductSortField
    {
        Relevance,
        Name,
        Price,
        Newest,
        Bestseller,
        Rating,
        Discount
    }

    /// <summary>
    /// Dirección de ordenamiento
    /// </summary>
    public enum SortOrder
    {
        Ascending,
        Descending
    }
}
