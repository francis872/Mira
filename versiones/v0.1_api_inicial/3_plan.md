# Plan Técnico v0.1 — Módulo de Investigación

**Fuente:** Plan de desarrollo Módulo de Investigación (v1.0)  
**Fecha:** Agosto 2026  
**Responsables:** Juan José Grisales Sañudo, Francisco Guisado López

---

## 1. Stack Tecnológico (Propuesto en Plan de Desarrollo)

| Componente | Tecnología | Justificación |
|-----------|-----------|---------------|
| **Backend** | ASP.NET Core 8.0 (C#) | Framework robusto, type-safe, escalable. |
| **Base de Datos** | PostgreSQL 15+ | Relativamente confiable, ACID, JSON support. |
| **Autenticación** | JWT (System.IdentityModel.Tokens.Jwt) | Stateless, seguro, standard en APIs REST. |
| **Cifrado Contraseñas** | bcrypt (BCrypt.Net-Next) | Algoritmo estándar, resistente a ataques. |
| **Validación** | FluentValidation | Reglas centralizadas, reutilizables. |
| **ORM** | Entity Framework Core 8 | Migraciones automáticas, queries compiladas. |
| **Logging** | Serilog + Seq | Structured logging, trazabilidad. |
| **API Doc** | Swagger/OpenAPI 3.0 | Documentación auto-generada. |
| **Containerización** | Docker + docker-compose | Reproducibilidad, facilita CI/CD. |
| **Testing** | xUnit + Moq | Test unitarios, mocks. |

---

## 2. Arquitectura Limpia (Clean Architecture)

```
MIRA.API/
├── src/
│   ├── MIRA.API/
│   │   ├── Controllers/        # Entrada HTTP
│   │   ├── Middleware/         # JWT, error handling
│   │   ├── Program.cs          # DI, config
│   │   └── appsettings.json    # Env vars
│   ├── MIRA.Application/       # Use cases (Services)
│   │   ├── Services/           # Business logic
│   │   ├── DTOs/               # Request/Response
│   │   ├── Validators/         # FluentValidation rules
│   │   └── Interfaces/         # Contracts
│   ├── MIRA.Domain/            # Entidades & Rules
│   │   ├── Entities/           # POCO models
│   │   ├── ValueObjects/       # DDD value objects
│   │   ├── Events/             # Domain events
│   │   └── Interfaces/         # Repository contracts
│   └── MIRA.Infrastructure/    # Persistencia & Externos
│       ├── Persistence/        # EF Core DbContext
│       ├── Repositories/       # EF Core implementations
│       └── Configurations/     # Entity mappings
└── tests/
    ├── MIRA.Tests.Unit/        # Unit tests
    └── MIRA.Tests.Integration/ # Integration tests
```

**Patrón:** Dependency Injection en Program.cs, inyección de repos en services, inyección de services en controllers.

---

## 3. Estrategia de Base de Datos

### 3.1 Migraciones
- **Tool:** Entity Framework Core Migrations
- **Workflow:** 
  1. Crear entidad en `Domain/Entities`
  2. Configurar en `Infrastructure/Configurations`
  3. Registrar en `DbContext`
  4. `dotnet ef migrations add <NombreMigracion>`
  5. `dotnet ef database update`

### 3.2 Seed Data
- Script SQL en `Infrastructure/Persistence/Scripts/` para datos iniciales (roles, ODS, etc.)
- Ejecutado automáticamente en `Program.cs` si BD está vacía.

### 3.3 Transacciones
- Usar `using var transaction = await context.Database.BeginTransactionAsync()`
- Rollback automático en exception.
- ACID compliance en todas las operaciones multi-tabla.

### 3.4 Soft Deletes
- Columna `deleted_at` en tablas críticas.
- Queries automáticamente filtran `deleted_at IS NULL`.
- Implementado vía EF Core Global Query Filters.

---

## 4. Estrategia de Docker & Despliegue

### 4.1 Contenedores Locales
```yaml
services:
  api:
    image: mira-api:0.1
    ports:
      - "5000:5000"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=mira_v01;User Id=mira_user;Password=...
    depends_on:
      - postgres

  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: mira_v01
      POSTGRES_USER: mira_user
      POSTGRES_PASSWORD: ...
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./database/init/init.sql:/docker-entrypoint-initdb.d/01-init.sql

volumes:
  postgres_data:
```

### 4.2 Build Multi-Stage
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY MIRA.sln .
COPY src/ src/
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
CMD ["dotnet", "MIRA.API.dll"]
```

---

## 5. Estrategia de Validación

### 5.1 Nivel API
- Validación de entrada en Controllers usando `ModelState.IsValid`
- Respuesta 400 Bad Request con errores estructurados.

### 5.2 Nivel Aplicación
- FluentValidation rules en `Application/Validators/`
- Validators inyectados en Services
- Ejemplo: `CreateGrupoValidator` valida GrupLAC URL, sede_id existente, etc.

### 5.3 Nivel Base de Datos
- Constraints SQL: UNIQUE, NOT NULL, FOREIGN KEY.
- Check constraints para enums (ej. ODS category IN ('social', 'economica', 'ambiental')).

---

## 6. Estrategia de Seguridad

### 6.1 Autenticación JWT
- Token emitido por `/api/auth/login`
- Payload: `{"sub": user_id, "roles": [...], "exp": timestamp}`
- Validación en Middleware: extrae token del header `Authorization: Bearer <token>`
- Expiración: 24 horas.

### 6.2 Autorización por Rol
- Atributo `[Authorize(Roles = "Admin,Coordinador")]` en controllers.
- Policy-based auth para reglas complejas (ej. "solo puedo editar mi grupo").

### 6.3 Input Validation
- Sanitización de strings: `HtmlEncoder.Default.Encode(input)`
- Validación de URLs: regex + prueba de conexión (futuro).
- Parameterización automática en EF Core (previene SQL injection).

---

## 7. Estrategia de Testing

### 7.1 Unit Tests (xUnit + Moq)
- 1 test file por Service (`GrupoServiceTests.cs`)
- Mocks de repositorios
- Cobertura objetivo: ≥ 80%

### 7.2 Integration Tests
- Test database en memoria o contenedor PostgreSQL temporal
- Valida flujo completo: Controller → Service → Repository → BD
- Ejecutados en CI/CD antes de merge a develop

### 7.3 Test Data Builders
- Clases helper para crear entidades de prueba rápidamente
- Reutilizables en múltiples tests

---

## 8. Dependencias Principales (NuGet)

```xml
<!-- Identity & Auth -->
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />
<PackageReference Include="Microsoft.IdentityModel.Protocols.OpenIdConnect" Version="7.0.0" />

<!-- Crypto -->
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />

<!-- Validation -->
<PackageReference Include="FluentValidation" Version="11.8.1" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.8.1" />

<!-- Data Access -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Npgsql" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />

<!-- Logging -->
<PackageReference Include="Serilog" Version="3.1.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Seq" Version="6.0.0" />

<!-- Documentation -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />

<!-- Testing -->
<PackageReference Include="xunit" Version="2.6.6" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
```

---

## 9. Gestión de Configuración

### Ambiente Local (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mira_v01;User Id=mira_user;Password=localdev123"
  },
  "Jwt": {
    "Key": "dev-key-min-32-chars-para-testing-local",
    "Issuer": "mira-api-dev",
    "Audience": "mira-frontend-dev",
    "ExpirationMinutes": 1440
  },
  "Logging": {
    "LogLevel": { "Default": "Information" }
  }
}
```

### Variables de Entorno (docker-compose .env)
```env
POSTGRES_USER=mira_user
POSTGRES_PASSWORD=secure_dev_password_123
POSTGRES_DB=mira_v01
JWT_KEY=dev-key-min-32-chars-para-testing-local
```

---

## 10. Hitos del Plan Técnico

| Sprint | Hito | Entregable |
|--------|------|------------|
| **3-4** | Setup infraestructura | Docker-compose, EF Core DbContext, auth middleware |
| **5-6** | Implementar repos & services | CRUD repositories, business logic services |
| **7-8** | Implementar controllers | Endpoints REST, validación, error handling |
| **9-10** | Testing & documentación | Unit tests ≥80%, Swagger docs, manual de dev |
| **11-12** | QA & refinamiento | Bug fixes, optimización, release candidate |

---

**Generado:** 2026-09-04  
**Versión:** 0.1  
**Estado:** Aprobado para Sprint 3

## Validation strategy

Template pending completion from official source documents.
