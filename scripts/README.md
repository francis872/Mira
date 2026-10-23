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

## Verificación en navegador real

Requiere MIRA iniciada, Node 24 (WebSocket y fetch incorporados) y Edge. Iniciar un perfil de pruebas independiente, oculto y con depuración limitada a loopback:

```powershell
$miraBrowserProfile = Join-Path (Get-Location) '.local\edge-audit'
Start-Process -FilePath 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe' -WindowStyle Hidden -ArgumentList "--headless=new --no-first-run --remote-debugging-address=127.0.0.1 --remote-debugging-port=9224 --user-data-dir=`"$miraBrowserProfile`" about:blank"
node scripts/verify-browser.mjs
```

El verificador usa `MIRA_E2E_URL` (por defecto `http://127.0.0.1:5000`) y `MIRA_BROWSER_DEBUG_URL` (por defecto `http://127.0.0.1:9224`). Comprueba búsqueda, navegación, Bootstrap real, creación, edición, retiro y menú móvil, usando registros `e2e-browser-*`. Las capturas se guardan en `docs/evidencias/` y el resumen en `.local/browser-check.json`. El CDN debe estar disponible para Bootstrap. El verificador no utiliza el perfil personal del navegador.

Para cerrar el navegador de pruebas sin afectar otras ventanas, enviar `Browser.close` por su conexión de depuración:

```powershell
node --input-type=module -e "const v=await (await fetch('http://127.0.0.1:9224/json/version')).json(); const w=new WebSocket(v.webSocketDebuggerUrl); w.onopen=()=>w.send(JSON.stringify({id:1,method:'Browser.close'}));"
```
