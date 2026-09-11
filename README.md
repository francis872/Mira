# MIRA

Módulo de Investigación para la Gestión de Proyectos Académicos

Universidad de San Buenaventura Medellín

## Estado

MIRA V1 — Catálogos

## Stack

- .NET 10
- ASP.NET Core 10
- PostgreSQL
- SQL parametrizado con Npgsql
- Docker / Docker Compose
- Swagger/OpenAPI

## Arquitectura

Frontend
→ Controller
→ Service
→ Repository
→ PostgreSQL

## Ejecutar

```powershell
git clone https://github.com/francis872/Mira.git
cd Mira
Copy-Item .env.example .env
docker compose up -d --build
```

## Swagger

http://localhost:8080/swagger/index.html

## Frontend

http://localhost:5173

## Health

http://localhost:8080/health

## V1

Recursos incluidos en el alcance actual:

- sedes
- areas-conocimiento
- ods
- areas-aplicacion
- palabras-clave

El flujo de V1 se mantiene sin autenticación, JWT ni bcrypt, según la Spec Kit activa.

## Fuera de alcance

- JWT
- bcrypt
- login
- autenticación
- roles finales
- funciones futuras

## Documentación

- Constitution: `docs/1_constitution.md`
- Spec Kit V1: `versiones/v1_catalogos/`
- Fuentes: `docs/fuentes/`
- Auditorías: `docs/spec-code-audit.md`, `docs/arquitectura/solid-audit.md`

## Validación realizada

- `dotnet restore`
- `dotnet build`
- `docker compose config`
- `docker compose build`
- `docker compose up -d`
- visitas reales a Swagger, health y frontend
- pruebas reales contra PostgreSQL local
