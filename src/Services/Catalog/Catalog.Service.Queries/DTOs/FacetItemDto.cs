namespace Catalog.Service.Queries.DTOs
{
    public class FacetItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public bool IsSelected { get; set; }
    }
}
