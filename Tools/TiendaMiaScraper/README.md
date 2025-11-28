# TiendaMia Product Scraper

Herramienta de scraping para extraer información de productos de TiendaMia.com y mapearlos al formato de la base de datos `[ECommerceDb].[Catalog].[Products]`.

## Características

- ✅ Extrae datos completos del producto (nombre, precio, marca, descripción)
- ✅ Obtiene todas las URLs de imágenes del producto
- ✅ Genera SKU único basado en la marca y producto
- ✅ Crea slug SEO-friendly automáticamente
- ✅ Traduce campos básicos al español
- ✅ Genera script SQL INSERT listo para usar
- ✅ Guarda resultados en JSON en Obsidian (`C:\Notas\Milton`)
- ✅ Formato compatible 100% con la DB [Catalog].[Products]

## Instalación

Los paquetes NuGet necesarios ya están instalados:
- HtmlAgilityPack 1.12.4
- Newtonsoft.Json 13.0.4

## Uso

### Método 1: Con argumento de línea de comandos

```bash
cd C:\Source\ECommerceMicroserviceArchitecture\tools\TiendaMiaScraper
dotnet run "https://tiendamia.com/ar/p/amz/b0cfwxw85v/lenovo-thinkpad-e16-gen-2-business-laptop-16-0-wuxga-ips"
```

### Método 2: Modo interactivo

```bash
cd C:\Source\ECommerceMicroserviceArchitecture\tools\TiendaMiaScraper
dotnet run
```

Luego ingresa la URL cuando se te solicite.

### Método 3: Compilar y usar el ejecutable

```bash
cd C:\Source\ECommerceMicroserviceArchitecture\tools\TiendaMiaScraper
dotnet publish -c Release -o publish
.\publish\TiendaMiaScraper.exe "URL_DEL_PRODUCTO"
```

## Ejemplos de URLs

```
https://tiendamia.com/ar/p/amz/b0cfwxw85v/lenovo-thinkpad-e16-gen-2-business-laptop-16-0-wuxga-ips
https://tiendamia.com/ar/p/amz/[PRODUCT_ID]/[product-name]
```

## Formato de Salida

El scraper genera un archivo JSON con la siguiente estructura:

```json
{
  "ProductInfo": {
    "Source": "TiendaMia.com",
    "SourceUrl": "...",
    "ScrapedDate": "2025-11-04",
    "SourceSKU": "AMZ-B0CFWXW85V"
  },
  "CatalogProductFormat": {
    "NameSpanish": "...",
    "NameEnglish": "...",
    "DescriptionSpanish": "...",
    "DescriptionEnglish": "...",
    "SKU": "LEN-LETH-B0CFWXW85V",
    "Brand": "Lenovo",
    "Slug": "lenovo-thinkpad-e16-gen-2...",
    "Price": 698.00,
    "Images": "url1, url2, url3, ...",
    "MetaTitle": "...",
    "MetaDescription": "...",
    "MetaKeywords": "...",
    "IsActive": true,
    "IsFeatured": false
  },
  "ExtractedData": {
    "OriginalName": "...",
    "Price": 698.00,
    "Currency": "USD",
    "Brand": "Lenovo",
    "ImageUrls": ["url1", "url2", "..."]
  },
  "SQLInsertScript": "INSERT INTO [Catalog].[Products] ..."
}
```

## Archivos de Salida

Los archivos se guardan automáticamente en:
```
C:\Notas\Milton\TiendaMia-{SKU}-{fecha}-{hora}.json
```

Ejemplo:
```
C:\Notas\Milton\TiendaMia-LEN-LETH-B0CFWXW85V-20251104-213635.json
```

## Insertar en la Base de Datos

El JSON incluye un campo `SQLInsertScript` con el script SQL completo:

```sql
-- Copiar el contenido de SQLInsertScript y ejecutar en SQL Server
INSERT INTO [Catalog].[Products] (
    [NameSpanish], [NameEnglish],
    [DescriptionSpanish], [DescriptionEnglish],
    ...
) VALUES (
    ...
);
```

## Procesamiento por Lotes

Para procesar múltiples productos, crea un archivo `productos.txt` con una URL por línea:

```
https://tiendamia.com/ar/p/amz/producto1/...
https://tiendamia.com/ar/p/amz/producto2/...
https://tiendamia.com/ar/p/amz/producto3/...
```

Luego ejecuta:

```powershell
Get-Content productos.txt | ForEach-Object {
    dotnet run $_
    Start-Sleep -Seconds 2  # Pausa de 2 segundos entre requests
}
```

## Mapeo de Campos

| Campo DB | Fuente | Descripción |
|----------|--------|-------------|
| NameSpanish | Auto-traducido | Nombre traducido al español |
| NameEnglish | HTML Title/H1 | Nombre original del producto |
| DescriptionSpanish | Meta description | Descripción traducida |
| DescriptionEnglish | Meta description | Descripción original |
| SKU | Generado | {BRAND-3}{NAME-4}{SOURCE-ID} |
| Brand | Meta tag | Marca del producto |
| Slug | Generado | URL-friendly del nombre |
| Price | HTML | Precio extraído |
| Images | img[@src] | URLs separadas por comas |
| MetaTitle | Generado | {Brand} - {Name[0:50]} |
| MetaDescription | Description | Primeros 150 caracteres |
| MetaKeywords | Generado | Brand + palabras clave del nombre |

## Personalización

### Agregar más traducciones

Edita el método `TranslateToSpanish()` en `Program.cs`:

```csharp
private string TranslateToSpanish(string text)
{
    var translations = new Dictionary<string, string>
    {
        { "Business Laptop", "Laptop Empresarial" },
        { "Display", "Pantalla" },
        // Agrega más aquí
    };
    // ...
}
```

### Modificar selectores HTML

Si TiendaMia cambia su estructura, actualiza los selectores en:
- `ExtractProductName()`
- `ExtractPrice()`
- `ExtractBrand()`
- `ExtractImages()`
- `ExtractDescription()`

## Notas Importantes

1. **Rate Limiting**: TiendaMia puede bloquear requests demasiado frecuentes. Usa pausas entre scrapes.
2. **User-Agent**: El scraper usa un User-Agent de navegador real para evitar bloqueos.
3. **Imágenes**: Las URLs de imágenes son de Amazon CDN y son estables.
4. **Precios**: Los precios pueden estar en pesos argentinos. Verifica la moneda antes de insertar en la DB.

## Troubleshooting

### Error: "No images found"
- Verifica que la página tenga imágenes cargadas
- Actualiza los selectores de imágenes si TiendaMia cambió su HTML

### Error: "Price is 0"
- El selector de precio puede haber cambiado
- Verifica manualmente el HTML de la página

### Error: "Connection timeout"
- TiendaMia puede estar bloqueando requests
- Espera unos minutos e intenta de nuevo
- Considera usar un proxy o VPN

## Próximas Mejoras

- [ ] Soporte para scraping por lotes desde archivo
- [ ] Detección automática de moneda (USD/ARS)
- [ ] Extracción de especificaciones técnicas
- [ ] Detección de descuentos y precios originales
- [ ] Extracción de variantes del producto
- [ ] Integración directa con la API de Catalog

## Licencia

Herramienta interna para el proyecto ECommerceMicroserviceArchitecture.
