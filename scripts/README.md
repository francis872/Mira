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

## Verificación de toda la plataforma en PostgreSQL 16

Con herramientas PostgreSQL 16 locales (los binarios de esta ejecución están en `.local/postgresql16/pgsql/bin`) y las dependencias .NET/Python preparadas:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-postgres16.ps1
```

El script crea una instancia independiente en `.local/pg16-validation/postgres`, una base de plataforma y otra exclusiva para SQL/.NET. Compila y publica el código actual de la API en Debug, en una ruta propia que no bloquea el Release habitual, y arranca Flask en puertos de pruebas 8082/5001, con PostgreSQL en 5547. Todos escuchan en loopback. Ejecuta `verify-local.ps1` con esa configuración y detiene solo sus procesos en `finally`. Conserva los datos y no convierte ni abre el directorio PostgreSQL 17 habitual.

`-PostgresBin`, `-DatabasePort`, `-ApiPort` y `-FrontendPort` permiten ajustar rutas/puertos. Un puerto ocupado produce error sin detener procesos ajenos. Las credenciales exclusivas y los logs quedan ignorados bajo `.local`. Paquete utilizado, resultados y límites: [verificacion-postgres16.md](../docs/verificacion-postgres16.md).

## Navegador real: preparación y ejecución

Requiere MIRA iniciada, Node 24 (WebSocket y fetch incorporados) y Edge. Iniciar un perfil de pruebas independiente, oculto y con depuración limitada a loopback:

```powershell
$miraBrowserProfile = Join-Path (Get-Location) '.local\edge-audit'
Start-Process -FilePath 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe' -WindowStyle Hidden -ArgumentList "--headless=new --no-first-run --remote-debugging-address=127.0.0.1 --remote-debugging-port=9224 --user-data-dir=`"$miraBrowserProfile`" about:blank"
node scripts/verify-browser.mjs
```

El verificador usa `MIRA_E2E_URL` (por defecto `http://127.0.0.1:5000`) y `MIRA_BROWSER_DEBUG_URL` (por defecto `http://127.0.0.1:9224`). Comprueba búsqueda, navegación, Bootstrap real, creación, edición, retiro y menú móvil, usando registros `e2e-browser-*`. Las capturas se guardan en `docs/evidencias/` y el resumen en `.local/browser-check.json`. El CDN debe estar disponible para Bootstrap. El verificador no utiliza el perfil personal del navegador.

La versión actual añade apertura/cierre del menú móvil, desplegable por teclado y 52 combinaciones de inicio/listado/formulario con anchos 390, 768, 1024 y 1440 px. Comprueba que no haya desbordamiento del documento ni superposición con la barra lateral y captura también el formulario de universidad. En total informa diez comprobaciones agrupadas; no las confunde con las pruebas de la API o con una auditoría exhaustiva de accesibilidad.

Para cerrar el navegador de pruebas sin afectar otras ventanas, enviar `Browser.close` por su conexión de depuración:

```powershell
node --input-type=module -e "const v=await (await fetch('http://127.0.0.1:9224/json/version')).json(); const w=new WebSocket(v.webSocketDebuggerUrl); w.onopen=()=>w.send(JSON.stringify({id:1,method:'Browser.close'}));"
```
