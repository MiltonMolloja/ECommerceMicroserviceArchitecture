# Guía de Refresh Tokens

Se ha implementado exitosamente el sistema de Refresh Tokens en el servicio de Identity. Esta funcionalidad permite renovar el Access Token sin requerir que el usuario vuelva a ingresar sus credenciales.

## Componentes implementados

### 1. Entidad RefreshToken
- **Ubicación**: `Identity.Domain/RefreshToken.cs`
- **Propiedades principales**:
  - `Token`: Token único generado criptográficamente
  - `UserId`: Identificador del usuario
  - `CreatedAt`, `ExpiresAt`: Control de tiempo de vida
  - `IsRevoked`: Estado de revocación
  - `CreatedByIp`, `RevokedByIp`: Auditoría de IP

### 2. Base de datos
- **Migración**: `AddRefreshTokens` (aplicada exitosamente)
- **Tabla**: `RefreshTokens` en el esquema `Identity`
- **Relación**: Un usuario puede tener múltiples refresh tokens

### 3. Servicio de RefreshToken
- **Interfaz**: `IRefreshTokenService`
- **Implementación**: `RefreshTokenService`
- **Métodos**:
  - `GenerateRefreshTokenAsync()`: Genera un nuevo refresh token
  - `ValidateRefreshTokenAsync()`: Valida un refresh token
  - `RevokeRefreshTokenAsync()`: Revoca un refresh token
  - `RevokeAllUserTokensAsync()`: Revoca todos los tokens de un usuario
  - `CleanupExpiredTokensAsync()`: Limpia tokens expirados

### 4. Configuración
- **Tiempo de vida Access Token**: 1 día
- **Tiempo de vida Refresh Token**: 7 días
- **Registrado en**: `Identity.Api/Startup.cs`

## Endpoints disponibles

### 1. Login (Actualizado)
```http
POST /v1/identity/authentication
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "contraseña"
}
```

**Respuesta exitosa**:
```json
{
  "succeeded": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-secure-random-token",
  "expiresAt": "2025-10-21T16:47:06.123Z"
}
```

### 2. Renovar Access Token (Nuevo)
```http
POST /v1/identity/refresh-token
Content-Type: application/json

{
  "refreshToken": "el-refresh-token-recibido-en-login"
}
```

**Respuesta exitosa**:
```json
{
  "succeeded": true,
  "accessToken": "nuevo-access-token",
  "refreshToken": "nuevo-refresh-token",
  "expiresAt": "2025-10-21T16:47:06.123Z"
}
```

**Notas**:
- El refresh token antiguo se revoca automáticamente
- Se genera un nuevo refresh token en cada renovación
- Si el refresh token es inválido o expirado, retorna 401 Unauthorized

### 3. Revocar Token (Nuevo)
```http
POST /v1/identity/revoke-token
Authorization: Bearer {access-token}
Content-Type: application/json

{
  "refreshToken": "el-refresh-token-a-revocar"
}
```

**Respuesta exitosa**:
```json
{
  "message": "Token revoked successfully"
}
```

**Notas**:
- Requiere autenticación con Bearer token válido
- Útil para implementar "Logout" o "Cerrar sesión en todos los dispositivos"

## Flujo de uso recomendado

### 1. Login inicial
```javascript
// Login
const loginResponse = await fetch('/v1/identity/authentication', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'usuario@ejemplo.com',
    password: 'contraseña'
  })
});

const { accessToken, refreshToken, expiresAt } = await loginResponse.json();

// Guardar tokens de forma segura
localStorage.setItem('accessToken', accessToken);
localStorage.setItem('refreshToken', refreshToken);
localStorage.setItem('tokenExpiry', expiresAt);
```

### 2. Hacer peticiones con Access Token
```javascript
const response = await fetch('/v1/products', {
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
  }
});
```

### 3. Manejar token expirado y renovarlo
```javascript
async function fetchWithTokenRefresh(url, options = {}) {
  // Intentar petición con token actual
  let response = await fetch(url, {
    ...options,
    headers: {
      ...options.headers,
      'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
    }
  });

  // Si retorna 401, intentar renovar token
  if (response.status === 401) {
    const refreshResponse = await fetch('/v1/identity/refresh-token', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        refreshToken: localStorage.getItem('refreshToken')
      })
    });

    if (refreshResponse.ok) {
      const { accessToken, refreshToken, expiresAt } = await refreshResponse.json();

      // Actualizar tokens
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      localStorage.setItem('tokenExpiry', expiresAt);

      // Reintentar petición original con nuevo token
      response = await fetch(url, {
        ...options,
        headers: {
          ...options.headers,
          'Authorization': `Bearer ${accessToken}`
        }
      });
    } else {
      // Refresh token inválido, redirigir a login
      window.location.href = '/login';
    }
  }

  return response;
}
```

