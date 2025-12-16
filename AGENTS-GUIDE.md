# Guía de Agentes Especializados

Este proyecto cuenta con **8 agentes especializados** que puedes usar dependiendo de la tarea que necesites realizar.

## 🔄 Cómo Cambiar de Agente

### Método 1: Atajo de Teclado
- Presiona **Ctrl+P** para abrir el menú de comandos
- Escribe "Switch Agent" o "Cambiar Agente"
- Selecciona el agente que desees usar

### Método 2: Tab (si está configurado)
- Algunos usuarios pueden tener configurado Tab para cambiar de agente
- Verifica tu configuración de OpenCode

---

## 👥 Agentes Disponibles

### 1. 🔧 DotNet Backend Expert
**Cuándo usarlo:**
- Desarrollar nuevas funcionalidades en microservicios
- Implementar patrones CQRS con MediatR
- Trabajar con Entity Framework Core
- Aplicar principios SOLID y Clean Architecture
- Desarrollar APIs RESTful

**Expertise:**
- .NET 9 Development
- Entity Framework Core
- Microservices Architecture
- Clean Architecture
- Design Patterns (Repository, Unit of Work, Factory, Strategy)

**Ejemplo de uso:**
```
"Necesito implementar un nuevo endpoint para actualizar el estado de una orden"
"Ayúdame a refactorizar este servicio siguiendo Clean Architecture"
```

---

### 2. 🗄️ Database Expert
**Cuándo usarlo:**
- Crear o modificar migraciones de Entity Framework
- Optimizar consultas lentas
- Diseñar índices para mejorar rendimiento
- Implementar Full-Text Search
- Resolver problemas de N+1 queries

**Expertise:**
- SQL Server Administration
- Entity Framework Core Migrations
- Query Optimization
- Indexing Strategies
- Full-Text Search

**Ejemplo de uso:**
```
"Esta consulta es muy lenta, ayúdame a optimizarla"
"Necesito crear una migración para agregar full-text search"
"Ayúdame a diseñar índices para la tabla Products"
```

**Referencias:**
- DATABASE_SCHEMA.md
- FULLTEXT-SEARCH-SETUP.md

---

### 3. 🌐 API Gateway Specialist
**Cuándo usarlo:**
- Configurar nuevas rutas en Ocelot
- Implementar rate limiting
- Configurar load balancing
- Resolver problemas de routing
- Configurar CORS

**Expertise:**
- Ocelot Gateway Configuration
- API Routing
- Rate Limiting
- Load Balancing
- Service Discovery

**Ejemplo de uso:**
```
"Necesito agregar una nueva ruta para el servicio de Payment"
"Configura rate limiting para proteger el endpoint de búsqueda"
"Ayúdame a implementar load balancing para el servicio de Catalog"
```

**Referencias:**
- API-ROUTES-ANALYSIS.md
- RATE-LIMITING-GUIDE.md

---

### 4. 🔒 Security Expert
**Cuándo usarlo:**
- Implementar autenticación JWT
- Configurar políticas de autorización
- Implementar refresh tokens
- Validar inputs del usuario
- Configurar CORS de manera segura

**Expertise:**
- JWT Authentication
- Role-Based Authorization
- Policy-Based Authorization
- API Key Management
- Input Validation

**Ejemplo de uso:**
```
"Necesito implementar refresh tokens para el servicio de Identity"
"Ayúdame a crear una política de autorización para administradores"
"Configura validación de inputs con FluentValidation"
```

**Referencias:**
- REFRESH-TOKENS-GUIDE.md
- Common.Validation (FluentValidation)

---

### 5. ⚡ Performance & Caching Expert
**Cuándo usarlo:**
- Implementar caché con Redis
- Optimizar rendimiento de endpoints
- Resolver problemas de caché
- Implementar estrategias de invalidación
- Optimizar uso de async/await

