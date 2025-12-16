using System.Collections.Generic;

namespace Catalog.Service.Queries.DTOs
{
    public class RatingFacetDto
    {
        public List<RatingRangeDto> Ranges { get; set; } = new List<RatingRangeDto>();
    }

    public class RatingRangeDto
    {
        public decimal MinRating { get; set; }
        public int Count { get; set; }
        public string Label { get; set; }
    }
}
