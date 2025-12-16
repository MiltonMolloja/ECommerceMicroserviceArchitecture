using Catalog.Domain;
using Catalog.Service.Queries.DTOs;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Service.Queries.Services
{
    public interface IFacetService
    {
        Task<SearchFacetsDto> CalculateFacetsAsync(
            IQueryable<Product> baseQuery,
            ProductAdvancedSearchRequest request);
    }
}
