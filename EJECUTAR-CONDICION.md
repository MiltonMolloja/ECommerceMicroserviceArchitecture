# Cómo Ejecutar el Script de Condición

## El script está listo, solo necesitas ejecutarlo manualmente

### Opción 1: Docker (Recomendado)

Si tienes Docker Desktop corriendo, abre PowerShell o CMD y ejecuta:

```bash
# 1. Copiar el script al contenedor
docker cp scripts/add-product-condition.sql sqlserver:/tmp/

# 2. Ejecutar el script
docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "MyComplexPassword123!" -d "ecommerce-db" -i /tmp/add-product-condition.sql
```

### Opción 2: SQL Server Management Studio (SSMS)

1. Abre **SQL Server Management Studio**
2. Conéctate a tu instancia SQL Server
3. Abre el archivo: `scripts/add-product-condition.sql`
4. Asegúrate de estar en la base de datos `ecommerce-db`
5. Presiona **F5** o click en **Execute**

### Opción 3: Azure Data Studio

1. Abre **Azure Data Studio**
2. Conéctate a tu instancia SQL Server
3. File → Open File → `scripts/add-product-condition.sql`
4. Selecciona la base de datos `ecommerce-db` en el dropdown
5. Click en **Run** o presiona **F5**

### Opción 4: sqlcmd desde CMD/PowerShell

Si tienes sqlcmd instalado:

```bash
sqlcmd -S localhost -U sa -P "MyComplexPassword123!" -d "ecommerce-db" -i "scripts\add-product-condition.sql"
```

### Opción 5: Copiar y Pegar el SQL

Si nada de lo anterior funciona, puedes:

1. Abrir `scripts/add-product-condition.sql` en un editor de texto
2. Copiar TODO el contenido
3. Abrir tu herramienta SQL favorita (SSMS, Azure Data Studio, DBeaver, etc.)
4. Pegar el código
5. Ejecutar

## Qué Esperar

Verás un output similar a:

```
=== Agregando atributo Condición ===

1. Creando atributo "Condition"...
   ✅ AttributeId creado: 110

2. Creando valores del atributo...
   ✅ Valor "Nuevo/New" creado: 1070
   ✅ Valor "Usado/Used" creado: 1071

3. Total de productos activos: 1000

4. Productos que serán marcados como usados (25%): 250

5. Asignando condición "Usado" a productos aleatorios...
   ✅ Productos marcados como "Usado": 250

6. Asignando condición "Nuevo" a productos restantes...
   ✅ Productos marcados como "Nuevo": 750

7. Verificando distribución final...

Condicion  Condition  TotalProductos  Porcentaje
---------  ---------  --------------  ----------
Nuevo      New        750             75.00
Usado      Used       250             25.00

=== ✅ Proceso Completado ===

Uso en filtros:
  GET /products/search?filter_attr_110=1070  (Nuevos)
  GET /products/search?filter_attr_110=1071  (Usados)
```

## Después de Ejecutar

### 1. Limpiar Cache (Importante!)

Abre PowerShell en la carpeta del proyecto:

```powershell
.\clear-redis-cache.ps1
```

O manualmente:

```bash
docker exec -it redis redis-cli FLUSHALL
```

### 2. Probar en la API

Reemplaza `110`, `1070`, `1071` con los IDs que te muestra el script:

```bash
# Productos usados
curl -X GET "https://localhost:45000/products/search?query=laptop&filter_attr_110=1071" -k

# Productos nuevos  
curl -X GET "https://localhost:45000/products/search?query=laptop&filter_attr_110=1070" -k
```

O prueba en Swagger:
- https://localhost:45000/swagger
- Endpoint: `GET /products/search`
- Query params: `query=laptop&filter_attr_110=1071`

## Verificar que Funcionó

Ejecuta este SQL para ver la distribución:

```sql
USE [ecommerce-db];

SELECT 
    av.ValueSpanish as Condicion,
    COUNT(*) as Total,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM Products WHERE IsActive = 1) AS DECIMAL(5,2)) as Porcentaje
FROM ProductAttributeValues pav
INNER JOIN AttributeValues av ON pav.ValueId = av.ValueId
INNER JOIN ProductAttributes pa ON pav.AttributeId = pa.AttributeId
WHERE pa.AttributeName = 'Condition'
GROUP BY av.ValueSpanish;
```

Deberías ver algo como:

```
Condicion  Total  Porcentaje
---------  -----  ----------
Nuevo      750    75.00
Usado      250    25.00
```

## Troubleshooting

### Si ves error "AttributeId ya existe"

El script automáticamente limpia datos anteriores, pero si persiste:

```sql
DELETE FROM ProductAttributeValues 
WHERE AttributeId = (SELECT AttributeId FROM ProductAttributes WHERE AttributeName = 'Condition');

DELETE FROM AttributeValues 
WHERE AttributeId = (SELECT AttributeId FROM ProductAttributes WHERE AttributeName = 'Condition');

DELETE FROM ProductAttributes 
WHERE AttributeName = 'Condition';
```

Luego ejecuta el script nuevamente.

### Si no ves cambios en la API

1. **Limpia cache**: `.\clear-redis-cache.ps1`
2. **Reinicia Catalog Service**: 
   ```bash
   docker-compose restart catalog.api
   ```
3. **Verifica en BD** que los datos existen

## Necesitas Ayuda?

El script SQL está en: `scripts/add-product-condition.sql`

Es completamente seguro ejecutarlo múltiples veces (elimina datos anteriores automáticamente).
