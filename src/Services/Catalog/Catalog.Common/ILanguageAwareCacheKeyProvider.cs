namespace Catalog.Common
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
    /// Implementation of language-aware cache key provider.
    /// </summary>
    public class LanguageAwareCacheKeyProvider : ILanguageAwareCacheKeyProvider
    {
        private readonly ILanguageContext _languageContext;

        public LanguageAwareCacheKeyProvider(ILanguageContext languageContext)
        {
            _languageContext = languageContext;
        }

        public string GenerateKey(string baseKey)
        {
            var language = _languageContext.CurrentLanguage ?? "es";
            return $"{baseKey}_lang={language}";
        }
    }
}
