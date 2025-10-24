namespace Catalog.Domain
{
    /// <summary>
    /// Tabla de unión muchos a muchos entre Products y Categories
    /// </summary>
    public class ProductCategory
    {
        /// <summary>
        /// ID del producto
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// ID de la categoría
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Indica si esta es la categoría principal del producto
        /// Solo puede haber una categoría principal por producto
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Navegación al producto
        /// </summary>
        public Product Product { get; set; }

        /// <summary>
        /// Navegación a la categoría
        /// </summary>
        public Category Category { get; set; }
    }
}
