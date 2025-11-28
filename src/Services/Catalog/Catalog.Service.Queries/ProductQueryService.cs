using Catalog.Common;
using Catalog.Domain;
using Catalog.Persistence.Database;
using Catalog.Service.Queries.DTOs;
using Catalog.Service.Queries.Extensions;
using Microsoft.EntityFrameworkCore;
using Service.Common.Collection;
using Service.Common.Mapping;
using Service.Common.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Service.Queries
{
    public interface IProductQueryService
    {
        Task<DataCollection<ProductDto>> GetAllAsync(int page, int take, IEnumerable<int> products = null);
        Task<ProductDto> GetAsync(int id);

        /// <summary>
        /// Busca productos con filtros avanzados
        /// </summary>
        Task<ProductSearchResponse> SearchAsync(ProductSearchRequest request);
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

        /// <summary>
        /// Busca productos con filtros avanzados, ordenamiento y paginación
        /// </summary>
        public async Task<ProductSearchResponse> SearchAsync(ProductSearchRequest request)
        {
            // Iniciar query base
            var query = _context.Products
                .Include(p => p.Stock)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .AsQueryable();

            // Aplicar filtros
            query = ApplySearchFilters(query, request);

            // Contar total antes de paginar
            var total = await query.CountAsync();

            // Calcular metadata de agregación (antes de ordenar/paginar)
            var metadata = await CalculateSearchMetadata(query, request);

            // Aplicar ordenamiento
            query = ApplySorting(query, request);

            // Aplicar paginación
            var skip = (request.Page - 1) * request.PageSize;
            var products = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            // Convertir a DTOs localizados
            var localizedDtos = products.ToLocalizedDtos(_languageContext).ToList();

            // Calcular número de páginas
            var pages = (int)Math.Ceiling((double)total / request.PageSize);

            return new ProductSearchResponse
            {
                Items = localizedDtos,
                Total = total,
                Page = request.Page,
                Pages = pages,
                Metadata = metadata
            };
        }

        /// <summary>
        /// Aplica todos los filtros de búsqueda a la query
        /// </summary>
        private IQueryable<Product> ApplySearchFilters(
            IQueryable<Product> query,
            ProductSearchRequest request)
        {
            // Filtro por texto de búsqueda
            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var searchTerm = request.Query.ToLower().Trim();
                query = query.Where(p =>
                    p.NameSpanish.ToLower().Contains(searchTerm) ||
                    p.NameEnglish.ToLower().Contains(searchTerm) ||
                    p.DescriptionSpanish.ToLower().Contains(searchTerm) ||
                    p.DescriptionEnglish.ToLower().Contains(searchTerm) ||
                    p.SKU.ToLower().Contains(searchTerm) ||
                    p.Brand.ToLower().Contains(searchTerm)
                );
            }

            // Filtro por categoría
            if (request.CategoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductCategories.Any(pc => pc.CategoryId == request.CategoryId.Value)
                );
            }

            // Filtro por marcas
            if (!string.IsNullOrWhiteSpace(request.BrandIds))
            {
                var brands = request.BrandIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(b => b.Trim())
                    .ToList();

                if (brands.Any())
                {
                    query = query.Where(p => brands.Contains(p.Brand));
                }
            }

            // Filtro por rango de precio
            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= request.MaxPrice.Value);
            }

            // Filtro por stock
            if (request.InStock.HasValue)
            {
                if (request.InStock.Value)
                {
                    query = query.Where(p => p.Stock != null && p.Stock.Stock > 0);
                }
                else
                {
                    query = query.Where(p => p.Stock == null || p.Stock.Stock == 0);
                }
            }

            // Filtro por productos destacados
            if (request.IsFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);
            }

            // Filtro por productos con descuento
            if (request.HasDiscount.HasValue)
            {
                if (request.HasDiscount.Value)
                {
                    query = query.Where(p => p.DiscountPercentage > 0);
                }
                else
                {
                    query = query.Where(p => p.DiscountPercentage == 0);
                }
            }

            // Solo productos activos
            query = query.Where(p => p.IsActive);

            return query;
        }

        /// <summary>
        /// Aplica el ordenamiento especificado
        /// </summary>
        private IQueryable<Product> ApplySorting(
            IQueryable<Product> query,
            ProductSearchRequest request)
        {
            // Determinar campo de ordenamiento según idioma
            var isSpanish = _languageContext.CurrentLanguage == "es";

            var sortedQuery = request.SortBy switch
            {
                ProductSortField.Name => request.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(p => isSpanish ? p.NameSpanish : p.NameEnglish)
                    : query.OrderByDescending(p => isSpanish ? p.NameSpanish : p.NameEnglish),

                ProductSortField.Price => request.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(p => p.Price)
                    : query.OrderByDescending(p => p.Price),

                ProductSortField.Newest => request.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt),

                ProductSortField.Discount => request.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(p => p.DiscountPercentage)
                    : query.OrderByDescending(p => p.DiscountPercentage),

                // Relevance: ordenar por coincidencia de búsqueda
                ProductSortField.Relevance => !string.IsNullOrWhiteSpace(request.Query)
                    ? query.OrderByDescending(p =>
                        (p.NameSpanish.ToLower().Contains(request.Query.ToLower()) ? 3 : 0) +
                        (p.NameEnglish.ToLower().Contains(request.Query.ToLower()) ? 3 : 0) +
                        (p.Brand.ToLower().Contains(request.Query.ToLower()) ? 2 : 0) +
                        (p.SKU.ToLower().Contains(request.Query.ToLower()) ? 2 : 0) +
                        (p.DescriptionSpanish.ToLower().Contains(request.Query.ToLower()) ? 1 : 0) +
                        (p.DescriptionEnglish.ToLower().Contains(request.Query.ToLower()) ? 1 : 0)
                    )
                    : query.OrderByDescending(p => p.IsFeatured)
                           .ThenByDescending(p => p.CreatedAt),

                // Bestseller y Rating (para implementación futura)
                ProductSortField.Bestseller => query.OrderByDescending(p => p.IsFeatured)
                                                     .ThenByDescending(p => p.CreatedAt),

                ProductSortField.Rating => query.OrderByDescending(p => p.IsFeatured)
                                                 .ThenByDescending(p => p.CreatedAt),

                _ => query.OrderBy(p => isSpanish ? p.NameSpanish : p.NameEnglish)
            };

            return sortedQuery;
        }

        /// <summary>
        /// Calcula metadata de la búsqueda (agregaciones)
        /// </summary>
        private async Task<SearchMetadata> CalculateSearchMetadata(
            IQueryable<Product> query,
            ProductSearchRequest request)
        {
            var metadata = new SearchMetadata
            {
                Query = request.Query,
                AppliedFilters = new AppliedFilters
                {
                    MinPrice = request.MinPrice,
                    MaxPrice = request.MaxPrice,
                    InStock = request.InStock,
                    IsFeatured = request.IsFeatured,
                    HasDiscount = request.HasDiscount,
                    CategoryId = request.CategoryId,
                    Brands = string.IsNullOrWhiteSpace(request.BrandIds)
                        ? new List<string>()
                        : request.BrandIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    SortBy = request.SortBy.ToString(),
                    SortOrder = request.SortOrder.ToString()
                }
            };

            // Calcular marcas disponibles con conteo
            metadata.AvailableBrands = await query
                .GroupBy(p => p.Brand)
                .Select(g => new BrandCount
                {
                    Brand = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(b => b.Count)
                .ToListAsync();

            // Calcular rango de precios
            if (await query.AnyAsync())
            {
                metadata.PriceRange = new PriceRange
                {
                    Min = await query.MinAsync(p => p.Price),
                    Max = await query.MaxAsync(p => p.Price)
                };
            }

            // Calcular distribución por categorías
            var isSpanish = _languageContext.CurrentLanguage == "es";
            metadata.CategoryDistribution = await query
                .SelectMany(p => p.ProductCategories)
                .GroupBy(pc => pc.CategoryId)
                .Select(g => new CategoryCount
                {
                    CategoryId = g.Key,
                    Name = isSpanish ? g.First().Category.NameSpanish : g.First().Category.NameEnglish,
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Count)
                .ToListAsync();

            // TODO: Implementar sugerencias "Did you mean" y búsquedas relacionadas
            metadata.DidYouMean = null;
            metadata.RelatedSearches = new List<string>();

            return metadata;
        }
    }
}
