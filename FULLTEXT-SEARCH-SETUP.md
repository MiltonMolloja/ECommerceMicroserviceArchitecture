# Full-Text Search Configuration

## 🔴 Error Resuelto

### Error Original
```
Microsoft.Data.SqlClient.SqlException: Cannot use a CONTAINS or FREETEXT predicate 
on table or indexed view 'Catalog.Products' because it is not full-text indexed.
```

**Ubicación**: `ProductQueryService.cs:464`

## ✅ Solución Implementada

### Cambio en el Código

El código ahora usa **LIKE** en lugar de **Full-Text Search** por defecto:

```csharp
// ✅ SOLUCIÓN: Usar LIKE sin Full-Text Search
if (!string.IsNullOrWhiteSpace(request.Query))
{
    var searchTerm = request.Query.Trim().ToLower();

    query = query.Where(p =>
        p.NameSpanish.ToLower().Contains(searchTerm) ||
        p.NameEnglish.ToLower().Contains(searchTerm) ||
        p.DescriptionSpanish.ToLower().Contains(searchTerm) ||
        p.DescriptionEnglish.ToLower().Contains(searchTerm) ||
        p.SKU.ToLower().Contains(searchTerm)
    );
}
```

**Archivo modificado**: `src/Services/Catalog/Catalog.Service.Queries/ProductQueryService.cs`

## 🚀 Cómo Habilitar Full-Text Search (Opcional)

Si deseas usar Full-Text Search para mejor performance en búsquedas de texto:

### Paso 1: Verificar que Full-Text Search esté instalado

```sql
SELECT FULLTEXTSERVICEPROPERTY('IsFullTextInstalled');
-- Debe retornar 1
```

Si retorna 0, instala **"Full-Text and Semantic Extractions for Search"** desde SQL Server Setup.

### Paso 2: Ejecutar el script de configuración

```bash
sqlcmd -S localhost -d CatalogDb -i scripts/enable-fulltext-search.sql
```

O desde **SQL Server Management Studio (SSMS)**:
1. Abre `scripts/enable-fulltext-search.sql`
2. Ejecuta el script completo (F5)

### Paso 3: Modificar el código para usar Full-Text Search

Una vez configurado el índice, puedes cambiar el código a:

```csharp
// Con Full-Text Search habilitado
if (!string.IsNullOrWhiteSpace(request.Query))
{
    var searchTerm = request.Query.Trim();

    query = query.Where(p =>
        EF.Functions.Contains(p.NameSpanish, searchTerm) ||
        EF.Functions.Contains(p.NameEnglish, searchTerm) ||
        EF.Functions.Contains(p.DescriptionSpanish, searchTerm) ||
        EF.Functions.Contains(p.DescriptionEnglish, searchTerm) ||
        EF.Functions.Contains(p.SKU, searchTerm)
    );
}
```

## 📊 Comparación: LIKE vs Full-Text Search

| Característica | LIKE (Actual) | Full-Text Search |
|---------------|---------------|------------------|
| **Performance** | ⚠️ Más lento en tablas grandes | ✅ Muy rápido con índice |
| **Configuración** | ✅ No requiere configuración | ⚠️ Requiere configurar índice |
| **Búsquedas complejas** | ❌ Limitado | ✅ Soporta AND, OR, NEAR, wildcards |
| **Búsqueda lingüística** | ❌ No | ✅ Stemming, sinónimos |
| **Case-insensitive** | ⚠️ Requiere ToLower() | ✅ Por defecto |
| **Espacio en disco** | ✅ No usa espacio adicional | ⚠️ Requiere índice (espacio) |
| **Mantenimiento** | ✅ No requiere | ⚠️ Requiere población periódica |

## 🎯 ¿Cuándo usar Full-Text Search?

### ✅ Usa Full-Text Search si:
- Tienes **más de 100,000 productos**
- Haces **búsquedas frecuentes** de texto
- Necesitas búsquedas complejas (ej: "laptop AND gaming NEAR monitor")
- Quieres **mejor performance** en búsquedas
- Necesitas búsquedas lingüísticas (plurales, sinónimos, etc.)

### ✅ Usa LIKE (actual) si:
- Base de datos pequeña o mediana (<100,000 productos)
- Búsquedas simples de palabras clave
- Quieres simplicidad sin mantenimiento adicional
- No tienes permisos para crear índices Full-Text

## 📝 Script de Configuración

El script `scripts/enable-fulltext-search.sql` realiza:

