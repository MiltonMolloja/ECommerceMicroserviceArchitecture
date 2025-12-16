using System.Collections.Generic;

namespace Catalog.Service.Queries.DTOs
{
    public class AttributeFacetDto
    {
        public int AttributeId { get; set; }
        public string AttributeName { get; set; }
        public string AttributeType { get; set; } // "Select", "Number", "Boolean"
        public string Unit { get; set; }
        public List<FacetItemDto> Values { get; set; } = new List<FacetItemDto>(); // Para Select/MultiSelect
        public NumericRangeDto Range { get; set; } // Para Number
    }
}
