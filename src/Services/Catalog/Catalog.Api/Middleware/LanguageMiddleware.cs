using Catalog.Common;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Api.Middleware
{
    public class LanguageMiddleware
    {
        private readonly RequestDelegate _next;

        public LanguageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILanguageContext languageContext)
        {
            var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();

            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                // Extract the primary language (e.g., "es-ES" -> "es", "en-US" -> "en")
                var language = acceptLanguage.Split(',').FirstOrDefault()?.Split('-').FirstOrDefault()?.Trim();

                if (!string.IsNullOrEmpty(language))
                {
                    if (languageContext is LanguageContext langContext)
                    {
                        langContext.CurrentLanguage = language;
                    }
                }
            }

            await _next(context);
        }
    }
}
