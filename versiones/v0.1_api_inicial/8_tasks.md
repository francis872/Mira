# Backlog Técnico v0.1 — Módulo de Investigación

**Fuente:** Cronograma Módulo de Investigación (Excel) + Historias de Usuario  
**Metodología:** Scrum + SDD (Spec-Driven Development)  
**Duración:** Sprint 3-12 (Julio 12 - Noviembre 15, 2026)  
**Equipo:** Juan José Grisales (Backend Lead), Francisco Guisado (DevOps/Frontend Lead)  

---

## Fase 1: Setup Infraestructura (Sprint 3-4, 7-26 julio)

**Responsables:** Francisco Guisado  
**Horas Estimadas:** 38  
**Historias:** Parte de HU-1, HU-2, HU-4

### Task 1.1 - Configurar Docker & docker-compose
- [ ] Dockerfile multi-stage (build + runtime)
- [ ] docker-compose.yml con PostgreSQL + API
- [ ] .env para configuración local
- [ ] Scripts de inicialización BD (seed.sql)
- **Verificación:** `docker-compose up` inicia aplicación + BD sin errores
- **Tiempo:** 6 horas
- **Prioridad:** BLOQUEADOR

### Task 1.2 - Crear Entity Framework Core DbContext
- [ ] DbContext con todas las entidades (usuarios, roles, sedes, grupos, etc.)
- [ ] Configuraciones de mapeos (Entity Fluent API)
- [ ] Global Query Filters para soft deletes
- [ ] Migraciones iniciales
- **Verificación:** `dotnet ef migrations add Initial` y `update` sin errores
- **Tiempo:** 8 horas
- **Prioridad:** BLOQUEADOR

### Task 1.3 - Implementar JWT Authentication Middleware
- [ ] Crear `AuthenticationService` con login
- [ ] Generar JWT con id, roles, exp
- [ ] Middleware de validación de token en requests
- [ ] Inyectar usuario actual en HttpContext
- **Verificación:** POST /auth/login retorna token válido. Request con Bearer token extrae usuario correctamente.
- **Tiempo:** 8 horas
- **Prioridad:** BLOQUEADOR

### Task 1.4 - Setup DI & Dependency Injection
- [ ] Registrar all services en Program.cs
- [ ] Registrar FluentValidation rules
- [ ] Configurar Serilog logging
- [ ] Registrar Swagger/OpenAPI
- **Verificación:** Aplicación inicia sin errores de DI. Swagger accesible en /swagger/ui
- **Tiempo:** 4 horas
- **Prioridad:** BLOQUEADOR

### Task 1.5 - Crear estructura base de carpetas del proyecto
- [ ] Controllers/
- [ ] Application/Services
- [ ] Application/DTOs
- [ ] Application/Validators
- [ ] Domain/Entities
- [ ] Infrastructure/Persistence
- [ ] Infrastructure/Repositories
- **Verificación:** Estructura lista para implementación
- **Tiempo:** 2 horas
- **Prioridad:** ALTA

---

## Fase 2: Implementar HU-1, HU-2, HU-4 (Sprint 3-4, 28 julio - 11 agosto)

**Responsables:** Juan José Grisales  
**Horas Estimadas:** 38  

### HU-1: Gestión de Seguridad & Roles (5 pts, 20h)

#### Task 2.1 - Implementar Repository<Usuario>
- [ ] EF Core repository pattern
- [ ] Métodos: GetByEmail, GetById, Create, Update
- [ ] Incluye usuario_roles en queries
- **Verificación:** CRUD completo funciona en tests
- **Tiempo:** 4 horas
- **Prioridad:** BLOQUEADOR

#### Task 2.2 - Implementar UsuarioService con login
- [ ] Hash de contraseña (bcrypt)
- [ ] Validación credenciales
- [ ] Generación JWT
- [ ] Asignación/revocación de roles
- **Verificación:** `_usuarioService.Login("test@example.com", "Pass123")` retorna token válido
- **Tiempo:** 6 horas
- **Prioridad:** BLOQUEADOR

#### Task 2.3 - Crear UsuarioController (/auth)
- [ ] POST /auth/login
- [ ] POST /auth/logout (opcional v0.1)
- [ ] Error handling 401/400
- **Verificación:** Postman tests pasan. Swagger documenta
- **Tiempo:** 3 horas
- **Prioridad:** ALTA

#### Task 2.4 - Implementar RoleService & RoleController
- [ ] CRUD de roles
- [ ] Validaciones (nombre único)
- [ ] POST /roles/{roleId}/usuarios/{usuarioId} para asignar
- **Verificación:** Roles CRUD completo. Asignación de roles a usuarios funciona
- **Tiempo:** 4 horas
- **Prioridad:** MEDIA

