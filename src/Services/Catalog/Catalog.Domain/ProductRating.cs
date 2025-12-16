using System;

namespace Catalog.Domain
{
    public class ProductRating
    {
        public int ProductId { get; set; }
        public decimal AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public int Rating5Star { get; set; } = 0;
        public int Rating4Star { get; set; } = 0;
        public int Rating3Star { get; set; } = 0;
        public int Rating2Star { get; set; } = 0;
        public int Rating1Star { get; set; } = 0;
        public DateTime LastUpdated { get; set; }

        // Navigation property
        public Product Product { get; set; }
    }
}