**Expertise:**
- Redis Caching
- Distributed Caching
- Cache Invalidation
- Performance Profiling
- Async/Await Optimization

**Ejemplo de uso:**
```
"Implementa caché Redis para el catálogo de productos"
"Este endpoint es muy lento, ayúdame a optimizarlo"
"La invalidación de caché no está funcionando correctamente"
```

**Referencias:**
- REDIS-SETUP.md
- CACHE-TROUBLESHOOTING.md
- CACHE-DISABLE-GUIDE.md

---

### 6. 🐳 DevOps & Docker Expert
**Cuándo usarlo:**
- Modificar docker-compose.yml
- Optimizar Dockerfiles
- Configurar health checks
- Resolver problemas de networking en Docker
- Configurar volúmenes y persistencia

**Expertise:**
- Docker Container Management
- Docker Compose Orchestration
- Multi-Stage Builds
- Container Networking
- Health Checks

**Ejemplo de uso:**
```
"Ayúdame a optimizar el Dockerfile del servicio de Catalog"
"Necesito agregar health checks a todos los servicios"
"Los contenedores no se pueden comunicar entre sí"
```

**Referencias:**
- docker-compose.yml
- INSTALACION_COMPLETADA.md

---

### 7. 🎨 Frontend Integration Specialist
**Cuándo usarlo:**
- Trabajar con Razor Pages
- Implementar búsqueda AJAX
- Integrar frontend con API Gateway
- Validar formularios
- Manejar autenticación en el cliente

**Expertise:**
- Razor Pages
- ASP.NET Core MVC
- AJAX Requests
- Form Validation
- Session Management

**Ejemplo de uso:**
```
"Implementa búsqueda AJAX en el catálogo de productos"
"Ayúdame a validar este formulario client-side y server-side"
"Necesito manejar tokens JWT en el cliente web"
```

**Referencias:**
- AJAX-SEARCH-FIX.md
- Clients.WebClient

---

### 8. ✅ Testing & Quality Assurance
**Cuándo usarlo:**
- Escribir tests unitarios
- Crear integration tests
- Aumentar cobertura de código
- Implementar TDD
- Hacer mocking con Moq

**Expertise:**
- xUnit Testing
- Integration Testing
- Mocking with Moq
- Test-Driven Development (TDD)
- Code Coverage Analysis

**Ejemplo de uso:**
```
"Necesito tests unitarios para el servicio de Order"
"Ayúdame a crear integration tests para el endpoint de productos"
"¿Cómo mockeo este repositorio con Moq?"
```

---

## 💡 Tips de Uso

### 1. **Sé específico sobre el contexto**
En lugar de decir: "Ayúdame con este código"
Di: "Usando el agente Database Expert, ayúdame a optimizar esta consulta de productos que está tardando 2 segundos"

### 2. **Cambia de agente según la tarea**
Si estás trabajando en múltiples aspectos:
1. Usa **Database Expert** para crear la migración
2. Cambia a **DotNet Backend Expert** para implementar el servicio
3. Cambia a **Testing & QA** para escribir los tests

### 3. **Combina agentes para tareas complejas**
Para una nueva funcionalidad completa:
1. **DotNet Backend Expert** - Implementar lógica de negocio
2. **Database Expert** - Optimizar queries y migraciones
3. **Security Expert** - Agregar autorización
4. **Performance Expert** - Implementar caché
5. **Testing & QA** - Escribir tests

### 4. **Usa las referencias de documentación**
Cada agente conoce y hace referencia a documentos específicos del proyecto:
- DATABASE_SCHEMA.md
- API-ROUTES-ANALYSIS.md
- REDIS-SETUP.md
- CACHE-TROUBLESHOOTING.md
- etc.

---

## 🔍 Ejemplos de Flujos Completos

### Ejemplo 1: Agregar nueva funcionalidad de "Descuentos"

**Paso 1:** Usa **Database Expert**
```
"Crea una migración para agregar una tabla Discounts con campos: Id, Code, Percentage, ValidFrom, ValidTo"
```

