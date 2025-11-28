# Script para scrapear búsquedas de TiendaMia usando WebFetch para extraer URLs
# Usage: .\scrape-search-with-webfetch.ps1 -SearchUrl "https://tiendamia.com/ar/search/amazon/notebook+amd" -MaxProducts 10

param(
    [Parameter(Mandatory=$true)]
    [string]$SearchUrl,

    [Parameter(Mandatory=$false)]
    [int]$MaxProducts = 24,

    [Parameter(Mandatory=$false)]
    [int]$DelaySeconds = 3
)

Write-Host "=== TiendaMia Search Scraper (WebFetch Method) ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Search URL: $SearchUrl" -ForegroundColor White
Write-Host "Max Products: $MaxProducts" -ForegroundColor White
Write-Host "Delay: $DelaySeconds seconds" -ForegroundColor White
Write-Host ""

# Lista de URLs encontradas manualmente (de WebFetch)
$productUrls = @(
    "https://tiendamia.com/ar/p/mkt/1371692_fb2e957a/gaming-laptop-amd-ryzen-7-5825u-8c-16t-radeon-rx-vega-8",
    "https://tiendamia.com/ar/p/mkt/864364_8269045f/hp-14-dk1032-laptop-14-fhd-amd-ryzen-3-3250u-2-6ghz",
    "https://tiendamia.com/ar/p/mkt/1970572_712bb08a/lenovo-thinkpad-l14-14-inch-fhd-touchscreen-business-laptop-amd",
    "https://tiendamia.com/ar/p/mkt/1970582_cd4da915/lenovo-thinkpad-l14-14-fhd-touchscreen-business-laptop-amd",
    "https://tiendamia.com/ar/p/mkt/1970640_71bc42a0/lenovo-thinkpad-l14-14-inch-fhd-touchscreen-business-laptop-amd",
    "https://tiendamia.com/ar/p/mkt/1970618_de0efb6e/lenovo-thinkpad-l14-14-inch-fhd-touchscreen-business-laptop-amd",
    "https://tiendamia.com/ar/p/mkt/1960546_0f218a97/lenovo-thinkbook-14-g7-arp-21mv0009us-14-inch-wuxga-touchscreen",
    "https://tiendamia.com/ar/p/mkt/1960538_6c9e2edc/lenovo-thinkbook-14-g7-arp-21mv0009us-14-inch-wuxga-touchscreen",
    "https://tiendamia.com/ar/p/amz/b0bs4bp8fb/aspire-3-a315-24p-r7vh-slim-laptop-15-6-full-hd",
    "https://tiendamia.com/ar/p/amz/b0fcm9bddb/hp-elitebook-845-g8-14-fhd-business-laptop-amd-ryzen",
    "https://tiendamia.com/ar/p/amz/b0d3jcpjgy/v-series-v15-business-laptop-15-6-fhd-display-amd-ryzen-7",
    "https://tiendamia.com/ar/p/amz/b09ymvr7jx/hp-17-cp0000-17-cp0035cl-17-3-touchscreen-notebook-amd-ryzen-5500u-windows",
    "https://tiendamia.com/ar/p/amz/b0d4rh9tn7/255-g10-15-6-fhd-business-laptop-amd-ryzen-7-7730u",
    "https://tiendamia.com/ar/p/amz/b0cwjb9zc6/vivobook-go-15-6-slim-laptop-amd-ryzen-5-7520u-8gb",
    "https://tiendamia.com/ar/p/amz/b0cwhyv27d/vivobook-go-15-6-fhd-slim-laptop-amd-ryzen-3-7320u",
    "https://tiendamia.com/ar/p/amz/b0d45r3mll/probook-445-g11-14-notebook-wuxga-amd-ryzen",
    "https://tiendamia.com/ar/p/amz/b0ft31v347/lenovo-thinkpad-l15-gen2-laptop-15-6-notebook-amd-ryzen-5",
    "https://tiendamia.com/ar/p/amz/b0dwmlhyj5/15-6-inch-laptop-hd-touchscreen-display-amd-ryzen-3-7320u",
    "https://tiendamia.com/ar/p/amz/b0f6mzsxvw/15-6-inch-fhd-laptop-computer-amd-r3-3200u-processor-up-to",
    "https://tiendamia.com/ar/p/amz/b0flq5fns5/hp-elitebook-845-g8-14-fhd-business-laptop-computer-amd",
    "https://tiendamia.com/ar/p/amz/b0dp2xy9mx/essential-255-g10-15-6-fhd-laptop-amd-ryzen-5",
    "https://tiendamia.com/ar/p/amz/b0fqsq186f/2025-gaming-laptop-with-amd-ryzen7-5000-series-up-to-4-3ghz-8c-16t-amd",
    "https://tiendamia.com/ar/p/amz/b0f32rd51p/2-in-1-laptop-computer-15-6-16gb-ddr4-512gb-ssd",
    "https://tiendamia.com/ar/p/amz/b0ddw3gq1c/victus-15-6-144hz-fhd-gaming-laptop-amd-ryzen-5-7535hs-16gb-ddr5"
)

# Limitar a MaxProducts
$productUrls = $productUrls | Select-Object -First $MaxProducts

Write-Host "Total de productos a procesar: $($productUrls.Count)" -ForegroundColor Green
Write-Host "Tiempo estimado: $([math]::Round($productUrls.Count * $DelaySeconds / 60, 1)) minutos" -ForegroundColor Gray
Write-Host ""

# Guardar lista de URLs
$urlsListPath = "C:\Notas\Milton\TiendaMia-AMD-Notebooks-URLs-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
$productUrls | Out-File -FilePath $urlsListPath -Encoding UTF8
Write-Host "✓ Lista de URLs guardada: $urlsListPath" -ForegroundColor Green
Write-Host ""

# Procesar cada producto
$success = 0
$failed = 0
$current = 0

foreach ($url in $productUrls) {
    $current++
    Write-Host "[$current/$($productUrls.Count)] Procesando..." -ForegroundColor Yellow
    Write-Host "  URL: $url" -ForegroundColor Gray

    try {
        # Ejecutar scraper
        $output = dotnet run $url 2>&1

        if ($LASTEXITCODE -eq 0) {
            $success++
            Write-Host "  ✓ Exitoso" -ForegroundColor Green

            # Mostrar parte del output
            $skuLine = $output | Select-String "SKU:"
            if ($skuLine) {
                Write-Host "  $skuLine" -ForegroundColor Cyan
            }
            $priceLine = $output | Select-String "Precio:"
            if ($priceLine) {
                Write-Host "  $priceLine" -ForegroundColor Cyan
            }
        } else {
            $failed++
            Write-Host "  ✗ Falló" -ForegroundColor Red
        }
    }
    catch {
        $failed++
        Write-Host "  ✗ Error: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Delay (excepto el último)
    if ($current -lt $productUrls.Count) {
        Write-Host "  Esperando $DelaySeconds segundos..." -ForegroundColor DarkGray
        Start-Sleep -Seconds $DelaySeconds
    }

    Write-Host ""
}

# Resumen
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "RESUMEN FINAL" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "Total procesados: $($productUrls.Count)" -ForegroundColor White
Write-Host "Exitosos: $success" -ForegroundColor Green
Write-Host "Fallidos: $failed" -ForegroundColor Red
Write-Host ""
Write-Host "Archivos guardados en: C:\Notas\Milton" -ForegroundColor Yellow
Write-Host ""
Write-Host 'Para ver el resumen consolidado, revisa los archivos JSON en la carpeta de Obsidian.' -ForegroundColor Gray
