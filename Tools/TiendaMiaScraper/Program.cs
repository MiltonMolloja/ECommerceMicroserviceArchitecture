using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace TiendaMiaScraper;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== TiendaMia Product Scraper ===\n");

        string inputUrl;
        int maxPages = 1;
        int delaySeconds = 2;

        if (args.Length > 0)
        {
            inputUrl = args[0];
            if (args.Length > 1 && int.TryParse(args[1], out var pages))
                maxPages = pages;
            if (args.Length > 2 && int.TryParse(args[2], out var delay))
                delaySeconds = delay;
        }
        else
        {
            Console.Write("Ingrese la URL (producto o búsqueda): ");
            inputUrl = Console.ReadLine() ?? string.Empty;

            Console.Write("Número máximo de páginas a procesar (1): ");
            var pagesInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(pagesInput) && int.TryParse(pagesInput, out var p))
                maxPages = p;

            Console.Write("Segundos de espera entre productos (2): ");
            var delayInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(delayInput) && int.TryParse(delayInput, out var d))
                delaySeconds = d;
        }

        if (string.IsNullOrWhiteSpace(inputUrl))
        {
            Console.WriteLine("Error: URL no proporcionada.");
            return;
        }

        try
        {
            var scraper = new TiendaMiaScraper();

            // Detectar tipo de URL
            if (IsSearchUrl(inputUrl))
            {
                Console.WriteLine("Detectada URL de búsqueda");
                await ProcessSearchAsync(scraper, inputUrl, maxPages, delaySeconds);
            }
            else
            {
                Console.WriteLine("Detectada URL de producto individual");
                await ProcessSingleProductAsync(scraper, inputUrl);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    static bool IsSearchUrl(string url)
    {
        return url.Contains("/search/") || url.Contains("?q=") || url.Contains("/s/");
    }

    static async Task ProcessSearchAsync(TiendaMiaScraper scraper, string searchUrl, int maxPages, int delaySeconds)
    {
        Console.WriteLine($"\n📋 Extrayendo productos de resultados de búsqueda...");
        Console.WriteLine($"   Páginas máximas: {maxPages}");
        Console.WriteLine($"   Delay entre productos: {delaySeconds}s\n");

        var allProductUrls = new List<string>();

        for (int page = 1; page <= maxPages; page++)
        {
            Console.WriteLine($"--- Página {page} ---");
            var pageUrl = page == 1 ? searchUrl : $"{searchUrl}?page={page}";

            var productUrls = await scraper.ExtractProductUrlsFromSearchAsync(pageUrl);

            if (productUrls.Count == 0)
            {
                Console.WriteLine("No se encontraron más productos. Finalizando.");
                break;
            }

            allProductUrls.AddRange(productUrls);
            Console.WriteLine($"✓ Encontrados {productUrls.Count} productos en esta página");

            if (page < maxPages)
            {
                await Task.Delay(delaySeconds * 1000);
            }
        }

        Console.WriteLine($"\n📦 Total de productos encontrados: {allProductUrls.Count}");
        Console.WriteLine($"⏱️  Tiempo estimado: {(allProductUrls.Count * delaySeconds / 60):F1} minutos\n");

        // Guardar lista de URLs
        var urlsListPath = Path.Combine(@"C:\Notas\Milton", $"TiendaMia-Search-URLs-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
        await File.WriteAllLinesAsync(urlsListPath, allProductUrls);
        Console.WriteLine($"✓ Lista de URLs guardada: {urlsListPath}\n");

        // Procesar cada producto
        var results = new List<ProductScrapingResult>();
        int current = 0;
        int success = 0;
        int failed = 0;

        foreach (var productUrl in allProductUrls)
        {
            current++;
            Console.WriteLine($"\n[{current}/{allProductUrls.Count}] Procesando: {productUrl}");

            try
            {
                var product = await scraper.ScrapeProductAsync(productUrl);
                results.Add(product);

                // Guardar producto individual
                var json = JsonConvert.SerializeObject(product, Formatting.Indented);
                var outputPath = @"C:\Notas\Milton";
                var fileName = $"TiendaMia-{product.CatalogProductFormat.SKU}-{DateTime.Now:yyyyMMdd-HHmmss}.md";
                var fullPath = Path.Combine(outputPath, fileName);

                Directory.CreateDirectory(outputPath);
                await File.WriteAllTextAsync(fullPath, json);

                Console.WriteLine($"  ✓ SKU: {product.CatalogProductFormat.SKU}");
                Console.WriteLine($"  ✓ Precio: ${product.CatalogProductFormat.Price}");
                Console.WriteLine($"  ✓ Guardado: {fileName}");
                success++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error: {ex.Message}");
                failed++;
            }

            // Delay entre productos (excepto el último)
            if (current < allProductUrls.Count)
            {
                await Task.Delay(delaySeconds * 1000);
            }
        }

        // Guardar resumen consolidado
        var summaryPath = Path.Combine(@"C:\Notas\Milton", $"TiendaMia-Search-Summary-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        var summary = new
        {
            SearchUrl = searchUrl,
            TotalProducts = allProductUrls.Count,
            Successful = success,
            Failed = failed,
            ScrapedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Products = results.Select(r => new
            {
                r.CatalogProductFormat.SKU,
                r.CatalogProductFormat.Brand,
                r.CatalogProductFormat.NameEnglish,
                r.CatalogProductFormat.Price,
                r.CatalogProductFormat.Images,
                r.ProductInfo.SourceUrl
            })
        };

        var summaryJson = JsonConvert.SerializeObject(summary, Formatting.Indented);
        await File.WriteAllTextAsync(summaryPath, summaryJson);

        Console.WriteLine($"\n{'='}{new string('=', 50)}");
        Console.WriteLine($"📊 RESUMEN FINAL");
        Console.WriteLine($"{'='}{new string('=', 50)}");
        Console.WriteLine($"Total encontrados: {allProductUrls.Count}");
        Console.WriteLine($"Exitosos: {success} ✓");
        Console.WriteLine($"Fallidos: {failed} ✗");
        Console.WriteLine($"\n📁 Archivos guardados en: C:\\Notas\\Milton\\");
        Console.WriteLine($"📄 Resumen consolidado: {Path.GetFileName(summaryPath)}");
    }

    static async Task ProcessSingleProductAsync(TiendaMiaScraper scraper, string productUrl)
    {
        var product = await scraper.ScrapeProductAsync(productUrl);

        // Generar JSON
        var json = JsonConvert.SerializeObject(product, Formatting.Indented);

        // Guardar en Obsidian
        var outputPath = @"C:\Notas\Milton";
        var fileName = $"TiendaMia-{product.CatalogProductFormat.SKU}-{DateTime.Now:yyyyMMdd-HHmmss}.md";
        var fullPath = Path.Combine(outputPath, fileName);

        Directory.CreateDirectory(outputPath);
        await File.WriteAllTextAsync(fullPath, json);

        Console.WriteLine($"\n✓ Producto extraído exitosamente!");
        Console.WriteLine($"✓ Archivo guardado en: {fullPath}");
        Console.WriteLine($"\nSKU: {product.CatalogProductFormat.SKU}");
        Console.WriteLine($"Nombre: {product.CatalogProductFormat.NameEnglish}");
        Console.WriteLine($"Precio: ${product.CatalogProductFormat.Price}");
        Console.WriteLine($"Imágenes encontradas: {product.ExtractedData.ImageUrls.Count}");
        Console.WriteLine($"\n{json}");
    }
}

public class TiendaMiaScraper
{
    private readonly HttpClient _httpClient;

    public TiendaMiaScraper()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<List<string>> ExtractProductUrlsFromSearchAsync(string searchUrl)
    {
        Console.WriteLine($"  Extrayendo de: {searchUrl}");

        var html = await _httpClient.GetStringAsync(searchUrl);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var productUrls = new List<string>();

        // Selectores para encontrar enlaces de productos
        var selectors = new[]
        {
            "//a[contains(@href, '/p/amz/')]/@href",
            "//a[contains(@href, '/p/mkt/')]/@href",
            "//div[contains(@class, 'product')]//a/@href",
            "//article//a/@href"
        };

        foreach (var selector in selectors)
        {
            var nodes = doc.DocumentNode.SelectNodes(selector);
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var href = node.GetAttributeValue("href", "");

                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        // Construir URL completa si es relativa
                        if (href.StartsWith("/"))
                            href = "https://tiendamia.com" + href;

                        // Verificar que sea un producto válido
                        if ((href.Contains("/p/amz/") || href.Contains("/p/mkt/")) &&
                            !productUrls.Contains(href))
                        {
                            productUrls.Add(href);
                        }
                    }
                }
            }
        }

        return productUrls.Distinct().ToList();
    }

    public async Task<ProductScrapingResult> ScrapeProductAsync(string url)
    {
        var html = await _httpClient.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var result = new ProductScrapingResult
        {
            ProductInfo = new ProductSourceInfo
            {
                Source = "TiendaMia.com",
                SourceUrl = url,
                ScrapedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                SourceSKU = ExtractSKUFromUrl(url)
            }
        };

        // Extraer datos
        var name = ExtractProductName(doc);
        var price = ExtractPrice(doc);
        var brand = ExtractBrand(doc);
        var images = ExtractImages(doc);
        var description = ExtractDescription(doc);

        // Llenar datos extraídos
        result.ExtractedData = new ExtractedProductData
        {
            OriginalName = name,
            Price = price,
            Currency = "ARS", // Pesos argentinos por defecto
            ImageUrls = images,
            Brand = brand
        };

        // Generar SKU único
        var sku = GenerateSKU(brand, name, result.ProductInfo.SourceSKU);
        var slug = GenerateSlug(name);

        // Mapear al formato de la DB
        result.CatalogProductFormat = new CatalogProduct
        {
            NameSpanish = TranslateToSpanish(name),
            NameEnglish = name,
            DescriptionSpanish = TranslateToSpanish(description),
            DescriptionEnglish = description,
            SKU = sku,
            Brand = brand,
            Slug = slug,
            Price = price,
            OriginalPrice = null,
            DiscountPercentage = 0,
            TaxRate = 0,
            Images = string.Join(", ", images),
            MetaTitle = $"{brand} - {name.Substring(0, Math.Min(name.Length, 50))}",
            MetaDescription = description.Substring(0, Math.Min(description.Length, 150)),
            MetaKeywords = GenerateKeywords(brand, name),
            IsActive = true,
            IsFeatured = false
        };

        // Generar SQL
        result.SQLInsertScript = GenerateSQLInsert(result.CatalogProductFormat);

        return result;
    }

    private string ExtractProductName(HtmlDocument doc)
    {
        // Intentar varios selectores
        var selectors = new[]
        {
            "//h1[@class='product-title']",
            "//h1[contains(@class, 'title')]",
            "//h1",
            "//meta[@property='og:title']/@content",
            "//title"
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            if (node != null)
            {
                var name = selector.Contains("@content")
                    ? node.GetAttributeValue("content", "")
                    : node.InnerText.Trim();

                if (!string.IsNullOrWhiteSpace(name))
                    return CleanText(name);
            }
        }

        return "Unknown Product";
    }

    private decimal ExtractPrice(HtmlDocument doc)
    {
        var selectors = new[]
        {
            "//span[contains(@class, 'price')]",
            "//div[contains(@class, 'price')]",
            "//meta[@property='product:price:amount']/@content",
            "//span[contains(text(), '$')]"
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            if (node != null)
            {
                var priceText = selector.Contains("@content")
                    ? node.GetAttributeValue("content", "")
                    : node.InnerText;

                // Remover símbolos de moneda y puntos de miles, mantener comas decimales
                priceText = priceText.Replace("$", "").Replace("AR$", "").Replace("U$S", "").Trim();

                var match = Regex.Match(priceText, @"[\d,\.]+");
                if (match.Success)
                {
                    var numStr = match.Value.Replace(".", "").Replace(",", ".");
                    if (decimal.TryParse(numStr, out var price))
                    {
                        return price;
                    }
                }
            }
        }

        return 0;
    }

    private string ExtractBrand(HtmlDocument doc)
    {
        var selectors = new[]
        {
            "//span[@class='brand']",
            "//meta[@property='product:brand']/@content",
            "//a[contains(@class, 'brand')]"
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            if (node != null)
            {
                var brand = selector.Contains("@content")
                    ? node.GetAttributeValue("content", "")
                    : node.InnerText.Trim();

                if (!string.IsNullOrWhiteSpace(brand))
                    return CleanText(brand);
            }
        }

        return "Unknown";
    }

    private List<string> ExtractImages(HtmlDocument doc)
    {
        var images = new List<string>();

        // Buscar imágenes en varios lugares
        var imageSelectors = new[]
        {
            "//img[contains(@class, 'product')]/@src",
            "//img[contains(@src, 'media-amazon')]/@src",
            "//div[contains(@class, 'image')]//img/@src",
            "//meta[@property='og:image']/@content",
            "//img[contains(@class, 'gallery')]/@src",
            "//picture//img/@src",
            "//img/@src",
            "//img/@data-src"
        };

        foreach (var selector in imageSelectors)
        {
            var nodes = doc.DocumentNode.SelectNodes(selector);
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var imgUrl = node.GetAttributeValue(
                        selector.Contains("@content") ? "content" :
                        selector.Contains("@data-src") ? "data-src" : "src", "");

                    if (!string.IsNullOrWhiteSpace(imgUrl) &&
                        (imgUrl.StartsWith("http") || imgUrl.StartsWith("//")))
                    {
                        if (imgUrl.StartsWith("//"))
                            imgUrl = "https:" + imgUrl;

                        // Solo procesar imágenes de Amazon
                        if (imgUrl.Contains("media-amazon"))
                        {
                            // Convertir cualquier tamaño a SL1500 (máxima resolución)
                            imgUrl = Regex.Replace(imgUrl, @"\._AC_[A-Z0-9_\.]+\.jpg", "._AC_SL1500_.jpg");
                            imgUrl = Regex.Replace(imgUrl, @"\._AC_SR\d+,\d+_\.jpg", "._AC_SL1500_.jpg");
                            imgUrl = Regex.Replace(imgUrl, @"\._AC_US\d+_\.jpg", "._AC_SL1500_.jpg");
                            imgUrl = Regex.Replace(imgUrl, @"\._AC_\.jpg", "._AC_SL1500_.jpg");

                            // Evitar duplicados
                            if (!images.Contains(imgUrl))
                                images.Add(imgUrl);
                        }
                    }
                }
            }
        }

        return images.Distinct().ToList();
    }

    private string ExtractDescription(HtmlDocument doc)
    {
        var selectors = new[]
        {
            "//meta[@name='description']/@content",
            "//div[contains(@class, 'description')]",
            "//div[@id='description']"
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            if (node != null)
            {
                var desc = selector.Contains("@content")
                    ? node.GetAttributeValue("content", "")
                    : node.InnerText;

                if (!string.IsNullOrWhiteSpace(desc))
                    return CleanText(desc);
            }
        }

        return "No description available";
    }

    private string ExtractSKUFromUrl(string url)
    {
        var match = Regex.Match(url, @"/p/(amz|mkt)/([^/]+)");
        if (match.Success)
            return match.Groups[1].Value.ToUpper() + "-" + match.Groups[2].Value.ToUpper();

        return "SKU-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }

    private string GenerateSKU(string brand, string name, string sourceSku)
    {
        var brandPart = new string(brand.Take(3).ToArray()).ToUpper();
        var namePart = new string(name.Split(' ').Take(2).SelectMany(w => w.Take(2)).ToArray()).ToUpper();
        var unique = sourceSku.Split('-').LastOrDefault() ?? Guid.NewGuid().ToString("N").Substring(0, 8);

        return $"{brandPart}-{namePart}-{unique}";
    }

    private string GenerateSlug(string name)
    {
        var slug = name.ToLower();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-').Substring(0, Math.Min(slug.Length, 100));
    }

    private string GenerateKeywords(string brand, string name)
    {
        var words = name.ToLower().Split(' ')
            .Where(w => w.Length > 3)
            .Take(5);

        return $"{brand.ToLower()}, {string.Join(", ", words)}";
    }

    private string TranslateToSpanish(string text)
    {
        // Traducciones básicas comunes
        var translations = new Dictionary<string, string>
        {
            { "Business Laptop", "Laptop Empresarial" },
            { "Gaming Laptop", "Laptop Gamer" },
            { "Notebook", "Portátil" },
            { "Display", "Pantalla" },
            { "Screen", "Pantalla" },
            { "Processor", "Procesador" },
            { "RAM", "Memoria RAM" },
            { "Storage", "Almacenamiento" },
            { "Backlit Keyboard", "Teclado Retroiluminado" },
            { "Fingerprint", "Lector de Huellas" },
            { "WiFi", "WiFi" },
            { "includes", "incluye" },
            { "with", "con" },
            { "and", "y" }
        };

        var translated = text;
        foreach (var (english, spanish) in translations)
        {
            translated = Regex.Replace(translated, english, spanish, RegexOptions.IgnoreCase);
        }

        return translated;
    }

    private string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return System.Net.WebUtility.HtmlDecode(text)
            .Trim()
            .Replace("\n", " ")
            .Replace("\r", "")
            .Replace("\t", " ")
            .Replace("  ", " ");
    }

    private string GenerateSQLInsert(CatalogProduct product)
    {
        return $@"-- SQL para insertar en [ECommerceDb].[Catalog].[Products]
INSERT INTO [Catalog].[Products] (
    [NameSpanish], [NameEnglish],
    [DescriptionSpanish], [DescriptionEnglish],
    [SKU], [Brand], [Slug],
    [Price], [OriginalPrice], [DiscountPercentage], [TaxRate],
    [Images],
    [MetaTitle], [MetaDescription], [MetaKeywords],
    [IsActive], [IsFeatured],
    [CreatedAt], [UpdatedAt]
)
VALUES (
    '{EscapeSql(product.NameSpanish)}',
    '{EscapeSql(product.NameEnglish)}',
    '{EscapeSql(product.DescriptionSpanish)}',
    '{EscapeSql(product.DescriptionEnglish)}',
    '{EscapeSql(product.SKU)}',
    '{EscapeSql(product.Brand)}',
    '{EscapeSql(product.Slug)}',
    {product.Price},
    {(product.OriginalPrice.HasValue ? product.OriginalPrice.Value.ToString() : "NULL")},
    {product.DiscountPercentage},
    {product.TaxRate},
    '{EscapeSql(product.Images)}',
    '{EscapeSql(product.MetaTitle)}',
    '{EscapeSql(product.MetaDescription)}',
    '{EscapeSql(product.MetaKeywords)}',
    {(product.IsActive ? "1" : "0")},
    {(product.IsFeatured ? "1" : "0")},
    GETUTCDATE(),
    GETUTCDATE()
);";
    }

    private string EscapeSql(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text.Replace("'", "''");
    }
}

// Modelos
public class ProductScrapingResult
{
    public ProductSourceInfo ProductInfo { get; set; } = new();
    public CatalogProduct CatalogProductFormat { get; set; } = new();
    public ExtractedProductData ExtractedData { get; set; } = new();
    public string SQLInsertScript { get; set; } = string.Empty;
}

public class ProductSourceInfo
{
    public string Source { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string ScrapedDate { get; set; } = string.Empty;
    public string SourceSKU { get; set; } = string.Empty;
}

public class CatalogProduct
{
    public string NameSpanish { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public string DescriptionSpanish { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TaxRate { get; set; }
    public string Images { get; set; } = string.Empty;
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string MetaKeywords { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
}

public class ExtractedProductData
{
    public string OriginalName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
}
