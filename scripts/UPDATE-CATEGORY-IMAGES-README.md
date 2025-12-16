# Actualizar Imágenes de Categorías

## Problema
Las categorías **Periféricos**, **Componentes**, **Monitores** y **Mobiliario** no tienen imágenes asignadas (ImageUrl = NULL).

## Solución

### Opción 1: Ejecutar el archivo .bat (Recomendado)
```bash
cd C:\Source\ECommerceMicroserviceArchitecture\scripts
update-category-images.bat
```

### Opción 2: Ejecutar el script SQL directamente
1. Abre **SQL Server Management Studio (SSMS)**
2. Conéctate a `localhost\SQLEXPRESS`
3. Abre el archivo `update-category-images.sql`
4. Ejecuta el script (F5)

### Opción 3: Copiar y pegar en SSMS
```sql
USE ECommerceDb;
GO

-- Periféricos (Teclados, ratones, etc.)
UPDATE Catalog.Categories
SET ImageUrl = 'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400'
WHERE CategoryId = 2;

-- Componentes (Tarjetas gráficas, RAM, etc.)
UPDATE Catalog.Categories
SET ImageUrl = 'https://images.unsplash.com/photo-1591799264318-7e6ef8ddb7ea?w=400'
WHERE CategoryId = 4;

-- Monitores (Pantallas y monitores)
UPDATE Catalog.Categories
SET ImageUrl = 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=400'
WHERE CategoryId = 5;

-- Mobiliario (Sillas gaming, escritorios)
UPDATE Catalog.Categories
SET ImageUrl = 'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=400'
WHERE CategoryId = 6;

GO

-- Verificar
SELECT CategoryId, NameSpanish, ImageUrl FROM Catalog.Categories ORDER BY DisplayOrder;
GO
```

## Imágenes Asignadas

| CategoryId | Categoría | Imagen | Descripción |
|------------|-----------|--------|-------------|
| 2 | Periféricos | [Ver imagen](https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400) | Teclado mecánico RGB |
| 4 | Componentes | [Ver imagen](https://images.unsplash.com/photo-1591799264318-7e6ef8ddb7ea?w=400) | Tarjeta gráfica GPU |
| 5 | Monitores | [Ver imagen](https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=400) | Monitor gaming curvo |
| 6 | Mobiliario | [Ver imagen](https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=400) | Silla de oficina moderna |

## Verificación

Después de ejecutar el script, verifica que las imágenes se hayan actualizado:

```sql
SELECT 
    CategoryId,
    NameSpanish,
    NameEnglish,
    ImageUrl,
    IsFeatured,
    IsActive
FROM Catalog.Categories
WHERE CategoryId IN (2, 4, 5, 6)
ORDER BY DisplayOrder;
```

## Resultado Esperado

Todas las categorías deberían tener una URL de imagen válida:

```
CategoryId | NameSpanish  | ImageUrl
-----------|--------------|------------------------------------------
2          | Periféricos  | https://images.unsplash.com/photo-158...
4          | Componentes  | https://images.unsplash.com/photo-159...
5          | Monitores    | https://images.unsplash.com/photo-152...
6          | Mobiliario   | https://images.unsplash.com/photo-155...
```

## Limpiar caché (Opcional)

Si las imágenes no aparecen inmediatamente en el frontend, limpia el caché de Redis:

```bash
cd C:\Source\ECommerceMicroserviceArchitecture
.\clear-redis-cache.ps1
```

O desde PowerShell:
```powershell
docker exec -it redis redis-cli FLUSHALL
```
