# Quickstart MIRA V1

Requisitos: Docker Desktop y Git.

```powershell
git clone <url-del-repositorio>
cd MIRA
Copy-Item .env.example .env
docker compose up -d --build
docker compose ps
```

URLs:

- Swagger: `http://localhost:8080/swagger`
- Frontend: `http://localhost:5173`
- Health: `http://localhost:8080/health`
- PostgreSQL host: `localhost:5544` (configurable por `.env`)

Ejemplos:

```powershell
Invoke-RestMethod http://localhost:8080/api/areas-conocimiento
Invoke-RestMethod http://localhost:8080/api/areas-conocimiento/1
Invoke-RestMethod http://localhost:8080/api/areas-conocimiento -Method Post -ContentType 'application/json' -Body '{"nombre":"Ingenieria"}'
Invoke-RestMethod http://localhost:8080/api/areas-conocimiento/1 -Method Put -ContentType 'application/json' -Body '{"nombre":"Ingenieria Aplicada","activo":true}'
Invoke-RestMethod http://localhost:8080/api/areas-conocimiento/1 -Method Patch -ContentType 'application/json' -Body '{"activo":false}'
Invoke-RestMethod http://localhost:8080/api/areas-conocimiento/1 -Method Delete
```

Al finalizar:

```powershell
docker compose down
```