**Paso 2:** Usa **DotNet Backend Expert**
```
"Implementa el servicio de Discounts siguiendo Clean Architecture con CQRS"
```

**Paso 3:** Usa **API Gateway Specialist**
```
"Agrega las rutas para el servicio de Discounts en el API Gateway"
```

**Paso 4:** Usa **Security Expert**
```
"Protege los endpoints de Discounts para que solo admins puedan crear/modificar"
```

**Paso 5:** Usa **Performance & Caching Expert**
```
"Implementa caché Redis para los descuentos activos"
```

**Paso 6:** Usa **Testing & QA**
```
"Crea tests unitarios e integration tests para el servicio de Discounts"
```

---

### Ejemplo 2: Optimizar búsqueda lenta

**Paso 1:** Usa **Performance & Caching Expert**
```
"Este endpoint de búsqueda de productos tarda 3 segundos, ayúdame a identificar el cuello de botella"
```

**Paso 2:** Usa **Database Expert**
```
"Crea índices apropiados para optimizar la búsqueda full-text"
```

**Paso 3:** Usa **Performance & Caching Expert**
```
"Implementa caché Redis para los resultados de búsqueda más frecuentes"
```

**Paso 4:** Usa **Frontend Integration Specialist**
```
"Implementa debouncing en la búsqueda AJAX para reducir requests"
```

---

## 📚 Documentos de Referencia por Agente

| Agente | Documentos Principales |
|--------|------------------------|
| DotNet Backend Expert | README.md, CHEAT_SHEET.md, MIGRATION_TO_NET9.md |
| Database Expert | DATABASE_SCHEMA.md, FULLTEXT-SEARCH-SETUP.md |
| API Gateway Specialist | API-ROUTES-ANALYSIS.md, RATE-LIMITING-GUIDE.md |
| Security Expert | REFRESH-TOKENS-GUIDE.md, CORRELATION-ID-GUIDE.md |
| Performance & Caching Expert | REDIS-SETUP.md, CACHE-TROUBLESHOOTING.md |
| DevOps & Docker Expert | docker-compose.yml, INSTALACION_COMPLETADA.md |
| Frontend Integration | AJAX-SEARCH-FIX.md, ROUTES-COMPARISON.md |
| Testing & QA | (Crear tests en cada servicio) |

---

## 🎯 Mejores Prácticas

1. **Especialización**: Cada agente está optimizado para su área. Úsalos en su especialidad.

2. **Contexto**: Los agentes tienen acceso a Context7 MCP para consultar mejores prácticas actualizadas.

3. **Workflow**: Cada agente sigue un workflow (beforeCoding, whileCoding, afterCoding) para garantizar calidad.

4. **Documentación**: Los agentes actualizarán documentación relevante automáticamente.

5. **Estándares**: Todos los agentes siguen los mismos estándares de código del proyecto.

---

## 🚀 Cómo Empezar

1. **Ctrl+P** → "Switch Agent"
2. Selecciona el agente apropiado para tu tarea
3. Describe claramente lo que necesitas
4. El agente seguirá su workflow especializado
5. Cambia de agente cuando cambies de contexto

---

## ❓ FAQ

**P: ¿Puedo usar múltiples agentes al mismo tiempo?**
R: No, pero puedes cambiar entre agentes rápidamente con Ctrl+P.

**P: ¿Los agentes recuerdan el contexto anterior?**
R: Sí, el contexto de la conversación se mantiene al cambiar de agente.

**P: ¿Qué agente uso si no estoy seguro?**
R: Usa **DotNet Backend Expert** como agente general, él te puede guiar a otros agentes si es necesario.

**P: ¿Puedo modificar los agentes?**
R: Sí, edita `.claude/agent-config.json` para personalizar capabilities, workflows, etc.

---

**¡Feliz desarrollo con agentes especializados! 🎉**