### 4. Logout (Revocar token)
```javascript
async function logout() {
  await fetch('/v1/identity/revoke-token', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${localStorage.getItem('accessToken')}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      refreshToken: localStorage.getItem('refreshToken')
    })
  });

  // Limpiar tokens locales
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('tokenExpiry');

  // Redirigir a login
  window.location.href = '/login';
}
```

## Seguridad

### Características implementadas:
1. **Tokens criptográficamente seguros**: Los refresh tokens se generan usando `RandomNumberGenerator` de 64 bytes
2. **Expiración automática**: Los refresh tokens expiran después de 7 días
3. **Rotación de tokens**: Cada vez que se renueva, se genera un nuevo refresh token
4. **Revocación**: Los tokens antiguos se revocan al renovar
5. **Auditoría de IP**: Se registra la IP de creación y revocación
6. **Trazabilidad**: Se puede rastrear qué token reemplazó a cuál

### Mejores prácticas:
- Almacenar tokens de forma segura (evitar XSS)
- Usar HTTPS en producción
- Implementar rate limiting en endpoints de autenticación
- Limpiar tokens expirados periódicamente con `CleanupExpiredTokensAsync()`
- Considerar implementar HttpOnly cookies para refresh tokens en lugar de localStorage

## Próximos pasos opcionales

1. **Background job para limpieza**: Implementar un job que ejecute `CleanupExpiredTokensAsync()` diariamente
2. **Límite de tokens activos**: Limitar cantidad de tokens activos por usuario
3. **Notificaciones**: Enviar email cuando se genera un nuevo token desde una IP desconocida
4. **HttpOnly Cookies**: Mover refresh tokens a cookies HttpOnly para mayor seguridad
5. **Refresh Token en Redis**: Cachear tokens activos en Redis para mejor rendimiento

## Pruebas

Para probar la funcionalidad:

1. Ejecutar el servicio Identity:
```bash
dotnet run --project src/Services/Identity/Identity.Api
```

2. Usar Swagger UI en: `http://localhost:{puerto}/swagger`

3. O usar curl:
```bash
# Login
curl -X POST http://localhost:5001/v1/identity/authentication \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"password123"}'

# Refresh Token
curl -X POST http://localhost:5001/v1/identity/refresh-token \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"tu-refresh-token"}'

# Revoke Token
curl -X POST http://localhost:5001/v1/identity/revoke-token \
  -H "Authorization: Bearer tu-access-token" \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"tu-refresh-token"}'
```

## Archivos modificados/creados

### Nuevos archivos:
- `Identity.Domain/RefreshToken.cs`
- `Identity.Persistence.Database/Configuration/RefreshTokenConfiguration.cs`
- `Identity.Service.EventHandlers/Services/IRefreshTokenService.cs`
- `Identity.Service.EventHandlers/Services/RefreshTokenService.cs`
- `Identity.Service.EventHandlers/Commands/RefreshTokenCommand.cs`
- `Identity.Service.EventHandlers/Commands/RevokeTokenCommand.cs`
- `Identity.Service.EventHandlers/RefreshTokenEventHandler.cs`
- `Identity.Service.EventHandlers/RevokeTokenEventHandler.cs`
- Migración: `{timestamp}_AddRefreshTokens.cs`

### Archivos modificados:
- `Identity.Domain/ApplicationUser.cs` - Agregada relación con RefreshTokens
- `Identity.Persistence.Database/ApplicationDbContext.cs` - Agregado DbSet y configuración
- `Identity.Service.EventHandlers/Responses/IdentityAccess.cs` - Agregados RefreshToken y ExpiresAt
- `Identity.Service.EventHandlers/Commands/UserLoginCommand.cs` - Agregado IpAddress
- `Identity.Service.EventHandlers/UserLoginEventHandler.cs` - Generación de refresh token
- `Identity.Api/Controllers/IdentityController.cs` - Nuevos endpoints
- `Identity.Api/Startup.cs` - Registro de RefreshTokenService

## Conclusión

El sistema de Refresh Tokens está completamente implementado y funcional. Ahora los usuarios pueden:
- Recibir un refresh token al hacer login
- Renovar su access token sin volver a ingresar credenciales
- Revocar tokens para cerrar sesión de forma segura
- Disfrutar de una experiencia de usuario mejorada con sesiones más largas y seguras
