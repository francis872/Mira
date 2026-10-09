# MIRA

Módulo de Investigación para la Gestión de Proyectos Académicos

Universidad de San Buenaventura Medellín

## About MIRA

This repository contains the initial architecture and development foundation for the MIRA academic research management module.

## Current State

MIRA V1 (seis catálogos) + V2 parcial (base relacional y rutinas almacenadas). **Sin inicio de sesión**: `http://localhost:5000` abre directamente el dashboard. Rama de trabajo: `feature/mira-v1-v2-integracion` (PR #4, borrador).

| Área | Estado |
|---|---|
| V1: 6 catálogos (CRUD, borrado lógico, frontend Flask, PostgreSQL real) | Implementado y probado, sin autenticación |
| Inicio de sesión, JWT, bcrypt, roles, restricciones por sesión | **Fuera de alcance** (versión futura sin fecha). El código retirado sigue en el historial: etiqueta `auth-futuro-v2-261872f` |
| V2: rutinas almacenadas `CREATE PROCEDURE` con JSONB, vista y atomicidad (`usuario`→`usuario_rol`) | Solo en PostgreSQL (migración 003 y `database/tests`); sin API ni pantalla |
| V2: maestro–detalle académico, selectores de claves foráneas, triggers y vistas del dominio | **Pendiente**: el modelo relacional oficial de la Entrega 2 no está en el repositorio |

## Architecture

Controller
→ Service
→ Repository
→ PostgreSQL

- Controller: HTTP input/output only.
- Service: business rules and application orchestration.
- Repository: data access and SQL execution.
- PostgreSQL: persistent data storage.

Repository is the only layer allowed to access PostgreSQL directly.

## Methodology

Spec-Driven Development (SDD).

Specification precedes implementation.

## Technology

- C# / ASP.NET Core Web API sobre .NET 10 (LTS)
- PostgreSQL 16 (Npgsql + Dapper, SQL parametrizado y procedimientos almacenados)
- Frontend Flask (Jinja, Bootstrap 5; token CSRF en los formularios)
- Docker / Docker Compose
- Swagger/OpenAPI

## Repository Structure

- MIRA.Api/: ASP.NET Core API project foundation.
- database/: PostgreSQL initialization foundation and database notes.
- docs/: permanent rules, architecture notes, and source-document references.
- versiones/: version-specific Spec Kits.
- scripts/: utility script conventions.

## Getting Started

1. Copiar `.env.example` a `.env` y reemplazar la contraseña de PostgreSQL (`POSTGRES_PASSWORD`). Opcional: `SECRET_KEY` (firma la cookie del token CSRF), y los puertos si están ocupados (`POSTGRES_PORT=5544`, `API_PORT=8081`, `FRONTEND_PORT=5000`). Nunca versionar `.env`.
2. `docker compose up -d --build`. Las migraciones de `database/migrations` se aplican solas al crear el volumen.
3. Abrir `http://localhost:5000` (dashboard, sin login). API: `http://localhost:8081/api/...`; Swagger: `http://localhost:8081/swagger`; PostgreSQL: `localhost:5544`.

Detener: `docker compose down` (conserva los datos). Reiniciar desde cero: `docker compose down -v` (**borra el volumen de este proyecto**).

**Base de datos existente** (sin borrar datos): `docker compose exec mira-postgres sh /migrations/apply.sh`. Aplica solo las migraciones nuevas (`schema_migrations`) y es idempotente.

**Sin Docker para la API** (por ejemplo, si no se puede descargar la imagen base de .NET): con PostgreSQL en marcha, `ConnectionStrings__PostgreSql="Host=localhost;Port=5544;Database=...;Username=...;Password=..."` y `dotnet run --project MIRA.Api` (puerto 8081 con `ASPNETCORE_URLS=http://localhost:8081`); frontend: `pip install -r frontend/requirements.txt`, `API_URL=http://127.0.0.1:8081/api` (mejor que `localhost`, que en Windows puede añadir ~2 s por petición al probar primero IPv6), `python frontend/app.py`.

**Solución de problemas**
- Puerto 5544/5432 ocupado (otro PostgreSQL local): cambiar `POSTGRES_PORT` en `.env`.
- El dashboard muestra "no disponible": la API no responde o no alcanza PostgreSQL; revisar `docker compose logs mira-api`.
- Error 400 "token de seguridad" al guardar: recargar el formulario (el token CSRF vence al reiniciar Flask sin `SECRET_KEY`).
- `docker compose build` falla al resolver `mcr.microsoft.com`: es un problema de red/DNS del equipo; usar la ejecución local de la API descrita arriba.

Pruebas:
- `dotnet test MIRA.sln`: pruebas sin base de datos. Con `MIRA_TEST_DB` apuntando a una base de pruebas con las migraciones aplicadas se ejecuta también la regresión CRUD de los seis catálogos.
- `pytest` en `frontend/` (`requirements-dev.txt`): pruebas del frontend con la API simulada.
- E2E contra la plataforma en marcha: `MIRA_E2E_URL=http://localhost:5000 MIRA_E2E_API_URL=http://localhost:8081 pytest frontend/tests/e2e` (los datos de prueba llevan el prefijo `e2e-` y se inactivan).
- SQL de rutinas almacenadas: `database/tests/v2_usuarios_roles.sql`.

## Development Workflow

1. Clone.
2. Review docs/fuentes.
3. Review Spec Kit.
4. Complete/update specification.
5. Implement.
6. Build.
7. Test.
8. Commit.

## Branch Strategy

- main: stable versions only.
- develop: integration branch.
- develop/v1-api-inicial: initial API implementation branch.

## Security Status

- Alcance: **no hay autenticación ni autorización** en V1/V2; se reserva para una versión futura. La API y el frontend son abiertos y están pensados para ejecución local; no exponer a internet.
- Medidas generales vigentes: validación de entradas en servicios, SQL parametrizado (Dapper/Npgsql), restricciones e integridad en PostgreSQL, borrado lógico, escape de HTML en Jinja y token CSRF en todos los POST del frontend.
- La cookie de Flask solo guarda el token CSRF y los mensajes; es `HttpOnly` y `SameSite=Strict`.
- No hay contraseñas ni secretos en el repositorio (`.env` está ignorado).
- Las tablas `usuario`, `rol` y `usuario_rol` (migración 003) se conservan con sus datos; no se usan desde la aplicación.

## Project Status

V1 + V2 parcial, sin inicio de sesión; ver `versiones/v2_profesor/8_tasks.md` para el detalle y los bloqueos.
