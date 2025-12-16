# Project Agents

This project uses a **hybrid configuration** with both **local** and **global agents**, plus **Context7 MCP** integration for up-to-date best practices.

## Configuration

### Agent Locations

The project has agents configured in two locations:

1. **Local Project Agent** (`.opencode/agents/`):
   - `angular-expert.md` - Angular 18+ frontend expert (local to this project)

2. **Global Agents** (`C:\Users\milto\.opencode\agents.json`):
   - `dotnet-expert` - .NET backend development
   - `sqlserver-expert` - SQL Server administration
   - `architect` - Software architecture

3. **Context7 MCP** (`.claude/agent-config.json`):
   - Provides real-time access to latest best practices and documentation
   - Available to all agents automatically

## Available Agents

### 1. angular-expert (LOCAL)
**Location**: `.opencode/agents/angular-expert.md`  
**Focus**: Frontend development with Angular 18+ and Angular Material  
**Specific to this project**: YES - Configured for E-Commerce Microservices APIs

- **Technologies**: Angular 18+, Standalone Components, Signals, RxJS
- **UI Framework**: Angular Material, Material Design 3, Accessibility (a11y)
- **Testing**: Jest + Jasmine/Karma (dual configuration)
- **Integration**: Configured for API Gateway (port 45000)
- **Security**: JWT authentication, Refresh Tokens, Correlation IDs
- **Use for**: Frontend development, UI components, client-side logic, Angular app development

**Key practices**:
- **ALWAYS consults Context7 MCP first** for latest Angular versions and best practices
- Standalone components, Signals for state management
- OnPush change detection, lazy loading, performance optimization
- Reactive forms, RxJS operators, testing with Jest/Jasmine
- Integration with backend microservices APIs

### 2. dotnet-expert (GLOBAL)
**Location**: `C:\Users\milto\.opencode\agents.json`  
**Focus**: Backend development with .NET Core/ASP.NET Core and Entity Framework Core
- .NET Core/ASP.NET Core, Entity Framework Core
- Clean Architecture, CQRS with MediatR, DDD
- Use for: Backend APIs, microservices, business logic, data access

**Key practices**:
- Consults Context7 for latest .NET versions and security patches
- Clean Architecture, CQRS, Repository pattern
- EF Core best practices (AsNoTracking, preventing N+1 queries)
- JWT authentication, FluentValidation, structured logging
- Async/await patterns, dependency injection

### 3. sqlserver-expert (GLOBAL)
**Location**: `C:\Users\milto\.opencode\agents.json`  
**Focus**: SQL Server administration, query optimization, and database design
- SQL Server, T-SQL, indexing strategies
- Query optimization, execution plans, performance tuning
- Use for: Database design, migrations, query optimization, index management

**Key practices**:
- Consults Context7 for latest SQL Server features and optimizations
- Index design (clustered, non-clustered, columnstore, filtered)
- Query optimization (execution plans, SARGable predicates)
- Stored procedures, functions, triggers
- Partitioning, statistics, maintenance plans

### 4. architect (GLOBAL)
**Location**: `C:\Users\milto\.opencode\agents.json`  
**Focus**: Software architecture and system design
- Microservices, Event-Driven Architecture, DDD
- Design patterns, SOLID principles, scalability
- Use for: Architecture decisions, system design, technical leadership

**Key practices**:
- Consults Context7 for modern architectural patterns
- Microservices, monolith modular, event-driven, serverless
- Design patterns (creational, structural, behavioral)
- SOLID, DRY, KISS, YAGNI principles
- Resilience patterns (Circuit Breaker, Retry, Bulkhead)
- Performance, security, scalability considerations

## How to Use

1. **Switch agents**: Press `Ctrl+P` → type "Switch Agent" → select agent
2. **@ Mention**: Type `@agent-name` in your message to invoke specific agent
3. **Context7 Integration**: All agents automatically consult Context7 for latest information

## Agent Workflow

Each agent follows this workflow:

1. **Consult Context7 first**: Get latest best practices, versions, and security updates
2. **Analyze context**: Review existing code, architecture, and project standards
3. **Implement solution**: Apply best practices and patterns
4. **Validate**: Ensure code quality, performance, and security
5. **Document**: Update relevant documentation

## Documentation References

This project has extensive documentation that agents can reference:

- **Backend**: README.md, CHEAT_SHEET.md, DATABASE_SCHEMA.md
- **Database**: DATABASE_SCHEMA.md, FULLTEXT-SEARCH-SETUP.md
- **API Gateway**: API-ROUTES-ANALYSIS.md, RATE-LIMITING-GUIDE.md
- **Security**: REFRESH-TOKENS-GUIDE.md, CORRELATION-ID-GUIDE.md
- **Performance**: REDIS-SETUP.md, CACHE-TROUBLESHOOTING.md
- **DevOps**: docker-compose.yml, INSTALACION_COMPLETADA.md
- **Frontend**: AJAX-SEARCH-FIX.md, ROUTES-COMPARISON.md

## Example Workflows

### Adding a New Feature
1. `@architect` - Design the feature architecture
2. `@sqlserver-expert` - Design database schema and migrations
3. `@dotnet-expert` - Implement backend service with CQRS
4. `@angular-expert` - Create frontend components (if applicable)

### Optimizing Performance
1. `@architect` - Identify architectural bottlenecks
2. `@sqlserver-expert` - Optimize queries and add indexes
3. `@dotnet-expert` - Implement Redis caching, async patterns

### Database Work
1. `@sqlserver-expert` - Design schema, create migrations
2. `@dotnet-expert` - Implement EF Core entities and DbContext
3. `@architect` - Review data access patterns

## Context7 MCP Integration

All agents have access to **Context7 MCP** which provides:
- ✅ Latest versions and features of technologies
- ✅ Current best practices and patterns
- ✅ Security patches and vulnerabilities
- ✅ Performance optimization techniques
- ✅ Deprecation warnings and migration guides

This ensures that all code generated follows the most current standards and practices.

## Notes

- **Local agent** is defined in `.opencode/agents/angular-expert.md` (project-specific)
- **Global agents** are defined in `C:\Users\milto\.opencode\agents.json` (shared across projects)
- Context7 MCP configuration is in `.claude/agent-config.json`
- All agents consult Context7 before providing solutions
- Agents follow project coding standards (PascalCase, camelCase, etc.)
- Documentation is automatically updated when relevant
- To add more local agents, create new `.md` files in `.opencode/agents/`
