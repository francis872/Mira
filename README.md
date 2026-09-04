# MIRA

Módulo de Investigación para la Gestión de Proyectos Académicos

Universidad de San Buenaventura Medellín

## About MIRA

This repository contains the initial architecture and development foundation for the MIRA academic research management module.

## Current State

Repository foundation plus initial runnable implementation.

Current implemented code includes:
- API health endpoint.
- Authentication endpoint with JWT generation (`/api/auth/login`).
- Sedes listing and creation endpoints (`/api/sedes`).
- PostgreSQL repository integration using Npgsql.
- Global exception middleware.
- Local frontend preview served by Docker Compose.

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

- C#
- ASP.NET Core Web API
- PostgreSQL
- Docker
- Swagger/OpenAPI

## Repository Structure

- MIRA.Api/: ASP.NET Core API project foundation.
- database/: PostgreSQL initialization foundation and database notes.
- docs/: permanent rules, architecture notes, and source-document references.
- versiones/: version-specific Spec Kits.
- scripts/: utility script conventions.

## Getting Started (Docker First)

1. Clone the repository.
2. Copy `.env.example` to `.env`.
3. Run `docker compose up -d --build`.
4. Open API docs at `http://localhost:8080/swagger`.
5. Open frontend preview at `http://localhost:5173`.

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
- v1-api-inicial: initial API implementation branch.

## Security Status

Authentication, JWT, bcrypt and role authorization are intentionally not implemented in the repository foundation.

## Project Status

Initial repository architecture.
