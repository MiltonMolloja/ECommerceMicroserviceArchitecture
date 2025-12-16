using System.Collections.Generic;

namespace Catalog.Domain
{
    /// <summary>
    /// Entidad que representa una categoría de productos.
    /// Soporta jerarquías con ParentCategoryId para subcategorías.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Identificador único de la categoría
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Nombre de la categoría en español
        /// </summary>
        public string NameSpanish { get; set; }

        /// <summary>
        /// Nombre de la categoría en inglés
        /// </summary>
        public string NameEnglish { get; set; }

        /// <summary>
        /// Descripción de la categoría en español
        /// </summary>
        public string DescriptionSpanish { get; set; }

        /// <summary>
        /// Descripción de la categoría en inglés
        /// </summary>
        public string DescriptionEnglish { get; set; }

        /// <summary>
        /// URL amigable para SEO (ej: "electronica", "ropa-hombre")
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// ID de la categoría padre (null si es categoría raíz)
        /// Permite crear jerarquías de categorías
        /// </summary>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// Indica si la categoría está activa (visible en el sitio)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Orden de visualización (menor número = mayor prioridad)
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Indica si la categoría debe aparecer en la página de inicio
        /// </summary>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// URL de la imagen representativa de la categoría (opcional)
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Navegación a la categoría padre
        /// </summary>
        public Category ParentCategory { get; set; }

        /// <summary>
        /// Navegación a las subcategorías
        /// </summary>
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        /// <summary>
        /// Navegación a la relación muchos a muchos con productos
        /// </summary>
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }
}
