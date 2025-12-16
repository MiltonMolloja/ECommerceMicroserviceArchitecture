using System.Collections.Generic;

namespace Catalog.Domain
{
    public class AttributeValue
    {
        public int ValueId { get; set; }
        public int AttributeId { get; set; }
        public string ValueText { get; set; }
        public string ValueTextEnglish { get; set; }
        public int DisplayOrder { get; set; } = 0;

        // Navigation properties
        public ProductAttribute ProductAttribute { get; set; }
        public ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();
    }
}