#### Task 2.5 - Unit Tests para HU-1
- [ ] Tests login exitoso
- [ ] Tests contraseña incorrecta
- [ ] Tests roles assignment
- [ ] Cobertura ≥ 80%
- **Verificación:** `dotnet test` pasa sin errores
- **Tiempo:** 3 horas
- **Prioridad:** MEDIA

### HU-2: Gestión de Catálogos (3 pts, 12h)

#### Task 2.6 - Implementar CatalogoService & Repositories
- [ ] Repository<AreaConocimiento>
- [ ] Repository<ODS>
- [ ] Repository<AreaAplicacion>
- [ ] Repository<PalabraClave>
- [ ] CRUD + borrado lógico
- **Verificación:** Todas las operaciones CRUD funcionan
- **Tiempo:** 4 horas
- **Prioridad:** ALTA

#### Task 2.7 - Crear CatalogosController
- [ ] GET /catalogos/areas-conocimiento
- [ ] POST /catalogos/areas-conocimiento (Admin only)
- [ ] Similares para ODS, AreaAplicacion, PalabraClave
- [ ] Validaciones de entrada
- **Verificación:** Endpoints documentados en Swagger. Tests Postman pasan
- **Tiempo:** 4 horas
- **Prioridad:** ALTA

#### Task 2.8 - Unit Tests para HU-2
- [ ] Test CRUD de catalogo
- [ ] Test validaciones
- [ ] Test borrado lógico
- **Verificación:** Cobertura ≥ 80%
- **Tiempo:** 2 horas
- **Prioridad:** MEDIA

### HU-4: Gestión de Sedes (1 pt, 6h)

#### Task 2.9 - Implementar SedeService & SedeController
- [ ] CRUD de sedes
- [ ] GET /sedes (lista con activos solamente)
- [ ] POST /sedes (Admin)
- [ ] Validaciones código único
- **Verificación:** CRUD funciona. Tests pasan
- **Tiempo:** 4 horas
- **Prioridad:** ALTA

#### Task 2.10 - Unit Tests para HU-4
- [ ] Test CRUD
- [ ] Test validaciones
- **Verificación:** Cobertura ≥ 80%
- **Tiempo:** 2 horas
- **Prioridad:** MEDIA

**Sprint Review (fin 11 agosto):** Demo de autenticación, catálogos y sedes. Aprobación de stakeholder.

---

## Fase 3: Implementar HU-3, HU-5, HU-7 (Sprint 5-6, 12-29 agosto)

**Responsables:** Juan José Grisales  
**Horas Estimadas:** 36  

### HU-3: Perfiles Docentes (3 pts, 12h)

#### Task 3.1 - Implementar DocenteService & Repository
- [ ] CRUD de docentes
- [ ] Vinculación con usuario
- [ ] Validación URL CvLAC
- **Tiempo:** 4 horas

#### Task 3.2 - DocenteController (/docentes)
- [ ] POST /docentes (own profile)
- [ ] GET /docentes/{id}
- [ ] PUT /docentes/{id} (own profile)
- **Tiempo:** 3 horas

#### Task 3.3 - Unit Tests HU-3
- [ ] Tests CRUD
- [ ] Tests validación URL
- **Tiempo:** 2 horas

#### Task 3.4 - Integración con frontend
- [ ] DTOs con mapeos
- [ ] Error handling
- **Tiempo:** 3 horas

### HU-5: Grupos de Investigación (4 pts, 16h)

#### Task 3.5 - Implementar GrupoService & Repository
- [ ] CRUD de grupos
- [ ] Vinculación de docentes con roles/períodos
- [ ] Control de Líder único activo
- [ ] Validación GrupLAC URL
- **Tiempo:** 6 horas

#### Task 3.6 - GrupoController
- [ ] POST /grupos (Admin)
- [ ] GET /grupos
- [ ] POST /grupos/{id}/docentes (Líder o Admin)
- [ ] GET /grupos/{id}/exportar (xlsx/pdf)
- **Tiempo:** 6 horas

#### Task 3.7 - Unit Tests HU-5
- [ ] Tests control Líder único
- [ ] Tests vinculación docentes
- [ ] Tests exportación
- **Tiempo:** 4 horas

### HU-7: Líneas de Investigación (2 pts, 8h)

#### Task 3.8 - Implementar LineaService & Repository
- [ ] CRUD líneas
- [ ] Estado Activa/Inactiva
- [ ] Validación código único
- **Tiempo:** 4 horas

#### Task 3.9 - LineaController
- [ ] POST /lineas (Admin/Coordinador)
- [ ] GET /lineas/activas
- [ ] PUT /lineas/{id}
- **Tiempo:** 3 horas

