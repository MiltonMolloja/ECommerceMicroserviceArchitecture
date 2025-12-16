using System.Collections.Generic;

namespace Catalog.Service.Queries.DTOs
{
    public class ProductAdvancedSearchResponse
    {
        public List<ProductDto> Items { get; set; } = new List<ProductDto>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public bool HasMore { get; set; }
        public SearchMetadataDto Metadata { get; set; }
        public SearchFacetsDto Facets { get; set; }
    }

    public class SearchMetadataDto
    {
        public string Query { get; set; }
        public int ExecutionTime { get; set; }
        public SearchPerformanceMetricsDto Performance { get; set; }
        public string DidYouMean { get; set; }
        public List<string> RelatedSearches { get; set; }
    }

    public class SearchPerformanceMetricsDto
    {
        public long QueryExecutionTime { get; set; } // ms
        public long FacetCalculationTime { get; set; } // ms
        public long TotalExecutionTime { get; set; } // ms
        public int TotalFilteredResults { get; set; }
        public bool CacheHit { get; set; }
    }
}
