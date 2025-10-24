namespace Catalog.Service.Queries.DTOs
{
    public class ProductInStockDto
    {
        public int ProductInStockId { get; set; }
        public int ProductId { get; set; }
        public int Stock { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }

        // Calculated Properties
        public bool IsLowStock { get; set; }
        public bool IsOutOfStock { get; set; }
        public bool IsOverStock { get; set; }
    }
}
