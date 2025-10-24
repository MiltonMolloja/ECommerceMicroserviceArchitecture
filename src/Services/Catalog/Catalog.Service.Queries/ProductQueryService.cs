using Catalog.Common;
using Catalog.Persistence.Database;
using Catalog.Service.Queries.DTOs;
using Catalog.Service.Queries.Extensions;
using Microsoft.EntityFrameworkCore;
using Service.Common.Collection;
using Service.Common.Mapping;
using Service.Common.Paging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Service.Queries
{
    public interface IProductQueryService
    {
        Task<DataCollection<ProductDto>> GetAllAsync(int page, int take, IEnumerable<int> products = null);
        Task<ProductDto> GetAsync(int id);
    }

    public class ProductQueryService : IProductQueryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILanguageContext _languageContext;

        public ProductQueryService(
            ApplicationDbContext context,
            ILanguageContext languageContext)
        {
            _context = context;
            _languageContext = languageContext;
        }

        public async Task<DataCollection<ProductDto>> GetAllAsync(int page, int take, IEnumerable<int> products = null)
        {
            var collection = await _context.Products
                .Where(x => products == null || products.Contains(x.ProductId))
                .OrderBy(x => x.NameSpanish) // Ordenar por nombre en español por defecto
                .GetPagedAsync(page, take);

            // Map to localized DTOs directly from domain entities
            var localizedItems = collection.Items.ToLocalizedDtos(_languageContext).ToList();

            return new DataCollection<ProductDto>
            {
                Items = localizedItems,
                Total = collection.Total,
                Page = collection.Page,
                Pages = collection.Pages
            };
        }

        public async Task<ProductDto> GetAsync(int id)
        {
            var product = await _context.Products.SingleAsync(x => x.ProductId == id);

            // Map to localized DTO directly from domain entity
            return product.ToLocalizedDto(_languageContext);
        }
    }
}
