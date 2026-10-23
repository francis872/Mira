# Scripts

This directory contains utility scripts for local development and repository maintenance.

## Windows sin Docker

Requisitos: PostgreSQL 17 (herramientas en `C:\Program Files\PostgreSQL\17\bin`), .NET 10, Python con `frontend/requirements-dev.txt` instalado y `dotnet restore MIRA.sln` ejecutado.

Copiar `.env.example` a `.env.local`, reemplazar las contraseñas y definir puertos libres (por ejemplo `POSTGRES_PORT=5546`, `API_PORT=8081`, `FRONTEND_PORT=5000`). Desde la raíz:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-local.ps1
```

El script crea un PostgreSQL exclusivo para MIRA, aplica las migraciones pendientes de manera transaccional y arranca la API y Flask en segundo plano. Los servicios escuchan en `127.0.0.1`. Dashboard: `http://localhost:5000`; Swagger: `http://localhost:8081/swagger`. Puede repetirse: conserva los datos y reutiliza los procesos iniciados. Un puerto ocupado por otro proceso produce un error; no lo detiene automáticamente.

Datos, PID y logs: `.local/` (ignorado por Git). La base local es independiente del volumen Docker y de otros PostgreSQL instalados; no copia datos de instalaciones anteriores. `-ConfigFile` y `-PostgresBin` permiten usar otros archivos y rutas. Para cambiar puertos o configuración, detener antes de arrancar nuevamente.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/stop-local.ps1
```

Detener conserva `.local/postgres`. Reiniciar Windows requiere ejecutar otra vez el comando de arranque.

## Preparación y verificación de la auditoría

Desde la raíz, preparar las dependencias antes del primer arranque:

```powershell
python -m pip install -r frontend/requirements-dev.txt
dotnet restore MIRA.sln
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-local.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-local.ps1
```

`verify-local.ps1` crea una base `mira_audit_<fecha>_<PID>` exclusiva para las migraciones y las pruebas SQL/.NET. No borra bases ni datos operativos y conserva esa base para inspección. Requiere permiso PostgreSQL para crear bases. Las pruebas E2E utilizan la plataforma local iniciada y crean registros con prefijo `e2e-`, que inactivan al terminar. Si una etapa falla, el script termina con error.

La configuración se lee desde `.env.local`; `-ConfigFile` y `-PostgresBin` permiten cambiar el archivo y las herramientas. El script restaura las variables de entorno que modifica y no imprime secretos. Cada ejecución conserva una base adicional; su limpieza no está automatizada.

Entorno comprobado y límites: [pruebas-v2.md](../docs/pruebas-v2.md). El equipo verificado usa PostgreSQL 17.9, .NET SDK 10.0.402 y Python 3.14. Docker no estuvo disponible. En Compose, los puertos publicados están limitados a `127.0.0.1`.
