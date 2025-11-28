using System.Text.Json.Serialization;

namespace Catalog.Domain
{
    public class ProductInStock
    {
        public int ProductInStockId { get; set; }
        public int ProductId { get; set; }

        public int Stock { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }

        // Navegación
        [JsonIgnore]
        public Product Product { get; set; }

        // Computed properties
        public bool IsLowStock => Stock <= MinStock && Stock > 0;
        public bool IsOutOfStock => Stock <= 0;
        public bool IsOverStock => Stock > MaxStock;
    }
}
