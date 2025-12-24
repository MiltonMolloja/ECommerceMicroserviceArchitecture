namespace Order.Domain
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; } // Required by PostgreSQL schema
        public int ProductId { get; set; }
        public string ProductName { get; set; } // Required by PostgreSQL schema (NOT NULL)
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
