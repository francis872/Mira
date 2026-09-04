# Investigación Técnica & Decisiones (ADR v0.1)

**Fuente:** Plan de Desarrollo Módulo de Investigación (v1.0)  
**Formato:** ADR (Architecture Decision Record)  
**Fecha:** Agosto 2026  
**Audiencia:** Equipo técnico, arquitectura, stakeholders  

---

## D1 — Seleccionar ASP.NET Core 8.0 como Backend

### Contexto
- Plan de desarrollo especifica: "Arquitectura moderna, escalable, basada en capas"
- Universidad requiere aplicación web API-first
- 3 tecnólogos definieron stack (C#/.NET, PostgreSQL, Windows)
- Necesidad de mantenibilidad a largo plazo (módulos adicionales previstos)

### Decisión
**ASP.NET Core 8.0 (C#)** como framework para la API REST centralizada.

### Alternativas Evaluadas
1. **Node.js (Express/NestJS)** - Más rápido prototipado pero débil en type-safety y escalabilidad
2. **Java (Spring Boot)** - Robusto pero overhead de infraestructura
3. **Python (FastAPI)** - Simple pero limitado en performance para múltiples módulos

### Justificación
- ✅ Type-safe: compilación detecta errores antes de runtime
- ✅ Performance: compilado, nativo en Windows
- ✅ Arquitectura limpia: soporte nativo para DI, middleware, validación
- ✅ Escalable: soporta crecimiento de módulos futuros (Desempeño, Desarrollo)
- ✅ Ecosistema: EntityFramework Core (ORM), Serilog (logging), FluentValidation

### Consecuencias
- ⚠️ Requiere licencia Visual Studio Professional (mitigado: VS Code + OmniBridge)
- ⚠️ Curva de aprendizaje moderada (C# desconocido = 2-3 semanas ramp-up)
- ✅ Consistencia: misma tecnología para todos los módulos futuros

**Fuente en Plan:** "Tecnologías Propuestas: ASP.NET Core, PostgreSQL, Windows"

---

## D2 — PostgreSQL 15+ como Base de Datos

### Contexto
- Plan propone inicialmente Oracle DB (futuro)
- Pero especifica PostgreSQL para v0.1 por costo y disponibilidad
- Necesidad de transacciones ACID, integridad referencial
- Datos académicos requieren confiabilidad

### Decisión
**PostgreSQL 15+** como BD relacional para v0.1. Migración a Oracle en v0.2+.

### Alternativas Evaluadas
1. **SQL Server** - Caro, requiere licencia
2. **MySQL** - Débil en soporte de enumerados y JSON
3. **Oracle** - Complejo, sobrecarga para v0.1, costo

### Justificación
- ✅ Open-source: costo operativo $0
- ✅ ACID compliance: transacciones confiables
- ✅ JSON support: flexible para futuros catálogos complejos
- ✅ Enumerados (ENUM): tipo nativo para estado, categoría
- ✅ Replicación: nativa, sin costo adicional
- ✅ EF Core nativo: Npgsql provider completo
- ✅ Docker: imagen oficial postgres:15-alpine disponible

### Consecuencias
- Migraciones a Oracle en v0.2 requerirán conversión de queries y tipos SQL
- Soft deletes implementados como patrón (no tabla de auditoría nativa Oracle)
- Equipo debe aprender Postgres específicamente (no generalista DB)

**Fuente en Plan:** "Base de Datos: PostgreSQL (inicialmente), Oracle (futuro)"

---

## D3 — Clean Architecture con 3 Capas (Controllers → Services → Repositories)

### Contexto
- Plan especifica: "Arquitectura de capas modular y mantenible"
- Necesidad de separación de responsabilidades
- Futuro: múltiples módulos (Desempeño, Desarrollo) compartiendo repositorio
- Equipo requiere código predecible y testeable

### Decisión
**Clean Architecture** con capas explícitas:
- 🔴 Presentation (Controllers, Middleware)
- 🟡 Application (Services, DTOs, Validators)
- 🟢 Domain (Entities, ValueObjects)
- 🔵 Infrastructure (Repositories, DbContext)

### Alternativas Evaluadas
1. **Monolithic MVC** - Acoplado, difícil de testear
2. **Microservicios (v0.1)** - Sobrecarga de orquestación para MVP
3. **Layered (2 capas)** - Insuficiente para complejidad futura

### Justificación
- ✅ Testeable: servicios con repos mockeados
- ✅ Modular: nuevas features aisladas (HU-1 no afecta HU-10)
- ✅ Mantenible: reglas de negocio centralizadas en Services
- ✅ Escalable: próximos módulos reutilizan repositories y domain
- ✅ Type-safe: inyección de dependencias fuerza contratos claros

### Consecuencias
- ⚠️ Más clases/interfaces inicialmente (18 controllers, 20 services, 13 repos)
- ⚠️ Disciplina de equipo requerida (no shortcuts directo a DB)
- ✅ Reutilización de componentes en módulos futuros

**Fuente en Plan:** "Gestión del Proceso: Uso de patrones de arquitectura limpia"

---

## D4 — JWT (JSON Web Tokens) para Autenticación Stateless

### Contexto
- Módulo es API-first (sin sesiones server-side)
- Necesidad de autenticación en 9+ endpoints
- Futuro: mobile app + múltiples frontends (requerido stateless)
- Plan: "Sistema robusto de autenticación"

### Decisión
**JWT (JSON Web Tokens)** con expiración de 24 horas y roles embedded.

### Alternativas Evaluadas
1. **Sesiones server-side (cookies)** - Incompatible con mobile/SPA
2. **OAuth 2.0/OpenID Connect** - Sobrecarga para v0.1
3. **API Keys** - Inseguro, no escalable para usuarios individuales

### Justificación
- ✅ Stateless: escalable horizontalmente sin sincronización
- ✅ Móvil-compatible: token en header Authorization, sin cookies
- ✅ Seguro: firmado criptográficamente (HMAC SHA-256)
- ✅ Roles embedded: no requiere lookup de BD en cada request
- ✅ Standard: OpenAPI/Swagger soporta Bearer
- ✅ Expiración: 24h mitiga riesgo de token comprometido

### Consecuencias
- ⚠️ Token revocation requiere blacklist en Redis (futuro)
- ⚠️ Si usuario pierde token, no hay "olvidar sesión" trivial
- ✅ Compatible con Single Sign-On (futuro LDAP/SAML)

**Fuente en Plan:** "Seguridad: Autenticación JWT y control de roles"

---

## D5 — bcrypt/Argon2 para Hash de Contraseñas

### Contexto
- Datos de 1000+ usuarios (futura escala)
- Seguridad es RNF-1 (crítica)
- Plan: "Cifrado de contraseñas con bcrypt/argon2"
- Cumplimiento: OWASP Top 10

### Decisión
**bcrypt (NuGet: BCrypt.Net-Next)** para hashing de contraseñas. Argon2 como alternativa futura.

### Alternativas Evaluadas
1. **MD5/SHA-1** - Quebrado, vulnerable a rainbow tables
2. **SHA-256 puro** - Rápido (indeseado para contraseñas)
3. **PBKDF2** - OK pero menos resistente a GPU cracking que bcrypt/Argon2

### Justificación
- ✅ Adaptable: costo computacional aumenta con tiempo (resistente a futuros ataques)
- ✅ Salt automático: cada hash único incluso para mismo password
- ✅ Lento deliberadamente: 10^12 operaciones/segundo máximo
- ✅ Standard: usado por Facebook, Uber, etc.
- ✅ Librería activa: BCrypt.Net-Next tiene 40M+ descargas

### Consecuencias
- ⚠️ Login requiere 100-200ms (aceptable vs riesgo de seguridad)
- ✅ Migración a Argon2 posterior es transparente para usuarios

**Fuente en Plan:** "Seguridad: Cifrado de contraseñas"

---

## D6 — Entity Framework Core como ORM

### Contexto
- 13 tablas con relaciones N:M complejas (grupo_docentes, linea_disciplinas, etc.)
- Migraciones de BD requeridas durante desarrollo
- Integración con C# type-safe
- Reutilización en módulos futuros

### Decisión
**Entity Framework Core 8.0** con Npgsql provider para PostgreSQL.

### Alternativas Evaluadas
1. **Dapper** - SQL puro, rápido pero manual
2. **NHibernate** - Completo pero overhead para v0.1
3. **Raw SQL** - Riesgoso, vulnerable a SQL injection

### Justificación
- ✅ Migrations: `dotnet ef migrations add` automatiza schema changes
- ✅ Query compiladas: compilación en tiempo de compilación detect errores
- ✅ Relaciones: lazy loading, eager loading, select-incluye automático
- ✅ Type-safe: `context.Usuarios.Where(u => u.Email == email)` vs `SELECT * FROM usuarios WHERE email = ?`
- ✅ SQL parameterizado: previene SQL injection automáticamente
- ✅ Soft deletes: Global Query Filters para `deleted_at IS NULL`

### Consecuencias
- ⚠️ Performance tuning requerido para reportes complejos (HU-10)
- ⚠️ N+1 query problem si no se maneja eager loading bien
- ✅ Equipo puede enfocarse en lógica, no SQL syntax

**Fuente en Plan:** "Arquitectura: ORM para persistencia"

---

## D7 — Docker + docker-compose para Desarrollo Local

### Contexto
- Plan especifica: Windows como OS
- Necesidad de entorno reproducible (dev ≈ prod)
- 2 miembros equipo, diferentes laptops
- PostgreSQL requiere configuración (puerto 5432 ocupado en Windows)

### Decisión
**Docker + docker-compose** con:
- `mira-api:0.1` (Dockerfile multi-stage)
- `postgres:15-alpine` servicio
- PostgreSQL en puerto 5544 (evita conflicto con native service)

### Alternativas Evaluadas
1. **Local PostgreSQL install** - Requiere configuración manual, diferencias entre máquinas
2. **Azure SQL Database** - Costo, internet requerido siempre
3. **SQLite** - Incompatible con features PostgreSQL (ENUM, JSON, etc.)

### Justificación
- ✅ Reproducible: `docker-compose up` funciona en Windows, Mac, Linux
- ✅ Aislamiento: no interfiere con otros servicios
- ✅ Portabilidad: deployar a staging/prod es `docker build + push`
- ✅ Escalable: próximas dependencias (Redis, RabbitMQ) se añaden sin fricción
- ✅ CI/CD: GitHub Actions puede ejecutar containers directamente

### Consecuencias
- ⚠️ Docker Desktop requerido (instalación inicial)
- ⚠️ Overhead de memoria (PostgreSQL container ~500MB)
- ✅ Familiaridad con Docker es ventaja competitiva

**Fuente en Plan:** "Despliegue: Docker para entornos locales"

---

## D8 — FluentValidation para Reglas de Negocio

### Contexto
- 25+ requisitos funcionales con validaciones complejas
- Ejemplo: "un único Líder de Grupo activo en período" (RF-5.3)
- Validaciones en 3 niveles: API, Service, BD
- Necesidad de mensajes claros de error

### Decisión
**FluentValidation** con validadores centralizados:
- `CreateUsuarioValidator`: email único, password min 8 chars
- `CreateGrupoValidator`: sede existe, GrupLAC URL válida
- `UpdateLineaValidator`: 1+ ODS y 1+ Área del Conocimiento

### Alternativas Evaluadas
1. **Data Annotations** - Limitado a validaciones simples
2. **Custom attributes** - Code scattered, difícil de reutilizar
3. **Validation en Service** - Sin patrón, inconsistente

### Justificación
- ✅ Reutilizable: mismo validator en tests y API
- ✅ Expresivo: fluent API (`RuleFor(x => x.Email).EmailAddress().WithMessage(...)`)
- ✅ Composable: validadores anidados para relaciones N:M
- ✅ Testeable: validadores independientes de HTTP context
- ✅ Centralizado: reglas de negocio en un lugar

### Consecuencias
- Mantenimiento: si regla cambia, update 1 validator
- ⚠️ Validaciones asincrónicas (ej. "email único") requieren async/await

**Fuente en Plan:** "Validación: Reglas centralizadas en services"

---

## D9 — Serilog para Structured Logging

### Contexto
- Plan: "Auditoría y trazas de cambios"
- Necesidad de debugging en production (sin acceso directo)
- 10+ tareas de negocio (crear usuario, vincular docente, etc.)
- Future: dashboards de monitoreo (Azure Monitor, etc.)

### Decisión
**Serilog** con structured logging en JSON y envío a Seq (development).

### Alternativas Evaluadas
1. **Console.WriteLine** - No escalable, imposible de buscar
2. **log4net** - Antiguo, configuración XML compleja
3. **NLog** - OK pero menos popular que Serilog en ASP.NET Core

### Justificación
- ✅ Structured: logs en JSON con propiedades queryables
- ✅ Context: automático incluye timestamp, trace ID, usuario
- ✅ Performance: asincrónico, no bloquea HTTP requests
- ✅ Sinks: Console (dev), Seq (staging), File/CloudWatch (prod)
- ✅ Integration: ASP.NET Core logging compatibile

### Consecuencias
- ⚠️ Cuidado con datos sensibles (no loguear passwords, tokens)
- ✅ Debugging 10x más fácil (buscar por usuario, endpoint, error)

**Fuente en Plan:** "Logging y auditoría para trazabilidad"

---

## D10 — Swagger/OpenAPI para Documentación de API

### Contexto
- 25+ endpoints de API (ver 6_contracts.md)
- Frontend y posibles integradores externos
- Plan: "API documentada y autodescriptiva"
- Necesidad de mantener docs en sync con código

### Decisión
**Swagger UI (Swashbuckle.AspNetCore)** con OpenAPI 3.0 spec auto-generado.

### Alternativas Evaluadas
1. **Documentación manual (Word)** - Desincronización rápida
2. **Postman collection** - Mejor pero requiere mantenimiento manual
3. **GraphQL** - Overkill para v0.1 REST API

### Justificación
- ✅ Auto-generado: mismo código = docs siempre actualizadas
- ✅ Interactive: endpoint tester en Swagger UI
- ✅ Standard: herramientas (postman, codegen) entienden OpenAPI
- ✅ Descriptivo: incluye tipos, status codes, ejemplos
- ✅ Developer experience: developers descubren API sin onboarding

### Consecuencias
- ⚠️ Requiere discipline: comentarios XML en code
- ✅ Reducción de preguntas sobre API

**Fuente en Plan:** "Documentación: API autodocumentada"

---

## D11 — Soft Deletes (deleted_at) en Lugar de Hard Delete

### Contexto
- Plan: "Historial de cambios sin pérdida de datos"
- Datos académicos son legales (auditoría requerida)
- Ejemplo: usuario vinculado a grupo y semillero, luego "eliminado"
- Referencias integrales: no se pueden hard-delete si existen referencias

### Decisión
**Soft deletes** mediante columna `deleted_at TIMESTAMP NULL`:
- Hard delete NUNCA
- "Delete" = `UPDATE tabla SET deleted_at = NOW()`
- Queries automáticamente filtran `WHERE deleted_at IS NULL`

### Alternativas Evaluadas
1. **Hard delete** - Perderíamos historial, violaría auditoría
2. **Archive table** - Complejidad innecesaria
3. **Audit log + hard delete** - Más tables, más sincronización

### Justificación
- ✅ Auditoría: recuperar datos históricos para investigación
- ✅ Integridad: referencias a usuarios "eliminados" siguen válidas
- ✅ Reversible: set `deleted_at = NULL` si es un error
- ✅ Simple: global query filters ocultan complejidad
- ✅ Performance: no requiere joins a audit table

### Consecuencias
- ⚠️ Nuevos desarrolladores deben recordar soft deletes (documentado)
- ⚠️ UNIQUE constraints no funcionan con deleted_at (usar partial indexes en Postgres)

**Fuente en Plan:** "Auditoría: Trazabilidad sin pérdida"

---

## D12 — Testing: xUnit + Moq para Unit & Integration Tests

### Contexto
- DoD (Definition of Done): "Tests ≥ 80% coverage"
- Necesidad de CI/CD pipeline confiable
- 28 historias = ~60 features = ~100+ unit tests mínimo
- Equipo no tiene experiencia con testing (ramp-up requerido)

### Decisión
**xUnit** framework + **Moq** library para mocking.

### Alternativas Evaluadas
1. **MSTest** - Microsoft official pero menos popular
2. **NUnit** - Legacy, menos enfoque en .NET Core
3. **Moq vs Rhino Mocks vs FakeItEasy** - Moq es estándar de facto

### Justificación
- ✅ xUnit: diseñado para .NET Core, limpio syntax
- ✅ Moq: fluent API (`Mock<IRepository>().Setup(...)`)
- ✅ Performance: tests ejecutan en segundos (no minutos)
- ✅ Integration: GitHub Actions soporta xUnit nativamente
- ✅ Community: millones de ejemplos en StackOverflow

### Consecuencias
- ⚠️ Aprendizaje: equipo requiere 2-3 semanas de ramp-up
- ✅ Testing cultura establecida desde v0.1

**Fuente en Plan:** "QA: Cobertura de tests ≥ 80%"

---

## Decisiones Diferidas (v0.2+)

| Decisión | v0.1 | v0.2+ | Razón |
|----------|------|-------|-------|
| Auditoría completa | Soft deletes | Tabla audit_log | Complejidad post-MVP |
| Caché | N/A | Redis | Performance tuning posterior |
| Búsqueda full-text | Simple filtro | Elasticsearch | Catálogos pequeños v0.1 |
| Notificaciones | N/A | Email/SMS | Orquestación post-MVP |
| LDAP/SAML SSO | JWT local | Entra ID | Integración corporativa posterior |
| Oracle migration | PostgreSQL | Oracle DB | Plan de desarrollo futuro |

---

**Generado:** 2026-09-04  
**Versión:** 0.1  
**Estado:** Aprobado para Sprint 3
