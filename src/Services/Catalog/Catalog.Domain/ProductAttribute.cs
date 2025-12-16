using System.Collections.Generic;

namespace Catalog.Domain
{
    public class ProductAttribute
    {
        public int AttributeId { get; set; }
        public string AttributeName { get; set; }
        public string AttributeNameEnglish { get; set; }
        public string AttributeType { get; set; } // 'Text', 'Number', 'Boolean', 'Select', 'MultiSelect'
        public string Unit { get; set; } // 'inches', 'GB', 'MP', 'Hz', etc.
        public bool IsFilterable { get; set; } = true;
        public bool IsSearchable { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public int? CategoryId { get; set; } // Null = global, valor = específico de categoría

        // Navigation properties
        public Category Category { get; set; }
        public ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();
        public ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();
    }
}
