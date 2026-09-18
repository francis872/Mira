# Quickstart MIRA V1

Requisitos: Docker Desktop y Git.

```powershell
git clone https://github.com/francis872/Mira.git
cd Mira
Copy-Item .env.example .env
docker compose up -d --build
docker compose ps
```

URLs validadas:

- Swagger: `http://localhost:8080/swagger/index.html`
- Frontend: `http://localhost:5173`
- Health: `http://localhost:8080/health`
- PostgreSQL host: `localhost:5544` (según la configuración actual de Docker Compose)

Ejemplos reales válidos:

```powershell
Invoke-RestMethod http://localhost:8080/api/sedes
Invoke-RestMethod http://localhost:8080/api/catalogos/ods

Invoke-RestMethod http://localhost:8080/api/sedes -Method Post -ContentType 'application/json' -Body '{"nombre":"Sede Validacion","ciudad":"Bogota"}'
Invoke-RestMethod http://localhost:8080/api/catalogos/ods -Method Post -ContentType 'application/json' -Body '{"nombre":"ODS 11"}'
```

Al finalizar:

```powershell
docker compose down
```

No usar `docker compose down -v` en el flujo normal del quickstart.
