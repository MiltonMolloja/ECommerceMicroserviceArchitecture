using Catalog.Common;
using Catalog.Domain;
using Catalog.Persistence.Database;
using Catalog.Service.Queries.DTOs;
using Catalog.Service.Queries.Extensions;
using Catalog.Service.Queries.Services;
using Microsoft.EntityFrameworkCore;
using Service.Common.Collection;
using Service.Common.Mapping;
using Service.Common.Paging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        /// <summary>
        /// Busca productos con filtros avanzados, facetas y Full-Text Search
        /// </summary>
        Task<ProductAdvancedSearchResponse> SearchAdvancedAsync(ProductAdvancedSearchRequest request);
    }

    public class ProductQueryService : IProductQueryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILanguageContext _languageContext;
        private readonly IFacetService _facetService;

        public ProductQueryService(
            ApplicationDbContext context,
            ILanguageContext languageContext,
            IFacetService facetService)
        {
            _context = context;
            _languageContext = languageContext;
            _facetService = facetService;
        }

        public async Task<DataCollection<ProductDto>> GetAllAsync(int page, int take, IEnumerable<int> products = null)
        {
            var collection = await _context.Products
                .Include(p => p.Stock)
                .Include(p => p.ProductRating)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.BrandNavigation)
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
            var product = await _context.Products
                .Include(p => p.ProductRating)  // Include ratings for product detail
                .Include(p => p.Stock)          // Include stock information
                .Include(p => p.ProductCategories)  // Include categories
                    .ThenInclude(pc => pc.Category)  // Include category details
                .Include(p => p.BrandNavigation)    // Include brand information
                .SingleAsync(x => x.ProductId == id);

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
                .Include(p => p.ProductRating)
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
            query = ApplyTextSearchFilter(query, request.Query);
            query = ApplyCategoryFilter(query, request.CategoryId);
            query = ApplyBrandFilter(query, request.BrandIds);
            query = ApplyPriceRangeFilter(query, request.MinPrice, request.MaxPrice);
            query = ApplyStockFilter(query, request.InStock);
            query = ApplyFeaturedFilter(query, request.IsFeatured);
            query = ApplyDiscountFilter(query, request.HasDiscount);
            query = ApplyRatingFilter(query, request.MinRating);

            // Solo productos activos
            return query.Where(p => p.IsActive);
        }

        private static IQueryable<Product> ApplyTextSearchFilter(IQueryable<Product> query, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return query;

            var searchTerm = searchQuery.ToLower().Trim();

            // Split into separate filters to reduce conditional operators (S1067)
            return query.Where(p =>
                MatchesNameOrDescription(p, searchTerm) ||
                MatchesSkuOrBrand(p, searchTerm)
            );
        }

        private static bool MatchesNameOrDescription(Product p, string searchTerm)
        {
            return p.NameSpanish.ToLower().Contains(searchTerm) ||
                   p.NameEnglish.ToLower().Contains(searchTerm) ||
                   p.DescriptionSpanish.ToLower().Contains(searchTerm) ||
                   p.DescriptionEnglish.ToLower().Contains(searchTerm);
        }

        private static bool MatchesSkuOrBrand(Product p, string searchTerm)
        {
            return p.SKU.ToLower().Contains(searchTerm) ||
                   p.Brand.ToLower().Contains(searchTerm);
        }

        private static IQueryable<Product> ApplyCategoryFilter(IQueryable<Product> query, int? categoryId)
        {
            if (!categoryId.HasValue)
                return query;

            return query.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId.Value));
        }

        private static IQueryable<Product> ApplyBrandFilter(IQueryable<Product> query, string brandIds)
        {
            if (string.IsNullOrWhiteSpace(brandIds))
                return query;

            var brands = brandIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(b => b.Trim())
                .ToList();

            return brands.Any() ? query.Where(p => brands.Contains(p.Brand)) : query;
        }

        private static IQueryable<Product> ApplyPriceRangeFilter(IQueryable<Product> query, decimal? minPrice, decimal? maxPrice)
        {
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return query;
        }

        private static IQueryable<Product> ApplyStockFilter(IQueryable<Product> query, bool? inStock)
        {
            if (!inStock.HasValue)
                return query;

            return inStock.Value
                ? query.Where(p => p.Stock != null && p.Stock.Stock > 0)
                : query.Where(p => p.Stock == null || p.Stock.Stock == 0);
        }

        private static IQueryable<Product> ApplyFeaturedFilter(IQueryable<Product> query, bool? isFeatured)
        {
            return isFeatured.HasValue
                ? query.Where(p => p.IsFeatured == isFeatured.Value)
                : query;
        }

        private static IQueryable<Product> ApplyDiscountFilter(IQueryable<Product> query, bool? hasDiscount)
        {
            if (!hasDiscount.HasValue)
                return query;

            return hasDiscount.Value
                ? query.Where(p => p.DiscountPercentage > 0)
                : query.Where(p => p.DiscountPercentage == 0);
        }

        private static IQueryable<Product> ApplyRatingFilter(IQueryable<Product> query, decimal? minRating)
        {
            return minRating.HasValue
                ? query.Where(p => p.ProductRating != null && p.ProductRating.AverageRating >= minRating.Value)
                : query;
        }

        /// <summary>
        /// Aplica el ordenamiento especificado
        /// </summary>
        private IQueryable<Product> ApplySorting(
            IQueryable<Product> query,
            ProductSearchRequest request)
        {
            var isSpanish = _languageContext.CurrentLanguage == "es";

            return request.SortBy switch
            {
                ProductSortField.Name => ApplySortByName(query, request.SortOrder, isSpanish),
                ProductSortField.Price => ApplySortByPrice(query, request.SortOrder),
                ProductSortField.Newest => ApplySortByNewest(query, request.SortOrder),
                ProductSortField.Discount => ApplySortByDiscount(query, request.SortOrder),
                ProductSortField.Relevance => ApplySortByRelevance(query, request.Query),
                ProductSortField.Bestseller => ApplySortByBestseller(query),
                ProductSortField.Rating => ApplySortByRating(query),
                _ => ApplySortByName(query, SortOrder.Ascending, isSpanish)
            };
        }

        private static IQueryable<Product> ApplySortByName(
            IQueryable<Product> query, SortOrder sortOrder, bool isSpanish)
        {
            return sortOrder == SortOrder.Ascending
                ? query.OrderBy(p => isSpanish ? p.NameSpanish : p.NameEnglish)
                : query.OrderByDescending(p => isSpanish ? p.NameSpanish : p.NameEnglish);
        }

        private static IQueryable<Product> ApplySortByPrice(
            IQueryable<Product> query, SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Ascending
                ? query.OrderBy(p => p.Price)
                : query.OrderByDescending(p => p.Price);
        }

        private static IQueryable<Product> ApplySortByNewest(
            IQueryable<Product> query, SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Ascending
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt);
        }

        private static IQueryable<Product> ApplySortByDiscount(
            IQueryable<Product> query, SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Ascending
                ? query.OrderBy(p => p.DiscountPercentage)
                : query.OrderByDescending(p => p.DiscountPercentage);
        }

        private static IQueryable<Product> ApplySortByRelevance(
            IQueryable<Product> query, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return query.OrderByDescending(p => p.IsFeatured)
                            .ThenByDescending(p => p.CreatedAt);
            }

            var lowerQuery = searchQuery.ToLower();
            return query.OrderByDescending(p => CalculateRelevanceScore(p, lowerQuery));
        }

        private static int CalculateRelevanceScore(Product p, string lowerQuery)
        {
            var nameScore = CalculateNameRelevanceScore(p, lowerQuery);
            var otherScore = CalculateOtherFieldsRelevanceScore(p, lowerQuery);
            return nameScore + otherScore;
        }

        private static int CalculateNameRelevanceScore(Product p, string lowerQuery)
        {
            var score = 0;
            if (p.NameSpanish.ToLower().Contains(lowerQuery)) score += 3;
            if (p.NameEnglish.ToLower().Contains(lowerQuery)) score += 3;
            return score;
        }

        private static int CalculateOtherFieldsRelevanceScore(Product p, string lowerQuery)
        {
            var score = 0;
            if (p.Brand.ToLower().Contains(lowerQuery)) score += 2;
            if (p.SKU.ToLower().Contains(lowerQuery)) score += 2;
            if (p.DescriptionSpanish.ToLower().Contains(lowerQuery)) score += 1;
            if (p.DescriptionEnglish.ToLower().Contains(lowerQuery)) score += 1;
            return score;
        }

        private static IQueryable<Product> ApplySortByBestseller(IQueryable<Product> query)
        {
            return query.OrderByDescending(p => p.TotalSold)
                        .ThenByDescending(p => p.CreatedAt);
        }

        private static IQueryable<Product> ApplySortByRating(IQueryable<Product> query)
        {
            return query.OrderByDescending(p => p.IsFeatured)
                        .ThenByDescending(p => p.CreatedAt);
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

        /// <summary>
        /// Búsqueda avanzada con Full-Text Search, facetas dinámicas y filtros de atributos
        /// </summary>
        public async Task<ProductAdvancedSearchResponse> SearchAdvancedAsync(ProductAdvancedSearchRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var queryStopwatch = Stopwatch.StartNew();

            // Iniciar query base con includes necesarios
            // NOTA: NO incluimos ProductAttributeValues aquí porque causa ciclos de referencia
            // Los filtros de atributos usan subconsultas SQL (Any/Contains) que no requieren Include
            var query = _context.Products
                .Include(p => p.Stock)
                .Include(p => p.BrandNavigation)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.ProductRating)
                .AsQueryable();

            // Aplicar filtros avanzados
            query = ApplyAdvancedSearchFilters(query, request);

            queryStopwatch.Stop();
            var queryTime = queryStopwatch.ElapsedMilliseconds;

            // IMPORTANTE: No usar Task.WhenAll con DbContext compartido
            // Ejecutar operaciones secuencialmente para evitar threading issues

            // 1. Contar total de resultados primero
            var total = await query.CountAsync();

            // 2. Aplicar ordenamiento
            query = ApplyAdvancedSorting(query, request);

            // 3. Aplicar paginación y obtener productos
            var skip = (request.Page - 1) * request.PageSize;
            var products = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            // 4. Calcular facetas (usa el mismo DbContext, debe ser secuencial)
            var facetStopwatch = Stopwatch.StartNew();
            var facets = await _facetService.CalculateFacetsAsync(query, request);
            facetStopwatch.Stop();
            var facetTime = facetStopwatch.ElapsedMilliseconds;

            // Convertir a DTOs localizados
            var localizedDtos = products.ToLocalizedDtos(_languageContext).ToList();

            // Calcular metadata
            var pageCount = (int)Math.Ceiling((double)total / request.PageSize);

            stopwatch.Stop();

            return new ProductAdvancedSearchResponse
            {
                Items = localizedDtos,
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize,
                PageCount = pageCount,
                HasMore = request.Page < pageCount,
                Facets = facets,
                Metadata = new SearchMetadataDto
                {
                    Query = request.Query,
                    Performance = new SearchPerformanceMetricsDto
                    {
                        QueryExecutionTime = queryTime,
                        FacetCalculationTime = facetTime,
                        TotalExecutionTime = stopwatch.ElapsedMilliseconds,
                        TotalFilteredResults = total,
                        CacheHit = false
                    },
                    DidYouMean = null, // TODO: Implementar spell checking
                    RelatedSearches = new List<string>() // TODO: Implementar búsquedas relacionadas
                }
            };
        }

        /// <summary>
        /// Aplica filtros avanzados incluyendo Full-Text Search, atributos y ratings
        /// </summary>
        private IQueryable<Product> ApplyAdvancedSearchFilters(
            IQueryable<Product> query,
            ProductAdvancedSearchRequest request)
        {
            query = ApplyAdvancedTextSearchFilter(query, request.Query);
            query = ApplyAdvancedCategoryFilter(query, request.CategoryIds);
            query = ApplyAdvancedBrandFilter(query, request.BrandIds);
            query = ApplyAdvancedPriceRangeFilter(query, request.MinPrice, request.MaxPrice);
            query = ApplyAdvancedRatingFilter(query, request.MinAverageRating);
            query = ApplyAdvancedReviewCountFilter(query, request.MinReviewCount);
            query = ApplyAttributeFilters(query, request.Attributes);
            query = ApplyAttributeRangeFilters(query, request.AttributeRanges);
            query = ApplyAdvancedStockFilter(query, request.InStock);
            query = ApplyAdvancedFeaturedFilter(query, request.IsFeatured);
            query = ApplyAdvancedDiscountFilter(query, request.HasDiscount);

            // Solo productos activos
            return query.Where(p => p.IsActive);
        }

        private static IQueryable<Product> ApplyAdvancedTextSearchFilter(
            IQueryable<Product> query, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return query;

            var searchTerm = searchQuery.Trim().ToLower();

            // Split into separate filters to reduce conditional operators (S1067)
            return query.Where(p =>
                MatchesAdvancedNameOrDescription(p, searchTerm) ||
                p.SKU.ToLower().Contains(searchTerm)
            );
        }

        private static bool MatchesAdvancedNameOrDescription(Product p, string searchTerm)
        {
            return p.NameSpanish.ToLower().Contains(searchTerm) ||
                   p.NameEnglish.ToLower().Contains(searchTerm) ||
                   p.DescriptionSpanish.ToLower().Contains(searchTerm) ||
                   p.DescriptionEnglish.ToLower().Contains(searchTerm);
        }

        private static IQueryable<Product> ApplyAdvancedCategoryFilter(
            IQueryable<Product> query, List<int> categoryIds)
        {
            if (categoryIds == null || !categoryIds.Any())
                return query;

            return query.Where(p =>
                p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));
        }

        private static IQueryable<Product> ApplyAdvancedBrandFilter(
            IQueryable<Product> query, List<int> brandIds)
        {
            if (brandIds == null || !brandIds.Any())
                return query;

            return query.Where(p =>
                p.BrandId.HasValue && brandIds.Contains((int)p.BrandId.Value));
        }

        private static IQueryable<Product> ApplyAdvancedPriceRangeFilter(
            IQueryable<Product> query, decimal? minPrice, decimal? maxPrice)
        {
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return query;
        }

        private static IQueryable<Product> ApplyAdvancedRatingFilter(
            IQueryable<Product> query, decimal? minAverageRating)
        {
            if (!minAverageRating.HasValue)
                return query;

            return query.Where(p =>
                p.ProductRating != null &&
                p.ProductRating.AverageRating >= minAverageRating.Value);
        }

        private static IQueryable<Product> ApplyAdvancedReviewCountFilter(
            IQueryable<Product> query, int? minReviewCount)
        {
            if (!minReviewCount.HasValue)
                return query;

            return query.Where(p =>
                p.ProductRating != null &&
                p.ProductRating.TotalReviews >= minReviewCount.Value);
        }

        private static IQueryable<Product> ApplyAttributeFilters(
            IQueryable<Product> query, Dictionary<string, List<string>> attributes)
        {
            if (attributes == null || !attributes.Any())
                return query;

            foreach (var attributeFilter in attributes)
            {
                query = ApplySingleAttributeFilter(query, attributeFilter.Key, attributeFilter.Value);
            }

            return query;
        }

        private static IQueryable<Product> ApplySingleAttributeFilter(
            IQueryable<Product> query, string attributeKey, List<string> values)
        {
            if (values == null || !values.Any())
                return query;

            // Intentar parsear la clave como AttributeId (numérico) o AttributeName (string)
            if (int.TryParse(attributeKey, out var attributeId))
            {
                return ApplyAttributeFilterById(query, attributeId, values);
            }

            return ApplyAttributeFilterByName(query, attributeKey, values);
        }

        private static IQueryable<Product> ApplyAttributeFilterById(
            IQueryable<Product> query, int attributeId, List<string> values)
        {
            return query.Where(p =>
                p.ProductAttributeValues.Any(pav =>
                    pav.AttributeId == attributeId &&
                    pav.ValueId.HasValue &&
                    values.Contains(pav.ValueId.Value.ToString())));
        }

        private static IQueryable<Product> ApplyAttributeFilterByName(
            IQueryable<Product> query, string attributeName, List<string> values)
        {
            return query.Where(p =>
                p.ProductAttributeValues.Any(pav =>
                    pav.ProductAttribute.AttributeName == attributeName &&
                    pav.ValueId.HasValue &&
                    values.Contains(pav.ValueId.Value.ToString())));
        }

        private static IQueryable<Product> ApplyAttributeRangeFilters(
            IQueryable<Product> query, Dictionary<string, NumericRangeDto> attributeRanges)
        {
            if (attributeRanges == null || !attributeRanges.Any())
                return query;

            foreach (var rangeFilter in attributeRanges)
            {
                query = ApplySingleAttributeRangeFilter(query, rangeFilter.Key, rangeFilter.Value);
            }

            return query;
        }

        private static IQueryable<Product> ApplySingleAttributeRangeFilter(
            IQueryable<Product> query, string attributeName, NumericRangeDto range)
        {
            return query.Where(p =>
                p.ProductAttributeValues.Any(pav =>
                    pav.ProductAttribute.AttributeName == attributeName &&
                    pav.NumericValue.HasValue &&
                    pav.NumericValue.Value >= range.Min &&
                    pav.NumericValue.Value <= range.Max));
        }

        private static IQueryable<Product> ApplyAdvancedStockFilter(
            IQueryable<Product> query, bool? inStock)
        {
            if (!inStock.HasValue || !inStock.Value)
                return query;

            return query.Where(p => p.Stock != null && p.Stock.Stock > 0);
        }

        private static IQueryable<Product> ApplyAdvancedFeaturedFilter(
            IQueryable<Product> query, bool? isFeatured)
        {
            if (!isFeatured.HasValue)
                return query;

            return query.Where(p => p.IsFeatured == isFeatured.Value);
        }

        private static IQueryable<Product> ApplyAdvancedDiscountFilter(
            IQueryable<Product> query, bool? hasDiscount)
        {
            if (!hasDiscount.HasValue || !hasDiscount.Value)
                return query;

            return query.Where(p => p.DiscountPercentage > 0);
        }

        /// <summary>
        /// Aplica ordenamiento avanzado con soporte para rating
        /// </summary>
        private IQueryable<Product> ApplyAdvancedSorting(
            IQueryable<Product> query,
            ProductAdvancedSearchRequest request)
        {
            var isSpanish = _languageContext.CurrentLanguage == "es";

            return request.SortBy switch
            {
                ProductSortField.Name => ApplySortByName(query, request.SortOrder, isSpanish),
                ProductSortField.Price => ApplySortByPrice(query, request.SortOrder),
                ProductSortField.Newest => ApplySortByNewest(query, request.SortOrder),
                ProductSortField.Discount => ApplySortByDiscount(query, request.SortOrder),
                ProductSortField.Rating => ApplyAdvancedSortByRating(query, request.SortOrder),
                ProductSortField.Relevance => ApplyAdvancedSortByRelevance(query, request.Query),
                ProductSortField.Bestseller => ApplyAdvancedSortByBestseller(query),
                _ => ApplySortByName(query, SortOrder.Ascending, isSpanish)
            };
        }

        private static IQueryable<Product> ApplyAdvancedSortByRating(
            IQueryable<Product> query, SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Ascending
                ? query.OrderBy(p => p.ProductRating != null ? p.ProductRating.AverageRating : 0)
                : query.OrderByDescending(p => p.ProductRating != null ? p.ProductRating.AverageRating : 0);
        }

        private static IQueryable<Product> ApplyAdvancedSortByRelevance(
            IQueryable<Product> query, string searchQuery)
        {
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                return query.OrderByDescending(p => p.IsFeatured)
                            .ThenByDescending(p => p.ProductRating != null ? p.ProductRating.AverageRating : 0);
            }

            return query.OrderByDescending(p => p.IsFeatured)
                        .ThenByDescending(p => p.CreatedAt);
        }

        private static IQueryable<Product> ApplyAdvancedSortByBestseller(IQueryable<Product> query)
        {
            return query.OrderByDescending(p => p.TotalSold)
                        .ThenByDescending(p => p.ProductRating != null ? p.ProductRating.AverageRating : 0);
        }
    }
}
