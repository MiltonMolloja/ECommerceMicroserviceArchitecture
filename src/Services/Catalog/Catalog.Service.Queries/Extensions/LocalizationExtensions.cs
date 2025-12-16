using Catalog.Common;
using Catalog.Domain;
using Catalog.Service.Queries.DTOs;
using Service.Common.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Catalog.Service.Queries.Extensions
{
    /// <summary>
    /// Extensions for localizing domain entities to DTOs based on Accept-Language header
    /// </summary>
    public static class LocalizationExtensions
    {
        /// <summary>
        /// Maps a Product domain entity to ProductDto with localized fields
        /// </summary>
        public static ProductDto ToLocalizedDto(this Product product, ILanguageContext languageContext)
        {
            if (product == null) return null;

            var dto = product.MapTo<ProductDto>();

            // Set localized Name and Description based on language
            if (languageContext.IsEnglish)
            {
                dto.Name = product.NameEnglish;
                dto.Description = product.DescriptionEnglish;
            }
            else // Default to Spanish
            {
                dto.Name = product.NameSpanish;
                dto.Description = product.DescriptionSpanish;
            }

            // Map ProductRating if available, otherwise default to 0
            if (product.ProductRating != null)
            {
                dto.AverageRating = product.ProductRating.AverageRating;
                dto.TotalReviews = product.ProductRating.TotalReviews;
                dto.Rating5Star = product.ProductRating.Rating5Star;
                dto.Rating4Star = product.ProductRating.Rating4Star;
                dto.Rating3Star = product.ProductRating.Rating3Star;
                dto.Rating2Star = product.ProductRating.Rating2Star;
                dto.Rating1Star = product.ProductRating.Rating1Star;
            }
            else
            {
                // Products without ratings should have 0 instead of null
                // This prevents unrated products from appearing in filtered results
                dto.AverageRating = 0;
                dto.TotalReviews = 0;
                dto.Rating5Star = 0;
                dto.Rating4Star = 0;
                dto.Rating3Star = 0;
                dto.Rating2Star = 0;
                dto.Rating1Star = 0;
            }

            // Map Categories with localization
            if (product.ProductCategories != null && product.ProductCategories.Any())
            {
                dto.Categories = product.ProductCategories
                    .Select(pc => pc.Category?.ToLocalizedDto(languageContext))
                    .Where(c => c != null)
                    .ToList();

                // Set primary category (first one, or you can add logic to determine primary)
                dto.PrimaryCategory = dto.Categories.FirstOrDefault();
            }

            // Map Brand name and BrandId if BrandNavigation is available
            if (product.BrandNavigation != null)
            {
                dto.BrandId = product.BrandNavigation.BrandId;
                dto.Brand = product.BrandNavigation.Name;
            }
            else if (product.BrandId.HasValue)
            {
                // If BrandNavigation is null but BrandId exists, keep the BrandId
                dto.BrandId = product.BrandId;
            }
            // If BrandNavigation is null, Brand property already has the value from MapTo

            return dto;
        }

        /// <summary>
        /// Maps a Category domain entity to CategoryDto with localized fields
        /// </summary>
        public static CategoryDto ToLocalizedDto(this Category category, ILanguageContext languageContext)
        {
            if (category == null) return null;

            var dto = category.MapTo<CategoryDto>();

            // Set localized Name and Description based on language
            if (languageContext.IsEnglish)
            {
                dto.Name = category.NameEnglish;
                dto.Description = category.DescriptionEnglish;
            }
            else // Default to Spanish
            {
                dto.Name = category.NameSpanish;
                dto.Description = category.DescriptionSpanish;
            }

            return dto;
        }

        /// <summary>
        /// Maps a collection of Product entities to localized DTOs
        /// </summary>
        public static IEnumerable<ProductDto> ToLocalizedDtos(this IEnumerable<Product> products, ILanguageContext languageContext)
        {
            return products?.Select(p => p.ToLocalizedDto(languageContext));
        }

        /// <summary>
        /// Maps a collection of Category entities to localized DTOs
        /// </summary>
        public static IEnumerable<CategoryDto> ToLocalizedDtos(this IEnumerable<Category> categories, ILanguageContext languageContext)
        {
            return categories?.Select(c => c.ToLocalizedDto(languageContext));
        }
    }
}
