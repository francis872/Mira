# Pruebas MIRA V3 — cierre de estabilización

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
