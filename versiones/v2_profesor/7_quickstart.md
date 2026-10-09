# Guía para ejecutar y verificar V1 + V2 (localhost, sin login)

## Requisitos
Docker Desktop (o PostgreSQL 16 local), .NET SDK 10 y Python 3.12+ si se ejecuta sin Docker. No se requiere ninguna credencial de usuario de la aplicación.

## Arranque con Docker Compose
1. `Copy-Item .env.example .env` y definir `POSTGRES_PASSWORD` (el resto tiene valores por defecto; cambiar los puertos si están ocupados). Nunca versionar `.env`.
2. `docker compose up -d --build`.
3. Abrir `http://localhost:5000`: el dashboard carga directamente. Swagger: `http://localhost:8081/swagger`. PostgreSQL: `localhost:5544`.
4. Detener: `docker compose down`. No usar `-v` si se quieren conservar los datos.

## Base de datos existente (sin perder datos)
`docker compose exec mira-postgres sh /migrations/apply.sh` aplica solo las migraciones pendientes (`001` tablas V1, `002` datos iniciales idempotentes, `003` usuario/rol/usuario_rol y rutinas). Verificado: con una instalación previa se conservaron las filas existentes, no se duplicaron semillas y se completaron las faltantes.

## Arranque local de la plataforma (sin construir imágenes)
```powershell
# PostgreSQL: docker compose up -d mira-postgres
# API
$env:ConnectionStrings__PostgreSql = 'Host=localhost;Port=5544;Database=<db>;Username=<usuario>;Password=<clave>'
$env:ASPNETCORE_URLS = 'http://localhost:8081'; $env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project MIRA.Api --no-launch-profile
# Frontend
pip install -r frontend/requirements.txt
$env:API_URL = 'http://127.0.0.1:8081/api'; python frontend/app.py   # 127.0.0.1 evita ~2 s de demora de IPv6 con "localhost" en Windows
```

## Pruebas
```powershell
dotnet test MIRA.sln                      # sin base de datos: ejecuta las pruebas de "sin autenticación"; omite las que requieren PostgreSQL
$env:MIRA_TEST_DB = 'Host=localhost;Port=5544;Database=<db de pruebas>;Username=<usuario>;Password=<clave>'
dotnet test MIRA.sln                      # + regresión CRUD de los seis catálogos contra PostgreSQL (datos zz-reg-, eliminados al terminar)
cd frontend; pip install -r requirements-dev.txt; pytest -q tests/test_app.py
$env:MIRA_E2E_URL = 'http://localhost:5000'; $env:MIRA_E2E_API_URL = 'http://localhost:8081'
pytest -q tests/e2e                       # navegador → Flask → API → PostgreSQL con la plataforma en marcha (datos e2e-)
```
Rutinas almacenadas: `psql -f database/tests/v2_usuarios_roles.sql` sobre una base de pruebas.

## Matriz de aceptación
| Comprobación | Prueba |
|---|---|
| `http://localhost:5000` abre el dashboard | `test_root_opens_the_dashboard_directly`, captura del navegador |
| Ningún módulo exige login; `/login`, `/logout`, `/usuarios` no existen | `test_there_is_no_authentication_surface`, `test_authentication_and_user_routes_do_not_exist` |
| La API funciona sin JWT | `test_api_answers_without_token`, `SinAutenticacionTests` |
| Seis CRUD de V1 | `test_catalog_create_update_inactivate_persist[*]`, `CatalogCrudRegressionTests` |
| Persistencia al recargar (otra sesión) | mismas pruebas, que releen con un cliente nuevo |
| Validaciones y conflictos | `test_empty_form_shows_validation_error_and_creates_nothing[*]`, `test_duplicate_keyword_shows_conflict` |
| Maestro–detalle con rutinas almacenadas | `database/tests/v2_usuarios_roles.sql` (solo base de datos); dominio académico pendiente del modelo oficial |
| No se pierden datos existentes | migraciones idempotentes; comprobación documentada arriba |
