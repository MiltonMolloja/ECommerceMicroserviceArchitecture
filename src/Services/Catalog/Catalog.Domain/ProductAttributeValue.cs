namespace Catalog.Domain
{
    public class ProductAttributeValue
    {
        public int ProductId { get; set; }
        public int AttributeId { get; set; }
        public int? ValueId { get; set; } // Para Select/MultiSelect
        public string TextValue { get; set; } // Para Text
        public decimal? NumericValue { get; set; } // Para Number
        public bool? BooleanValue { get; set; } // Para Boolean

        // Navigation properties
        public Product Product { get; set; }
        public ProductAttribute ProductAttribute { get; set; }
        public AttributeValue AttributeValue { get; set; }
    }
}