1. ✅ Verifica que Full-Text esté instalado
2. ✅ Crea Full-Text Catalog (`CatalogFullTextCatalog`)
3. ✅ Crea Full-Text Index en las columnas:
   - `NameSpanish` (Español - LCID 3082)
   - `NameEnglish` (Inglés - LCID 1033)
   - `DescriptionSpanish` (Español)
   - `DescriptionEnglish` (Inglés)
   - `SKU` (Inglés)
4. ✅ Configura auto-tracking de cambios
5. ✅ Inicia la población inicial del índice

## 🔧 Mantenimiento de Full-Text Index

### Verificar estado del índice
```sql
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    is_enabled,
    change_tracking_state_desc,
    crawl_type_desc
FROM sys.fulltext_indexes
WHERE object_id = OBJECT_ID('Catalog.Products');
```

### Verificar progreso de población
```sql
SELECT 
    DB_NAME(database_id) AS DatabaseName,
    OBJECT_NAME(table_id) AS TableName,
    status_description,
    CAST((range_count_completed * 100.0 / range_count) AS DECIMAL(5,2)) AS PercentComplete
FROM sys.dm_fts_index_population
WHERE database_id = DB_ID();
```

### Reorganizar índice (mantenimiento)
```sql
ALTER FULLTEXT INDEX ON Catalog.Products REORGANIZE;
```

### Forzar población incremental
```sql
ALTER FULLTEXT INDEX ON Catalog.Products START INCREMENTAL POPULATION;
```

### Forzar población completa
```sql
ALTER FULLTEXT INDEX ON Catalog.Products START FULL POPULATION;
```

## 🔍 Ejemplos de Búsqueda con Full-Text Search

### Búsqueda simple
```sql
SELECT ProductId, NameSpanish
FROM Catalog.Products
WHERE CONTAINS(NameSpanish, 'laptop');
```

### Búsqueda con AND
```sql
SELECT ProductId, NameSpanish
FROM Catalog.Products
WHERE CONTAINS(NameSpanish, 'laptop AND gaming');
```

### Búsqueda con OR
```sql
SELECT ProductId, NameSpanish
FROM Catalog.Products
WHERE CONTAINS(NameSpanish, 'laptop OR notebook');
```

### Búsqueda con NEAR (palabras cercanas)
```sql
SELECT ProductId, NameSpanish
FROM Catalog.Products
WHERE CONTAINS(NameSpanish, 'laptop NEAR gaming');
```

### Búsqueda con wildcard
```sql
SELECT ProductId, NameSpanish
FROM Catalog.Products
WHERE CONTAINS(NameSpanish, '"compu*"');
```

### FREETEXT (búsqueda más flexible)
```sql
SELECT ProductId, NameSpanish
FROM Catalog.Products
WHERE FREETEXT((NameSpanish, DescriptionSpanish), 'computadora portátil para juegos');
```

## 🎯 Uso en Entity Framework Core

### Con Full-Text Search
```csharp
// Búsqueda simple
query.Where(p => EF.Functions.Contains(p.NameSpanish, "laptop"))

// Búsqueda en múltiples columnas
query.Where(p => 
    EF.Functions.Contains(p.NameSpanish, searchTerm) ||
    EF.Functions.Contains(p.DescriptionSpanish, searchTerm))

// FREETEXT (más flexible pero menos preciso)
query.Where(p => EF.Functions.FreeText(p.NameSpanish, "laptop gaming"))
```

### Con LIKE (actual)
```csharp
var searchTerm = request.Query.Trim().ToLower();

query.Where(p =>
    p.NameSpanish.ToLower().Contains(searchTerm) ||
    p.DescriptionSpanish.ToLower().Contains(searchTerm))
```

## 📚 Referencias

- [SQL Server Full-Text Search](https://learn.microsoft.com/en-us/sql/relational-databases/search/full-text-search)
- [CREATE FULLTEXT INDEX](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-fulltext-index-transact-sql)
- [CONTAINS (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/queries/contains-transact-sql)
- [EF Core Full-Text Search](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/functions#full-text-search)

## ✅ Estado Actual

- [x] Error identificado
- [x] Código modificado para usar LIKE
- [x] Script SQL de configuración creado
- [x] Documentación creada
- [ ] Full-Text Search configurado (opcional)
- [ ] Testing de performance (opcional)

## 🎯 Recomendación

**Para desarrollo y testing**: Usar LIKE (código actual) es suficiente y más simple.

**Para producción con muchos productos**: Considera configurar Full-Text Search para mejor performance.
