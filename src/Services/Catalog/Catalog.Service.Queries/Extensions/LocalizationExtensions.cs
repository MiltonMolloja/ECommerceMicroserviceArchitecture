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
