using System.Collections.Generic;

namespace Catalog.Domain
{
    public class Brand
    {
        public int BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
