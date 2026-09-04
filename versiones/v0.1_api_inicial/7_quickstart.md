# Guía de Inicio Rápido v0.1 — Módulo de Investigación

**Audiencia:** Desarrolladores backend (C#/ASP.NET Core)  
**Duración Estimada:** 15 minutos (primera instalación)  
**Requisitos:** Git, Docker Desktop, Visual Studio Code + C# extension  
**Última Actualización:** 2026-09-04

---

## 1. Clonar el Repositorio

```bash
# Clonar desde GitHub
git clone https://github.com/francis872/Mira.git
cd Mira

# Verificar rama
git branch
# Deberías estar en 'develop' o 'main'

# Cambiar a rama de desarrollo v0.1
git checkout v1-api-inicial
```

---

## 2. Requisitos Previos

### Windows (Recomendado)

```powershell
# Verificar Docker Desktop está corriendo
docker --version
# Output: Docker version 24.0+

# Verificar .NET SDK
dotnet --version
# Output: .NET 8.0.xxx o superior

# Verificar Git
git --version
# Output: git version 2.42+
```

### macOS/Linux
Similar a Windows, pero asegurate que Docker Desktop esté corriendo.

---

## 3. Iniciar Servicios Locales

### Opción A: Docker Compose (Recomendado)

```bash
# Posicionarse en raíz del proyecto
cd ~/projects/Mira

# Construir imagen Docker y ejecutar servicios
docker compose up -d --build

# Verificar servicios iniciados
docker compose ps
```

**Salida esperada:**
```
NAME          STATUS              PORTS
mira-api      Up 10 seconds       0.0.0.0:8080->8000/tcp
postgres      Up 15 seconds       0.0.0.0:5544->5432/tcp
```

### Opción B: PostgreSQL Local (si no usas Docker)

```powershell
# En Windows, si tienes PostgreSQL instalado:
# Asegúrate que escucha en puerto 5544 (no 5432)

# Actualizar appsettings.Development.json:
# "ConnectionStrings": {
#   "DefaultConnection": "Host=localhost;Port=5544;Database=mira_v01;User Id=postgres;Password=..."
# }

# Luego:
cd src/Mira.API
dotnet watch run
# La aplicación escuchará en http://localhost:5000
```

---

## 4. Verificar Estado de la Aplicación

### Endpoint Health Check

```bash
# Verificar API activo
curl http://localhost:8080/health

# Respuesta esperada:
# {"status":"Healthy","timestamp":"2026-09-04T10:30:00Z"}
```

### Swagger (Documentación Interactiva)

Abre tu navegador:
```
http://localhost:8080/swagger/ui
```

Deberías ver lista de todos los endpoints (Auth, Catálogos, Sedes, etc.)

### Base de Datos

```bash
# Conectar a PostgreSQL desde cliente psql
psql -h localhost -p 5544 -U mira_user -d mira_v01

# En prompt:
mira_v01=# SELECT COUNT(*) FROM usuarios;
mira_v01=# SELECT * FROM roles;
mira_v01=# \dt  (listar todas las tablas)

# Salir
\q
```

---

## 5. Estructura del Proyecto

```
Mira/
├── Mira.sln                              # Solución Visual Studio
├── src/
│   ├── Mira.API/                         # Proyecto principal
│   │   ├── Controllers/                  # Endpoints HTTP
│   │   ├── Middleware/                   # Autenticación, error handling
│   │   ├── Program.cs                    # DI, configuración
│   │   ├── appsettings.json              # Configuración base
│   │   └── appsettings.Development.json  # Configuración local
│   ├── Mira.Application/                 # Services & DTOs (v0.2+)
│   ├── Mira.Domain/                      # Entidades (v0.2+)
│   └── Mira.Infrastructure/              # DbContext & Repositories (v0.2+)
├── tests/
│   ├── Mira.Tests.Unit/                  # Unit tests
│   └── Mira.Tests.Integration/           # Integration tests
├── database/
│   └── init/
│       ├── init.sql                      # Script de inicialización
│       └── seed.sql                      # Datos iniciales (roles, ODS, etc.)
├── docs/
│   ├── SDD.md                            # Software Design Document
│   ├── arquitectura/
│   │   └── branch-strategy.md            # Git workflow
│   └── fuentes/                          # Documentos extraídos
├── versiones/
│   └── v0.1_api_inicial/                 # Spec Kit (Este archivo)
│       ├── 1_readme.md
│       ├── 2_spec.md                     # Especificación funcional
│       ├── 3_plan.md                     # Plan técnico
│       ├── 4_research.md                 # Decisiones técnicas (ADR)
│       ├── 5_data_model.md               # Modelo de datos + ER
│       ├── 6_contracts.md                # Contratos API (OpenAPI)
│       ├── 7_quickstart.md               # Este archivo
│       └── 8_tasks.md                    # Backlog técnico
├── Dockerfile
├── docker-compose.yml
├── .gitignore
└── .github/
    └── workflows/                        # CI/CD (v0.2+)
```

---

## 6. Workflow de Desarrollo

### Crear Feature Branch

```bash
# Crear rama feature basada en develop
git checkout develop
git pull origin develop
git checkout -b feature/HU-1-auth

# Cambios + commits
git add .
git commit -m "feat(auth): implement login endpoint"

# Push a GitHub
git push origin feature/HU-1-auth
```

### Ejecutar Tests Locales

```bash
# Unit tests
dotnet test tests/Mira.Tests.Unit

# Con cobertura
dotnet test /p:CollectCoverage=true

# Integration tests (requiere PostgreSQL activo)
dotnet test tests/Mira.Tests.Integration
```

### Crear Pull Request

1. Ve a GitHub: https://github.com/francis872/Mira/pulls
2. Click "New Pull Request"
3. Base: `develop`, Compare: `feature/HU-1-auth`
4. Descripción: referencia la historia (Closes #5)
5. Request review a franciscoguisado o juanjosegrisales
6. Esperar CI/CD (tests + linting)

### Merge a Develop

- ✅ CI/CD pasa
- ✅ 1+ revisión aprobada
- ✅ 0 conflictos
- Click "Squash and merge"

---

## 7. Configuración del Editor

### Visual Studio Code + C#

```bash
# Extensiones recomendadas:
# - C# (ms-dotnettools.csharp)
# - C# Dev Kit (ms-dotnettools.csdevkit)
# - REST Client (humao.rest-client) - para testear API

# Crear archivo .vscode/settings.json:
{
  "omnisharp.enableRoslynAnalyzers": true,
  "omnisharp.enableEditorConfigSupport": true,
  "[csharp]": {
    "editor.defaultFormatter": "ms-dotnettools.csharp",
    "editor.formatOnSave": true,
    "editor.codeActionsOnSave": {
      "source.fixAll.csharp": "explicit"
    }
  }
}
```

### Visual Studio 2022+ (Alternativa)

```bash
# Si tienes Visual Studio:
# 1. Abrir Mira.sln
# 2. Build > Build Solution (Ctrl+Shift+B)
# 3. Run (F5) para ejecutar con debugger
# 4. Swagger UI abre automáticamente
```

---

## 8. Variables de Entorno Locales

### .env (Docker Compose)

```bash
# .env en raíz del proyecto
POSTGRES_USER=mira_user
POSTGRES_PASSWORD=localdev123
POSTGRES_DB=mira_v01
JWT_KEY=dev-key-min-32-chars-for-testing-locally
```

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=mira_v01;User Id=mira_user;Password=localdev123"
  },
  "Jwt": {
    "Key": "dev-key-min-32-chars-for-testing-locally",
    "Issuer": "mira-api-dev",
    "Audience": "mira-frontend-dev",
    "ExpirationMinutes": 1440
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 9. Troubleshooting Común

### Docker: "Bind for 0.0.0.0:5544 failed: port is already in use"

```powershell
# Windows: Encontrar qué está usando puerto 5544
Get-NetTCPConnection -LocalPort 5544 -State Listen | Get-Process

# Solución:
# 1. Cerrar aplicación que usa el puerto, o
# 2. Cambiar puerto en docker-compose.yml: "5555:5432"
# 3. Reintentar: docker compose up -d
```

### .NET Build Error: "Project file not found"

```bash
# Asegurate estar en raíz de Mira.sln
pwd  # Deberías ver .../Mira/

# Si no:
cd /path/to/Mira

# Rebuild
dotnet clean
dotnet build
```

### PostgreSQL Connection Refused

```bash
# Verificar que contenedor está corriendo
docker compose ps

# Si postgres no aparece:
docker compose logs postgres
# Revisar errores

# Reiniciar:
docker compose restart postgres
```

### "Migrations pending. Apply migrations before running the application."

```bash
# Dentro del contenedor, ejecutar migraciones:
docker compose exec mira-api dotnet ef database update

# O localmente (si conectas a BD):
dotnet ef database update --project src/Mira.API
```

---

## 10. Próximos Pasos

1. **Leer especificación:** [2_spec.md](2_spec.md)
2. **Entender arquitectura:** [3_plan.md](3_plan.md)
3. **Revisar modelo de datos:** [5_data_model.md](5_data_model.md)
4. **Estudiar contratos API:** [6_contracts.md](6_contracts.md)
5. **Tomar la primera tarea:** [8_tasks.md](8_tasks.md)

---

## 11. Contactos & Ayuda

| Rol | Nombre | GitHub | Expertise |
|-----|--------|--------|-----------|
| **Backend Lead** | Juan José Grisales | @juanjosegrisales | C#, ASP.NET Core, arquitectura |
| **DevOps/Frontend Lead** | Francisco Guisado | @franciscoguisado | Docker, CI/CD, frontend |
| **Stakeholder** | Carlos Arturo Castro | @carlosarturo | Requisitos, validación |

---

## 12. Referencias Rápidas

### Comandos Útiles

```bash
# Build
dotnet build

# Tests
dotnet test

# Run (local)
dotnet run --project src/Mira.API

# Migrations
dotnet ef migrations add MigrationName --project src/Mira.API
dotnet ef database update --project src/Mira.API

# Docker
docker compose up -d --build
docker compose down
docker compose logs -f mira-api
```

### URLs Locales

| Servicio | URL |
|----------|-----|
| API | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger/ui |
| PostgreSQL | localhost:5544 (mira_user/localdev123) |

### Documentos Clave

- [SDD.md](../SDD.md) — Visión general de arquitectura
- [2_spec.md](2_spec.md) — Requisitos funcionales v0.1
- [6_contracts.md](6_contracts.md) — API endpoints
- [5_data_model.md](5_data_model.md) — Schema BD

---

**Versión:** 0.1  
**Última actualización:** 2026-09-04  
**Mantén esta guía actualizada conforme avance v0.1**
