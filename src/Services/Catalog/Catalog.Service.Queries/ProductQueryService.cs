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

            // Filtro por rating mínimo
            if (request.MinRating.HasValue)
            {
                query = query.Where(p =>
                    p.ProductRating != null &&
                    p.ProductRating.AverageRating >= request.MinRating.Value
                );
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

                // Bestseller - Ordenar por ventas reales (TotalSold)
                ProductSortField.Bestseller => query.OrderByDescending(p => p.TotalSold)
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
            // Búsqueda de texto usando LIKE (sin Full-Text Search)
            // Para habilitar Full-Text Search ver: FULLTEXT-SEARCH-SETUP.md
            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var searchTerm = request.Query.Trim().ToLower();

                query = query.Where(p =>
                    p.NameSpanish.ToLower().Contains(searchTerm) ||
                    p.NameEnglish.ToLower().Contains(searchTerm) ||
                    p.DescriptionSpanish.ToLower().Contains(searchTerm) ||
                    p.DescriptionEnglish.ToLower().Contains(searchTerm) ||
                    p.SKU.ToLower().Contains(searchTerm)
                );
            }

            // Filtro por categorías (múltiples)
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                query = query.Where(p =>
                    p.ProductCategories.Any(pc => request.CategoryIds.Contains(pc.CategoryId))
                );
            }

            // Filtro por marcas normalizadas (múltiples)
            if (request.BrandIds != null && request.BrandIds.Any())
            {
                query = query.Where(p =>
                    p.BrandId.HasValue && request.BrandIds.Contains((int)p.BrandId.Value)
                );
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

            // Filtro por rating mínimo
            if (request.MinAverageRating.HasValue)
            {
                query = query.Where(p =>
                    p.ProductRating != null &&
                    p.ProductRating.AverageRating >= request.MinAverageRating.Value
                );
            }

            // Filtro por cantidad mínima de reviews
            if (request.MinReviewCount.HasValue)
            {
                query = query.Where(p =>
                    p.ProductRating != null &&
                    p.ProductRating.TotalReviews >= request.MinReviewCount.Value
                );
            }

            // Filtro por atributos de selección
            if (request.Attributes != null && request.Attributes.Any())
            {
                foreach (var attributeFilter in request.Attributes)
                {
                    var attributeKey = attributeFilter.Key;
                    var values = attributeFilter.Value;

                    if (values != null && values.Any())
                    {
                        // Intentar parsear la clave como AttributeId (numérico) o AttributeName (string)
                        if (int.TryParse(attributeKey, out var attributeId))
                        {
                            // Filtro por AttributeId (ej: "107" -> 107)
                            query = query.Where(p =>
                                p.ProductAttributeValues.Any(pav =>
                                    pav.AttributeId == attributeId &&
                                    pav.ValueId.HasValue &&
                                    values.Contains(pav.ValueId.Value.ToString())
                                )
                            );
                        }
                        else
                        {
                            // Filtro por AttributeName (ej: "ScreenSize")
                            query = query.Where(p =>
                                p.ProductAttributeValues.Any(pav =>
                                    pav.ProductAttribute.AttributeName == attributeKey &&
                                    pav.ValueId.HasValue &&
                                    values.Contains(pav.ValueId.Value.ToString())
                                )
                            );
                        }
                    }
                }
            }

            // Filtro por rangos de atributos numéricos
            if (request.AttributeRanges != null && request.AttributeRanges.Any())
            {
                foreach (var rangeFilter in request.AttributeRanges)
                {
                    var attributeName = rangeFilter.Key;
                    var range = rangeFilter.Value;

                    query = query.Where(p =>
                        p.ProductAttributeValues.Any(pav =>
                            pav.ProductAttribute.AttributeName == attributeName &&
                            pav.NumericValue.HasValue &&
                            pav.NumericValue.Value >= range.Min &&
                            pav.NumericValue.Value <= range.Max
                        )
                    );
                }
            }

            // Filtro por stock
            if (request.InStock.HasValue && request.InStock.Value)
            {
                query = query.Where(p => p.Stock != null && p.Stock.Stock > 0);
            }

            // Filtro por productos destacados
            if (request.IsFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);
            }

            // Filtro por descuento
            if (request.HasDiscount.HasValue && request.HasDiscount.Value)
            {
                query = query.Where(p => p.DiscountPercentage > 0);
            }

            // Solo productos activos
            query = query.Where(p => p.IsActive);

            return query;
        }

        /// <summary>
        /// Aplica ordenamiento avanzado con soporte para rating
        /// </summary>
        private IQueryable<Product> ApplyAdvancedSorting(
            IQueryable<Product> query,
            ProductAdvancedSearchRequest request)
        {
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

                ProductSortField.Rating => request.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(p => p.ProductRating != null ? p.ProductRating.AverageRating : 0)
                    : query.OrderByDescending(p => p.ProductRating != null ? p.ProductRating.AverageRating : 0),

                // Relevance con Full-Text Search ranking
                ProductSortField.Relevance => !string.IsNullOrWhiteSpace(request.Query)
                    ? query.OrderByDescending(p => p.IsFeatured)
                           .ThenByDescending(p => p.ProductRating != null ? p.ProductRating.AverageRating : 0)
                    : query.OrderByDescending(p => p.IsFeatured)
                           .ThenByDescending(p => p.CreatedAt),

                ProductSortField.Bestseller => query.OrderByDescending(p => p.TotalSold)
                                                     .ThenByDescending(p => p.ProductRating != null ? p.ProductRating.AverageRating : 0),

                _ => query.OrderBy(p => isSpanish ? p.NameSpanish : p.NameEnglish)
            };

            return sortedQuery;
        }
    }
}
