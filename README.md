# MIRA

Módulo de Investigación para la Gestión de Proyectos Académicos

Universidad de San Buenaventura Medellín

## About MIRA

This repository contains the initial architecture and development foundation for the MIRA academic research management module.

## Current State

MIRA V1 (seis catálogos) + V2 parcial (seguridad y usuarios/roles). Rama de trabajo: `feature/mira-v1-v2-integracion` (PR #4, borrador).

| Área | Estado |
|---|---|
| V1: 6 catálogos (CRUD, borrado lógico, frontend Flask) | Implementado; ahora protegido por JWT |
| V2: login bcrypt + JWT, roles, 401/403 | Implementado y probado |
| V2: administración de usuarios maestro–detalle (`CREATE PROCEDURE`) | Implementado y probado contra PostgreSQL real |
| V2: modelo académico maestro–detalle oficial (Entrega 2) | **Bloqueado**: el modelo oficial no está en el repositorio |
| Triggers / vistas del dominio académico | Pendiente del modelo oficial (solo existe `vw_usuarios_roles`) |

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
- BCrypt.Net-Next y JWT Bearer
- Frontend Flask (sesión del lado del servidor)
- Docker / Docker Compose
- Swagger/OpenAPI

## Repository Structure

- MIRA.Api/: ASP.NET Core API project foundation.
- database/: PostgreSQL initialization foundation and database notes.
- docs/: permanent rules, architecture notes, and source-document references.
- versiones/: version-specific Spec Kits.
- scripts/: utility script conventions.

## Getting Started

1. Copiar `.env.example` a `.env` y reemplazar **todos** los secretos (`Jwt__Secret` de ≥ 32 bytes aleatorios, `SECRET_KEY`, contraseña de PostgreSQL). Nunca versionar `.env`.
2. `docker compose up -d --build` (el SQL de `database/init` solo se ejecuta con un volumen nuevo; en uno existente aplicar `database/migrations/002-v2-usuarios-roles.sql`).
3. Aprovisionar el primer administrador una sola vez (ver `versiones/v2_profesor/7_quickstart.md`).
4. API: `http://localhost:8081/swagger`; frontend: `http://localhost:5000`.

Pruebas: `dotnet test MIRA.sln` (con `MIRA_TEST_DB` apuntando a una base de pruebas se ejecutan también las pruebas de procedimientos) y `pytest` en `frontend/` (`requirements-dev.txt`).

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

- Autenticación: contraseñas con bcrypt (costo configurable 10–14, 12 por defecto) y JWT con expiración de 60 minutos.
- Autorización: lectura de catálogos para cualquier usuario autenticado; escritura solo `Administrador` y `Coordinador`; gestión de usuarios solo `Administrador`. Aplicada en la API y en Flask.
- Respuestas: 401 sin token/credenciales/token vencido; 403 con rol insuficiente.
- No hay autorregistro público ni contraseñas/hashes en el repositorio.
- Pendiente: token CSRF en formularios Flask (hoy: cookie `SameSite=Strict` + verificación de `Origin`/`Referer`).

## Project Status

V1 + V2 parcial; ver `versiones/v2_profesor/8_tasks.md` para el detalle y los bloqueos.
