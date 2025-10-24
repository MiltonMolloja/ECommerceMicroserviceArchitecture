using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Api.Gateway.Models
{
    /// <summary>
    /// Provides cache keys that include the current language from Accept-Language header.
    /// This ensures that cached responses are language-specific.
    /// </summary>
    public interface ILanguageAwareCacheKeyProvider
    {
        /// <summary>
        /// Generates a cache key that includes the current request language.
        /// </summary>
        /// <param name="baseKey">The base cache key (e.g., "products_1_10")</param>
        /// <returns>A language-aware cache key (e.g., "products_1_10_lang=en")</returns>
        string GenerateKey(string baseKey);
    }

    /// <summary>
    /// Implementation of language-aware cache key provider for API Gateway.
    /// </summary>
    public class LanguageAwareCacheKeyProvider : ILanguageAwareCacheKeyProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageAwareCacheKeyProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateKey(string baseKey)
        {
            var acceptLanguage = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].FirstOrDefault();

            // Extract primary language (e.g., "es-ES" -> "es", "en-US" -> "en")
            var language = "es"; // Default

            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                var primaryLang = acceptLanguage.Split(',').FirstOrDefault()?.Split('-').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(primaryLang))
                {
                    language = primaryLang;
                }
            }

            return $"{baseKey}_lang={language}";
        }
    }
}
