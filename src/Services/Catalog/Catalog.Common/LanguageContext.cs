using System;

namespace Catalog.Common
{
    public interface ILanguageContext
    {
        string CurrentLanguage { get; }
        bool IsSpanish { get; }
        bool IsEnglish { get; }
    }

    public class LanguageContext : ILanguageContext
    {
        public string CurrentLanguage { get; set; } = "es";

        public bool IsSpanish => CurrentLanguage?.StartsWith("es", StringComparison.OrdinalIgnoreCase) ?? true;
        public bool IsEnglish => CurrentLanguage?.StartsWith("en", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
