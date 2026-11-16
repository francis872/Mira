# Pruebas MIRA V3 — cierre de estabilización

> Actualización del 16 de noviembre de 2026: **170 Flask aprobadas, cinco omitidas**, recuperación real desde suspensión en **51,571 s**, primer login 302 sin reenvío, inicio/readiness 200 y mismo JWT realmente expirado 401. Aprobaron 16 comprobaciones remotas y seis muestras responsive emuladas. Se conservan los ensayos fallidos y las interrupciones del instrumento. OSV: 25 distribuciones fijadas, sin avisos conocidos; no es un inventario del SO. Actions está excluido como requisito; C06 y la revisión formal continúan pendientes. [Despliegue y alcance](DESPLIEGUE_RENDER_NEON.md), [evidencia nueva](evidencias/arranque-render-20261116/verificacion.json). Los resultados históricos siguientes conservan sus fechas y fuentes.

> Fase 6 (2026-11-12): SAPIENS está operativa en [https://sapiens-v1-v3-20261112.vercel.app](https://sapiens-v1-v3-20261112.vercel.app), con Flask en Vercel Hobby, API .NET en Render Free y PostgreSQL 16 en Neon Free. La URL Production aprobó **50 criterios funcionales (49 en la pasada completa y repetición puntual aprobada tras un timeout TLS)**, 14 comprobaciones de seguridad/operación y una revisión de navegador de escritorio, tableta y móvil emulados. GitHub Actions continúa bloqueado por la cuenta; el PR #7 permanece Draft y sin merge. V2 mantiene pendientes académicos, incluido C06. No se declara cerrada toda la misión ni certificada producción institucional. [Informe vigente](FASE6_INFORME_FINAL.md). Las secciones fechadas de abajo se conservan como antecedentes.

> Seguimiento local 2026-11-07: [cierre académico y propuesta sin publicar](V3_CIERRE_ACADEMICO.md). Este documento conserva su alcance histórico; el PR #7 publicado contiene ocho commits V3 más 43 anteriores. Las nuevas correcciones y CI requieren autorización de publicación.

## Trazabilidad de la corrección de arranque del 16 de noviembre

| Control | Implementación | Prueba o evidencia |
|---|---|---|
| Activar salud sin credenciales desde el navegador | `templates/login.html`, `static/render-startup.js` | `verify-render-startup.mjs`: GET 200 por HTTPS, sin cuerpo, cookie, Authorization, Referer ni assertion; resultado saneado nuevo |
| Publicar solo una URL de salud sin secretos, únicamente en Render | `app.py::inject_globals` | `test_render_login_page_can_activate_only_public_health_from_browser`, `test_non_public_configuration_never_becomes_a_browser_startup_target`, `test_browser_startup_is_limited_to_render_profile` |
| Esperar la API sin repetir credenciales | `app.py::_wake_render_api/api_call` | `test_gateway_startup_get_is_retried_without_credentials_and_login_is_signed_once`, `test_credential_post_is_never_replayed_and_diagnostics_do_not_log_its_values` |
| Rechazar redirecciones del servidor y respuestas de carga incompatibles | `app.py::_wake_render_api` | `test_non_startup_errors_never_forward_credentials_or_follow_redirects`, `test_loading_html_does_not_count_as_a_ready_api` |
| Agotar la espera sin enviar el login; conservar límites de otras llamadas | `app.py::api_call` | `test_unready_platform_is_bounded_and_does_not_attempt_login`, `test_deadline_exhaustion_does_not_send_credentials`, `test_slow_api_startup_reserves_a_longer_login_wait_without_changing_other_calls` |
| Mantener CSRF y separar liveness/readiness | `app.py::verify_csrf/health/ready` | `test_csrf_rejection_happens_before_any_startup_probe`, `test_readiness_can_wake_the_api_but_liveness_does_not`, `test_readiness_stays_unavailable_when_api_cannot_wake` |
| Impedir que los probes HEAD autentiquen u ocupen threads esperando la API | `seguridad.py::register_security.login` | `test_login_head_is_read_only_and_never_authenticates_or_wakes_dependencies`, con y sin cuerpo de credenciales; logs históricos de HEAD de 36–37 s conservados |

Las pruebas Python anteriores están en [test_render_startup.py](../frontend/tests/test_render_startup.py). [Resultados, fuentes y fallos conservados](evidencias/arranque-render-20261116/verificacion.json). Los cinco tests de PostgreSQL compartido se omiten por falta de la base aislada del verificador integrado; no se cuentan como aprobados. Los muestreos anteriores de interfaz y V1/V2 conservan su alcance y fecha.

## Antecedente de estabilización

Fecha: 2026-11-05. [Matriz de 30 requisitos, métodos y evidencia](V3_REVISION_FINAL.md). Los resultados históricos de V1/V2 no se cuentan como ejecuciones actuales.

| Comprobación | Resultado ejecutado | Evidencia |
|---|---|---|
| API .NET con PostgreSQL 16.15 aislado | 71 aprobadas, 0 fallidas, 0 omitidas | TRX privado tests/MIRA.Api.Tests/TestResults/v3-backend.trx; .local/v3-stabilization-final.log |
| Flask, sesión, configuración y regresión | 78 aprobadas | mismo log; test_production.py y tests previos |
| HTTP Flask → API → PostgreSQL | 43 aprobadas | mismo log; frontend/tests/e2e |
| Las mismas E2E sobre Linux/Gunicorn/Docker | 43 aprobadas, repetición del conjunto | .local/v3-docker-final.log |
| Migraciones y SQL | 001–008 en bases nuevas; 5 suites sin error | database/tests/*.sql, mismo log local |
| C06 catálogo real | 22 columnas FK públicas, 0 opcionales | database/tests/v3_c06_nulabilidad.sql; aceptación del profesor pendiente |
| Navegador Edge | 50 combinaciones, 0 errores consola, documento sin overflow | [navegador](evidencias/v3/browser-results.json), capturas |
| Teclado y reflow | 6 modales completos, tabla, validación, detalle, zoom visual 2x y CSS 640/320 | [teclado](evidencias/v3/keyboard-results.json) |
| axe-core 4.10.3 | 18 revisiones, 0 infracciones automáticas finales; 10 instancias incomplete detalladas | [axe](evidencias/v3/accessibility-results.json) |
| Dependencias Python imagen Linux | OSV: 18 paquetes, 0 avisos conocidos | [OSV final](evidencias/v3/osv-estabilizado.json) |
| Dependencias .NET | NuGet sin avisos en consulta previa de la fecha; paquetes sin cambio posterior | revisión final E6 |
| Imágenes | builds correctos, sin root, /app sin archivos privados detectados | [imágenes](evidencias/v3/docker-images.json) |
| Reinicio/logout real sobre Gunicorn | sesión opaca persiste con volumen; logout limpia y revoca mismo JWT | [sesiones Docker](evidencias/v3/docker-session-results.json) |
| Modo Production local | PORT 10000; API health 200/sin token 401/Swagger 404; cookie Secure/HttpOnly/Strict | [smoke](evidencias/v3/production-smoke.json) |
| Render YAML | conforme a JSON Schema público | [schema](evidencias/v3/render-schema.json); sin despliegue |

## Reproducción

### Regresión anónima del arranque en Edge

`scripts/verify-render-startup.mjs` comprueba en un navegador real que el script sí llega a la red, obtiene 200 de la URL de salud y no transmite cuerpo, cookie, Authorization, Referer ni assertion. No usa cuentas y **no acredita suspensión ni login**: ese ensayo se registra aparte. Requiere Node con WebSocket global y Microsoft Edge; usar un perfil exclusivo y un puerto de depuración libre en loopback.

```powershell
if (Get-NetTCPConnection -LocalPort 9226 -State Listen -ErrorAction SilentlyContinue) {
    throw 'El puerto 9226 está ocupado; elija otro puerto libre para esta prueba.'
}
$perfilPrueba = Join-Path (Get-Location).Path ('.local\edge-arranque-' + [guid]::NewGuid().ToString('N'))
$edgePrueba = Start-Process 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe' -WindowStyle Hidden -PassThru -ArgumentList @('--headless=new', '--remote-debugging-address=127.0.0.1', '--remote-debugging-port=9226', "--user-data-dir=`"$perfilPrueba`"", 'about:blank')
try {
    Start-Sleep -Seconds 3
    $env:MIRA_E2E_URL = 'https://sapiens-web-v1-v3-20261113.onrender.com'
    $env:MIRA_BROWSER_ALLOWED_ORIGIN = $env:MIRA_E2E_URL
    $env:MIRA_RENDER_API_HEALTH_URL = 'https://sapiens-api-v1-v3-20261112.onrender.com/health'
    $env:MIRA_BROWSER_DEBUG_URL = 'http://127.0.0.1:9226'
    node scripts/verify-render-startup.mjs
} finally {
    if (-not $edgePrueba.HasExited) { Stop-Process -Id $edgePrueba.Id }
}
```

La evidencia se guarda en `.local/render-startup-browser/render-startup-browser.json`. El proceso de cierre corresponde exclusivamente a la instancia creada por el comando; no cerrar otros procesos de Edge. El perfil de prueba queda conservado bajo `.local`, ignorado por Git. Para el ensayo de inactividad seguir el procedimiento de [operación reproducible](DESPLIEGUE_RENDER_NEON.md#operación-reproducible), que evita calentar la API antes de medir.

### Regresión integrada histórica

Instalar `frontend/requirements-dev.txt` en entorno limpio; incluye requirements.lock. Restaurar .NET. Para el conjunto integrado usar `scripts/verify-postgres16.ps1 -Browser`; requiere PostgreSQL 16, Edge y Node. `MIRA_AXE_SCRIPT` debe señalar una copia local de axe-core 4.10.3 para incluir axe; sin ella no se genera esa verificación. Los scripts crean bases nuevas, datos ficticios y servicios en puertos exclusivos y detienen sus procesos; nunca apuntar las E2E a una base institucional.

```powershell
scripts/verify-postgres16.ps1 -Browser
# Repetir solo navegador después de cambiar presentación/verificador:
scripts/verify-postgres16.ps1 -BrowserOnly -Browser
# Después de construir las etiquetas de prueba:
docker build -t mira-v3-api:stabilization .
docker build -t mira-v3-frontend:stabilization frontend
scripts/verify-docker.ps1
python scripts/verify-production-smoke.py
python scripts/verify-image-content.py --output docs/evidencias/v3/docker-images.json mira-v3-api:stabilization mira-v3-frontend:stabilization
```

El proyecto Docker usa nombre único mira-v3-check-<timestamp>, puertos exclusivos y credenciales aleatorias privadas. En esta ejecución los puertos alternos fueron 15647/18086/15006 para no detener servicios ya existentes. Conserva únicamente sus volúmenes de prueba (no usa down -v). El smoke usa 18087/15007, no realiza login ni contacta sus destinos ficticios. Sus cookies Secure se inspeccionan por HTTP local: no prueba TLS real.

Auditoría OSV desde la imagen final (transmite únicamente nombres/versiones de paquetes): montar scripts/audit-python.py como /audit.py de solo lectura y ejecutar `python /audit.py --output /tmp/osv-report.json` mediante `docker run --rm --entrypoint python`. Capturar/sanear el JSON antes de publicarlo. Alternativa host: `python scripts/audit-python.py --package-dir <site-packages-del-entorno> --output <reporte.json>`; sin --package-dir audita todas las distribuciones instaladas. Un bloqueo DNS sale con código 2 y estado blocked, nunca passed.

Se usó PYTHONPATH=.local/stabilized-deps para probar las versiones corregidas sin alterar Python global. El temporal global pytest tenía ACL de otro contexto: se fijó PYTEST_ADDOPTS con --basetemp en un directorio nuevo y exclusivo de .local y las 78 pruebas pasaron. No repetir esa ruta ni usar un directorio con datos propios: pytest administra su basetemp.

## Seguridad y regresión cubiertas

SeguridadTests ejecuta login correcto 200, credencial errónea 401, sin token 401, JWT expirado/firma/emisor/audiencia inválidos 401, mismo JWT revocado 401 y RBAC 403; también límite 429, rol desconocido, seis acciones administrativas contra dos roles y fronteras 11/12/72/73. SeguridadDatabaseTests verifica bcrypt persistido, sesiones y cambios de rol/password con PostgreSQL. ProductionConfigurationTests rechaza TLS no verificado y errores SQL detallados; aceptar configuración no abre una conexión remota.

Flask comprueba rotación de identificador, JWT solo servidor, CSRF, expiración, 401/403 comprensibles, logout con API fallida, next externo ignorado, guardas productivas, Secure/HttpOnly/Strict y recuperación con/sin almacenamiento. E2E/SQL verifican seis catálogos, once recursos, cinco asociaciones, ficha grupo-semilleros, triggers y rollback. Se conservan separados usuario y usuarios.

## Hallazgos corregidos y límites

El reporte histórico [dependencias-python.json](evidencias/v3/dependencias-python.json) conserva 8 GHSA en 4 paquetes: requests 2.32.5, urllib3 2.6.3, idna 3.11 y certifi 2026.2.25. No describe el runtime final: ahora son 2.33.0/2.8.0/3.15/2026.10.18. El final Linux incorpora Gunicorn 26.2.0; pip/setuptools/wheel se retiran tras instalación porque no son necesarios en runtime. No se auditó todo el SO ni se actualizó el entorno global viejo.

La ampliación del navegador encontró pérdida de contraste durante transiciones del menú; se eliminó esa transición de color manteniendo paleta y estados. Se corrigieron nombres de modales, foco al quitar detalle, scroll por teclado y el texto accesible de obligatorio. El verificador ahora espera loader/transición de Bootstrap: sus intentos fallidos no se cuentan como aprobados. El reporte final reemplaza los conteos anteriores 42/59/70.

Pendiente: NVDA/VoiceOver (Narrator está instalado pero no se recorrió), dispositivos reales, zoom de escritorio 200/400%, decisión humana sobre incomplete de axe, vencimiento por paso del tiempo real y producción Render/Neon. Zoom visual CDP 2x y reflow CSS no equivalen a certificar zoom de escritorio 400%. /health es liveness, no readiness de BD. C06 no se presenta como aceptado por el profesor.

No se modificaron credenciales reales ni datos habituales. No se crearon commits ni servicios cloud. Ver [guía de diez minutos](V3_GUIA_SUSTENTACION.md) y [dictamen final](V3_REVISION_FINAL.md).

Para repetir el escaneo de candidatos Git: `python scripts/audit-worktree.py`; genera nombres de archivos/hallazgos y snapshot SHA-256 en .local, sin imprimir valores secretos. Los cuatro avisos de literales se revisaron como fixtures aislados o destinos example.invalid, no credenciales reales. [Justificación por archivo](evidencias/v3/secret-scan.json). Los [SHA-256 de artefactos](evidencias/v3/artifact-sha256.json) permiten verificar los JSON/PNG entregados; no cubren todo el historial Git.
