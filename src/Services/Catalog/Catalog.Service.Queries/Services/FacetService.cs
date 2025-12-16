using Catalog.Common;
using Catalog.Domain;
using Catalog.Persistence.Database;
using Catalog.Service.Queries.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Service.Queries.Services
{
    public class FacetService : IFacetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILanguageContext _languageContext;

        public FacetService(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILanguageContext languageContext)
        {
            _context = context;
            _cache = cache;
            _languageContext = languageContext;
        }

        public async Task<SearchFacetsDto> CalculateFacetsAsync(
            IQueryable<Product> baseQuery,
            ProductAdvancedSearchRequest request)
        {
            var facets = new SearchFacetsDto();

            // IMPORTANTE: DbContext NO es thread-safe. 
            // NO usar Task.Run() porque crea múltiples threads accediendo al mismo DbContext.
            // Ejecutar queries secuencialmente usando await.
            // Si necesitas paralelismo real, considera crear un DbContext por thread con IDbContextFactory.

            if (request.IncludeBrandFacets)
                facets.Brands = await CalculateBrandFacetsAsync(baseQuery);

            if (request.IncludeCategoryFacets)
                facets.Categories = await CalculateCategoryFacetsAsync(baseQuery);

            if (request.IncludePriceFacets)
                facets.PriceRanges = await CalculatePriceFacetsAsync(baseQuery);

            if (request.IncludeRatingFacets)
                facets.Ratings = await CalculateRatingFacetsAsync(baseQuery);

            if (request.IncludeAttributeFacets)
                facets.Attributes = await CalculateAttributeFacetsAsync(baseQuery);

            return facets;
        }

        private async Task<List<FacetItemDto>> CalculateBrandFacetsAsync(IQueryable<Product> query)
        {
            var cacheKey = $"facets:brands:{query.GetHashCode()}";

            if (_cache.TryGetValue(cacheKey, out List<FacetItemDto> cached))
                return cached;

            var facets = await query
                .Where(p => p.BrandId != null)
                .GroupBy(p => new { p.BrandId, p.BrandNavigation.Name })
                .Select(g => new FacetItemDto
                {
                    Id = g.Key.BrandId.Value,
                    Name = g.Key.Name,
                    Count = g.Count(),
                    IsSelected = false
                })
                .OrderByDescending(f => f.Count)
                .Take(20)
                .ToListAsync();

            _cache.Set(cacheKey, facets, TimeSpan.FromMinutes(5));
            return facets;
        }

        private async Task<List<FacetItemDto>> CalculateCategoryFacetsAsync(IQueryable<Product> query)
        {
            var cacheKey = $"facets:categories:{query.GetHashCode()}";

            if (_cache.TryGetValue(cacheKey, out List<FacetItemDto> cached))
            {
                return cached;
            }

            // Usar el contexto de idioma para localizar nombres
            var languageName = _languageContext.IsEnglish ? "NameEnglish" : "NameSpanish";
            
            var facets = await query
                .SelectMany(p => p.ProductCategories)
                .GroupBy(pc => new { 
                    pc.CategoryId, 
                    NameSpanish = pc.Category.NameSpanish,
                    NameEnglish = pc.Category.NameEnglish
                })
                .Select(g => new FacetItemDto
                {
                    Id = g.Key.CategoryId,
                    // Usar el nombre localizado basado en el idioma actual
                    Name = _languageContext.IsEnglish ? g.Key.NameEnglish : g.Key.NameSpanish,
                    Count = g.Count(),
                    IsSelected = false
                })
                .OrderByDescending(f => f.Count)
                .Take(15)
                .ToListAsync();

            _cache.Set(cacheKey, facets, TimeSpan.FromMinutes(5));
            return facets;
        }

        private async Task<PriceFacetDto> CalculatePriceFacetsAsync(IQueryable<Product> query)
        {
            var prices = await query.Select(p => p.Price).ToListAsync();

            if (!prices.Any())
                return new PriceFacetDto { Ranges = new List<PriceRangeDto>() };

            var min = prices.Min();
            var max = prices.Max();
            var range = max - min;

            // Generar rangos dinámicos basados en distribución
            var ranges = GeneratePriceRanges(min, max, range, prices);

            return new PriceFacetDto
            {
                Min = min,
                Max = max,
                Ranges = ranges
            };
        }

        private List<PriceRangeDto> GeneratePriceRanges(decimal min, decimal max, decimal range, List<decimal> prices)
        {
            var rangeCount = 4;
            var step = range / rangeCount;
            var ranges = new List<PriceRangeDto>();

            for (int i = 0; i < rangeCount; i++)
            {
                var rangeMin = min + (step * i);
                var rangeMax = i == rangeCount - 1 ? max : min + (step * (i + 1));
                var count = prices.Count(p => p >= rangeMin && p < rangeMax);

                if (count > 0)
                {
                    ranges.Add(new PriceRangeDto
                    {
                        Min = Math.Round(rangeMin, 0),
                        Max = Math.Round(rangeMax, 0),
                        Count = count,
                        Label = FormatPriceRangeLabel(rangeMin, rangeMax, i == rangeCount - 1)
                    });
                }
            }

            return ranges;
        }

        private string FormatPriceRangeLabel(decimal min, decimal max, bool isLast)
        {
            if (min == 0)
                return $"Hasta ${max:N0}";
            if (isLast)
                return $"${min:N0} y Más";
            return $"${min:N0} a ${max:N0}";
        }

        private async Task<RatingFacetDto> CalculateRatingFacetsAsync(IQueryable<Product> query)
        {
            var productsWithRatings = query
                .Where(p => p.ProductRating != null && p.ProductRating.TotalReviews > 0);

            var ranges = new List<RatingRangeDto>
            {
                new RatingRangeDto
                {
                    MinRating = 4.0m,
                    Count = await productsWithRatings.CountAsync(p => p.ProductRating.AverageRating >= 4.0m),
                    Label = "⭐⭐⭐⭐ 4 estrellas o más"
                },
                new RatingRangeDto
                {
                    MinRating = 3.0m,
                    Count = await productsWithRatings.CountAsync(p => p.ProductRating.AverageRating >= 3.0m),
                    Label = "⭐⭐⭐ 3 estrellas o más"
                }
            };

            return new RatingFacetDto { Ranges = ranges.Where(r => r.Count > 0).ToList() };
        }

        private async Task<Dictionary<string, AttributeFacetDto>> CalculateAttributeFacetsAsync(IQueryable<Product> query)
        {
            var facets = new Dictionary<string, AttributeFacetDto>();

            // Obtener IDs de productos del query base (materializar solo IDs)
            var productIds = await query.Select(p => p.ProductId).ToListAsync();

            if (!productIds.Any())
            {
                return facets;
            }

            // Obtener atributos filtrables
            var filterableAttributes = await _context.ProductAttributes
                .Where(a => a.IsFilterable)
                .ToListAsync();

            foreach (var attribute in filterableAttributes)
            {
                var attributeFacet = new AttributeFacetDto
                {
                    AttributeId = attribute.AttributeId,
                    AttributeName = attribute.AttributeName,
                    AttributeType = attribute.AttributeType,
                    Unit = attribute.Unit
                };

                if (attribute.AttributeType == "Select" || attribute.AttributeType == "MultiSelect")
                {
                    // Facetas para atributos de selección
                    // Usar Contains() con lista materializada en lugar de Any() con IQueryable
                    attributeFacet.Values = await _context.ProductAttributeValues
                        .Where(pav => pav.AttributeId == attribute.AttributeId)
                        .Where(pav => productIds.Contains(pav.ProductId))
                        .GroupBy(pav => new { pav.ValueId, pav.AttributeValue.ValueText })
                        .Select(g => new FacetItemDto
                        {
                            Id = g.Key.ValueId.Value,
                            Name = g.Key.ValueText,
                            Count = g.Count()
                        })
                        .OrderByDescending(f => f.Count)
                        .ToListAsync();
                }
                else if (attribute.AttributeType == "Number")
                {
                    // Rangos para atributos numéricos
                    // Usar Contains() con lista materializada en lugar de Any() con IQueryable
                    var values = await _context.ProductAttributeValues
                        .Where(pav => pav.AttributeId == attribute.AttributeId)
                        .Where(pav => productIds.Contains(pav.ProductId))
                        .Where(pav => pav.NumericValue != null)
                        .Select(pav => pav.NumericValue.Value)
                        .ToListAsync();

                    if (values.Any())
                    {
                        attributeFacet.Range = new NumericRangeDto
                        {
                            Min = values.Min(),
                            Max = values.Max()
                        };
                    }
                }

                if (attributeFacet.Values?.Any() == true || attributeFacet.Range != null)
                {
                    facets.Add(attribute.AttributeName, attributeFacet);
                }
            }

            return facets;
        }
    }
}
