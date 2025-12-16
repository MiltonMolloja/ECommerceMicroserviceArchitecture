using System.Collections.Generic;

namespace Api.Gateway.Models.Catalog.DTOs
{
    public class PriceFacetDto
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public List<PriceRangeDto> Ranges { get; set; } = new List<PriceRangeDto>();
    }

    public class PriceRangeDto
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public int Count { get; set; }
        public string Label { get; set; }
    }
}