#### Task 3.10 - Unit Tests HU-7
- [ ] Tests CRUD
- [ ] Tests estado
- **Tiempo:** 1 hora

**Sprint Review (fin 29 agosto):** Demo de perfiles, grupos y líneas.

---

## Fase 4: Implementar HU-6, HU-8, HU-9 (Sprint 7-8, 30 agosto - 26 septiembre)

**Responsables:** Juan José Grisales  
**Horas Estimadas:** 28  

### HU-6: Semilleros (3 pts, 12h)
- Task: CRUD semilleros + asignación de tutores
- **Tiempo:** 12 horas

### HU-8: Articulación (2 pts, 8h)
- Task: Vinculación M:N de grupos/semilleros a líneas
- **Tiempo:** 8 horas

### HU-9: Relación de Líneas (3 pts, 12h)
- Task: Asociar líneas con Disciplinas, ODS, Áreas de Aplicación
- Task: Validación obligatoria 1 ODS + 1 Área del Conocimiento
- **Tiempo:** 12 horas

**Sprint Review (fin 26 septiembre):** Demo de semilleros, articulación y relaciones.

---

## Fase 5: Implementar HU-10, Testing & Refinamiento (Sprint 9-12, 27 sep - 15 nov)

**Responsables:** Ambos  
**Horas Estimadas:** 70  

### HU-10: Reportes (2 pts, 8h)
- Task: Dashboard unificado (Grupo ↔ Docentes ↔ Línea ↔ ODS)
- Task: Descargas Excel/CSV con filtros
- **Tiempo:** 8 horas

### QA & Testing (40h)
- Task: Integration tests end-to-end
- Task: Performance testing (1000 usuarios concurrentes)
- Task: Security testing (SQL injection, XSS, auth bypass)
- **Tiempo:** 40 horas

### Documentación (10h)
- Task: Manual de usuario
- Task: Manual de instalación/deployment
- Task: API docs (Swagger actualizado)
- Task: Glosario completo
- **Tiempo:** 10 horas

### Refinamiento & Bug Fixes (12h)
- Task: Iteración final basada en feedback
- Task: Optimizaciones de rendimiento
- **Tiempo:** 12 horas

**Sprint Review Final (15 noviembre):** Release v0.1 al ambiente staging.

---

## Cronograma Resumido

| Sprint | Período | Historias | Puntos | Estado |
|--------|---------|-----------|--------|--------|
| 3-4 | 12 jul - 26 jul | HU-1, HU-2, HU-4 | 9 | Por Iniciar |
| 5-6 | 27 jul - 10 ago | HU-3, HU-5, HU-7 | 9 | Por Iniciar |
| 7-8 | 11 ago - 31 ago | HU-6, HU-8, HU-9 | 7 | Por Iniciar |
| 9-12 | 01 sep - 15 nov | HU-10 + QA | 2 | Por Iniciar |
| **Total** | | | **28** | |

---

## Criterios de Aceptación por Sprint

### Sprint 3-4
- [ ] Repositorio GitHub con estructura base
- [ ] Docker-compose ejecuta aplicación + BD
- [ ] Autenticación funciona (login genera token)
- [ ] Catálogos CRUD completos
- [ ] Sedes CRUD completas
- [ ] Cobertura ≥ 80% en tests
- [ ] Swagger documentado

### Sprint 5-6
- [ ] Perfiles docentes CRUD
- [ ] Grupos CRUD + vinculación docentes
- [ ] Control de Líder único activo
- [ ] Líneas CRUD
- [ ] Exportación de integrantes (xlsx/pdf)
- [ ] Cobertura ≥ 80% en tests

### Sprint 7-8
- [ ] Semilleros CRUD
- [ ] Articulación Grupo-Línea y Semillero-Línea
- [ ] Articulación Línea-Disciplina, Línea-ODS, Línea-Aplicación
- [ ] Validación obligatoria (1 ODS + 1 Área)
- [ ] Cobertura ≥ 80% en tests

### Sprint 9-12
- [ ] Dashboard trazabilidad funcional
- [ ] Reportes y descargas
- [ ] Todos los tests pasan
- [ ] Documentación completa
- [ ] Release candidate listo

---

**Generado:** 2026-09-04  
**Versión:** 0.1  
**Estado:** Aprobado para Sprint 3

## Phase 8 — Configure Dependency Injection

Verify:

## Phase 9 — Docker/PostgreSQL

Verify:

## Phase 10 — Swagger

Verify:

## Phase 11 — Smoke Tests

Verify:

## Phase 12 — Version validation

Verify:
