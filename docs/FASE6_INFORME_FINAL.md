# SAPIENS — informe final de Fase 6

Corte: 2026-11-12T21:44:37.945552-05:00, America/Bogota. Las marcas UTC de los proveedores corresponden al 13 de noviembre; las pruebas se ejecutaron el 12 en Colombia. Trabajo exclusivo en `C:\Users\Usuario\OneDrive\Escritorio\MIRA`.

SAPIENS está operativa en [https://sapiens-v1-v3-20261112.vercel.app](https://sapiens-v1-v3-20261112.vercel.app), con Flask en Vercel Hobby, API .NET en Render Free y PostgreSQL 16 en Neon Free. La URL Production aprobó **50 criterios funcionales (49 en la pasada completa y repetición puntual aprobada tras un timeout TLS)**, 14 comprobaciones de seguridad/operación y una revisión de navegador de escritorio, tableta y móvil emulados. GitHub Actions continúa bloqueado por la cuenta; el PR #7 permanece Draft y sin merge. V2 mantiene pendientes académicos, incluido C06. No se declara cerrada toda la misión ni certificada producción institucional.

## A. Estado del repositorio

Remoto `https://github.com/francis872/Mira.git`, rama `feature/mira-v3`; HEAD de código funcional revisado `e3f60e5e78381432c885e68cf8b24376c303aedb`. No se cambió de rama, reescribió historia ni hizo force push. La documentación final se publica después de ese commit; su SHA se obtiene del historial de la rama. [Preservación](cierre-v1-v3/evidencias/fase6-20261112/preservacion.json), [control de cambios](cierre-v1-v3/evidencias/fase6-20261112/control-final.json).

Antes de intervenir se verificó un ZIP de 355 archivos fuente/configuración (SHA256 `1009d5415cb79deac5ecceaadd674001ff5c2351ed274a5c88c6b8ca36c454de`) y un bundle de todas las referencias Git (`5222fb63a878771549c845f7a6b2209d883cc1db5c03a59b969e0360eba90120`). No incluye todos los artefactos reproducibles de .local/build. Los respaldos anteriores se conservan. No se accedió a BACKSTAGE.

## B. Archivos modificados

El inventario completo está en control-final.json; se compara contra el HEAD inicial `3105273af84e55a1e813e76f2acb92acc31caa94` y conserva los cambios locales preexistentes. Responsabilidades principales:

- API/modelo: servicios/repositorios/controladores de requisitos V1/V2, `Readiness.cs`, `Program.cs` y migraciones nuevas 009–015. 001–008 y los componentes protegidos D01 conservan sus hashes.
- Web: `frontend/session_store.py`, `seguridad.py`, `login_protection.py`, `observabilidad.py`, `app.py`, reportes y plantillas. `api_call` distingue JSON 401/403 de HTML del perímetro; un bloqueo ajeno devuelve 503 sin perder la sesión.
- Pruebas: suites .NET/Flask/HTTP/SQL, navegador/zoom y casos de sesiones PostgreSQL. El PDF compara contenido normalizando saltos de línea del diseño; el texto íntegro sigue exigido.
- Infraestructura/CI: dependencias fijadas, Dockerfiles/Compose, salud TCP de PostgreSQL, `frontend/vercel.json`, `.python-version`, `.vercelignore`, roles runtime y ejemplo Render Free. La corrección forwarding del workflow permanece intacta.
- Documentación: matriz de 104 registros, manuales, guía de sustentación, informes y evidencia saneada. No se publica .env, .local, .vercel, OIDC, claves, DSN, dumps ni credenciales QA.

## C. Commits publicados

| SHA | Responsabilidad |
|---|---|
| 3535b3c | API, requisitos y migraciones |
| de8dfc0 | Frontend y sesiones compartidas |
| ca1fb9e | Pruebas y regresión |
| 0ece0a8 | Herramientas de CI |
| e775868 | Infraestructura y despliegue |
| 630721a | Documentación y evidencia de cierre |
| ac4a839 | Exclusiones Vercel y diagnóstico del bloqueo Actions |
| e3f60e5 | Manejo de HTML 403 del perímetro y verificaciones remotas |

El historial añade un commit de documentación/evidencias finales posterior a este corte de código. Se publica por fast-forward; no se ejecutan commits sobre main.

## D. PR #7

[PR existente](https://github.com/francis872/Mira/pull/7), abierto, Draft, sin merge. Antes del commit documental final comparaba 59 commits/375 archivos con main: incluye la evolución histórica V1/V2/V3, no solo el trabajo actual. La descripción se actualiza con URL, pruebas y bloqueos efectivos. La revisión académica y los checks requeridos siguen pendientes.

## E. GitHub Actions

[Ejecución comprobada 43350345420](https://github.com/francis872/Mira/actions/runs/43350345420) en `e3f60e5e78381432c885e68cf8b24376c303aedb`: ambos jobs terminan en failure con **cero pasos ejecutados**. Mensaje literal: «The job was not started because your account is locked due to a billing issue». También ocurrió en 43345193087 y 43347296447. [Anotaciones obtenidas por API](cierre-v1-v3/evidencias/fase6-20261112/github-actions.json).

El repositorio es público y usa ubuntu-24.04 estándar. Las consultas de facturación disponibles devolvieron 404; no revelan saldo ni ofrecen desbloqueo. No se presume deuda. La [documentación oficial](https://docs.github.com/es/billing/how-tos/troubleshooting/locked-account) explica que una retención de autorización fallida puede afectar también acceso gratuito. Se preparó [solicitud para el titular/soporte](CI_BLOQUEO_CUENTA.md), no enviada. No se cambian tarjetas, planes o presupuestos ni se simula CI verde. El desbloqueo requiere intervención del titular en el panel o de GitHub.

Las pruebas locales aprobadas no sustituyen Actions. **Este bloqueo impide declarar completada la misión y fusionar el PR.**

## F. Arquitectura desplegada

Navegador HTTPS → Flask/Jinja en Vercel → API ASP.NET Core Docker en Render → Neon PostgreSQL. Flask accede únicamente a `mira_web.sesion` para su sesión compartida. No se ejecuta .NET/Docker en Vercel ni se migró a otro framework. [Arquitectura, variables y recuperación](FASE6_DESPLIEGUE.md).

Cookie opaca, SID rotado, SHA256 de clave y contenido de sesión firmado HMAC. JWT permanece en servidor. La base/backup requieren protección: la firma no cifra el contenido. El ingreso Vercel valida el origen sobrescrito por el proveedor y el protocolo D01 firmado permanece en la API.

## G. Servicios y proveedores

| Proveedor | Recurso real | Plan/configuración |
|---|---|---|
| Vercel | sapiens-v1-v3-20261112, prj_gtf7LJGJmEH9VpM6uSaIYe2iPhNC | Hobby, Flask/Python 3.14; secretos por entorno |
| Render | srv-dbr6i3dckfvc73dc3hr0 | Free, Docker, Virginia, /ready, autodeploy desactivado |
| Neon | snowy-bonus-50718377, base sapiens | Free, PostgreSQL 16.16, aws-us-east-1; roles separados |

Todos son recursos nuevos de este proyecto. Ninguna base preexistente se modificó. Preview/Production comparten esta API/base de demostración; **no representan aislamiento de datos para una institución real**. Los registros QA se identifican como ficticios y no tienen valor académico.

## H. URL de Vercel

- Production comprobada: https://sapiens-v1-v3-20261112.vercel.app
- Despliegue Production: `dpl_CZ8PmvmSDaceuD4N8HjPFHyEcBaC`, https://sapiens-v1-v3-20261112-awbg19h9g.vercel.app.
- Preview estable comprobada: https://sapiens-preview-v1-v3-20261112.vercel.app.

La CLI asignó automáticamente el primer despliegue al alias Production antes de configurar ese entorno; ese intento no se validó ni se ocultó. Tras validar Preview se configuraron secretos Production distintos y se reemplazó el alias por el despliegue probado. El staging contiene 51 archivos de producto rastreados, cotejados con Git, sin .env/cachés. [Manifest](cierre-v1-v3/evidencias/fase6-20261112/fuentes-desplegadas.json).

## I. URL de API

https://sapiens-api-v1-v3-20261112.onrender.com. `/health` y `/ready` 200; `/api/universidad` anónimo 401; `/swagger/index.html` 404. Correlación generada por servidor. Release .NET se construyó desde `630721a`; los commits funcionales posteriores solo cambian web/pruebas y no el producto API. JWT normal: 20 minutos. Deploy activo registrado: `dep-dbr6nft6e07s73an9bd0`.

Reinicio real autorizado por API Render: respuesta 200, instancia `srv-dbr6i3dckfvc73dc3hr0-mvw44` reemplazada por `srv-dbr6i3dckfvc73dc3hr0-dvgqw`; readiness, el mismo JWT y la misma cookie volvieron a 200. No se afirma cero interrupciones. El cambio de despliegue Flask conservó la sesión a través del alias Preview. [Reinicio](cierre-v1-v3/evidencias/fase6-20261112/reinicio-api.json), [redeploy web](cierre-v1-v3/evidencias/fase6-20261112/redeploy-web.json).

## J. PostgreSQL

Destino verificado vacío; backup pg_dump -Fc y restauración en otra base aislada comprobados antes de aplicar **001–015 atómicamente**. SHA del backup inicial `7cba6987514d9241c0c0315112f715fea745b4d89e1502064cf4c00f0e262ef9`. Cliente TLS verify-full y channel binding requerido; `pg_stat_ssl` describe el enlace interno de Neon, no el TLS del cliente al proxy.

`mira_migrador` administra DDL; `mira_api` tiene DML/ejecución de dominio, sin DDL ni edición de schema_migrations; `mira_web` solo CRUD de la tabla de sesión. Intentos cruzados/DDL fueron denegados. [Migraciones y privilegios](cierre-v1-v3/evidencias/fase6-20261112/neon-migraciones.json).

El endpoint informa `suspend_timeout_seconds=0` (valor predeterminado del plan). Cambiarlo explícitamente a 300 devolvió 412: «modifying the suspend interval is not permitted on this account». Neon documenta suspensión tras cinco minutos de inactividad, fija en Free; no se contrató otro plan ni se deshabilitó esa protección. [Fuente oficial](https://neon.com/docs/introduction/scale-to-zero). Además del respaldo vacío previo, se generó una copia con datos QA (SHA256 `27e85a0829d3fdad3debf137079c98da3bf8877175eedde3f542024a9d1604d8`). Su restauración directa en PostgreSQL estándar rechazó `pg_attver`. Una copia SQL derivada, omitiendo solo 125 ALTER COLUMN SET (pg_attver=...), restauró 42 tablas públicas, 15 migraciones, 3 cuentas QA, 2 personas QA y la tabla de sesiones en un clúster nuevo. No se alteraron COPY ni otras instrucciones; el dump original se conserva intacto. [Evidencia y límites](cierre-v1-v3/evidencias/fase6-20261112/neon-recuperacion-con-datos.json). No se probó la restauración de ese dump sin cambios dentro de Neon ni la aplicación completa contra el destino restaurado; falta programar y ensayar backups periódicos.

## K. Pruebas remotas

| Alcance | Resultado y límite |
|---|---|
| Production HTTPS → Flask → API → Neon | **49 aprobados, 1 timeout TLS, 0 omitidos; repetición ODS 3/3 aprobada, cobertura de 50 criterios**, CRUD, asociaciones, maestro-detalle, mínimos, estados, filtros, XLSX/PDF y persistencia |
| Seguridad/operación Production | **14 checks aprobados**: health/readiness, login, password incorrecto, anónimo, roles web, cookies/rotación, CSRF, logout/revocación, Swagger cerrado |
| Preview funcional | Primera pasada 47 aprobados/3 fallos/1 omitido; se conservaron los resultados. Tras correcciones, 49 aprobados y un timeout de lectura GET; repetición ODS 3/3 aprobada. Cobertura final de 50 criterios, sin presentar la corrida inicial como limpia |
| Perímetro | Cloudflare rechaza la carga maliciosa antes de API; web devuelve 503, conserva sesión, no persiste ese dato. Este caso se documenta separado del test local que exige persistencia y escape; no se fuerza el firewall |
| Expiración real | JWT emitido con 60 s llegó a 401 esperando el tiempo real, sin alterar firma/reloj; API normal restaurada a 20 min. Se ejecutó durante Preview contra la misma API; no se repitió una espera de 20 min en Production |
| Reinicio/redeploy | Mismo JWT/SID operativos tras reinicio Render y mismo SID tras nuevo despliegue Flask en Preview |
| Navegador Preview | 110 responsive, 38 axe (0 infracciones, 2 incomplete), 10 grupos de teclado, 5 académicos, 51 zoom real 100/200/400% |
| Navegador Production | 11 combinaciones escritorio/tableta/móvil emulados, 8 axe sin infracciones; capturas publicadas. No equivale al recorrido asistivo completo |

Localmente: **138 .NET, 135 Flask, 51 HTTP, 11 SQL**, y repetición de los mismos 51 HTTP en Linux/Gunicorn con sesiones PostgreSQL, todos aprobados. No sumar Docker como casos distintos. OSV consultó 25 paquetes del entorno limpio sin avisos; NuGet dos proyectos sin advisories reportados. No es auditoría completa de SO/base ni garantía futura.

Los timeouts/DNS del cliente Windows se registran; solo se permitieron reintentos de establecimiento, nunca respuestas/lecturas o escrituras ambiguas. El fallo PDF inicial era un salto de línea de diseño: se conserva la comparación de todas las palabras/acentos. [Resultados locales](cierre-v1-v3/evidencias/fase6-20261112/resultados.json), [resultados remotos y casos](cierre-v1-v3/evidencias/fase6-20261112/remoto.json), [comandos](cierre-v1-v3/evidencias/fase6-20261112/comandos.json).

## L. Seguridad

| Operación | Archivo, clase/método |
|---|---|
| Generación bcrypt coste 12 | MIRA.Api/Servicios/Seguridad/AuthService.cs: PasswordService.Hash; usado por Crear/Restablecer/Bootstrap/RestablecerAdmin |
| Verificación bcrypt | Mismo archivo: PasswordService.Verify y AuthService.Login; hash dummy para cuenta inexistente/formato inválido |
| Generación JWT HS256 | Mismo archivo: AuthService.GenerarJwt; registra jti antes de responder |
| Firma/issuer/audience/exp JWT | MIRA.Api/Configuracion/SeguridadConfiguration.cs: AddAuthentication/AddJwtBearer, TokenValidationParameters |
| Sesión activa y roles actuales | JwtBearerEvents.OnTokenValidated → AuthService.ValidarSesion → CuentaRepository.Sesion; compara roles y consulta jti/cuenta activa |
| Lectura de roles | MIRA.Api/Repositorios/Seguridad/CuentaRepository.cs: Columns/Buscar/Sesion/Roles; usuario_rol JOIN rol activo |
| Autorización | Políticas de SeguridadConfiguration, atributos de controladores y guardas frontend; el backend decide |

Un traceback privado mostrado durante la inspección contenía una cookie QA; se invalidó esa sesión y se comprobó que su cookie antigua vuelve a login302. No se publica el log crudo. No se identificaron secretos de alta confianza en el árbol publicable; se revisaron también coincidencias de los valores runtime privados sin imprimirlos. No equivale a una garantía absoluta. Logs de correlación omiten cuerpo, query, cabeceras y cookies. Sesión falla cerrada con 503 ante almacenamiento caído. Contraseñas privadas y OAuth permanecen en archivos ignorados y gestores oficiales; nunca en este informe.

## M. Riesgos pendientes

Actions bloqueado y revisión PR pendiente. V2: C06 (22 FK obligatorias), Facultad, perfil propio cuenta–docente y periodo/líder requieren definición/aceptación, sin inventar datos. Tres roles cerrados frente al enunciado de roles dinámicos requiere decisión académica. Dos incomplete axe, NVDA/VoiceOver, dispositivos físicos y contenidos largos institucionales pendientes. Carga/concurrencia a escala, cuotas/cold starts, políticas del perímetro, dependencias CDN, sistema operativo/base y restauraciones periódicas y recuperación nativa de la copia con datos en Neon no certificados. Preview y Production no aíslan datos; separar recursos antes de uso institucional real.

## N. Costos o recursos contratados

Solo Vercel Hobby, Render Free y Neon Free, comprobados. No se contrataron planes de pago, dominios, discos, réplicas ni monitorización de pago. No se modificó facturación GitHub ni se conoce su saldo. Mantenerse en cuotas y revisar condiciones oficiales; no existe garantía de servicio continuo por esta validación.

## O. Estado final V1, V2 y V3

| Versión/aceptación | Dictamen |
|---|---|
| V1 | Operativa y verificada local/remotamente en el alcance implementado; lista para revisión académica |
| V2 | Funciones implementadas operativas y verificadas; **cierre académico incompleto** por definiciones/criterios pendientes |
| V3 | Autenticación/roles/sesiones/CSRF/revocación y adaptación serverless verificadas local y remotamente |
| Production | URL y pruebas funcionales/de seguridad remotas aprobadas en el entorno gratuito de demostración; **sin certificación institucional de carga, accesibilidad o disponibilidad** |
| Entrega/misión | Preparada para demostración y revisión; **no entrega definitiva ni merge** hasta liberar/aprobar CI y resolver revisión/aceptación pendientes |

La [matriz de 104 registros](cierre-v1-v3/02_matriz_trazabilidad.md) mantiene fuentes, archivos, métodos, pruebas y límites. El [recorrido de diez minutos](cierre-v1-v3/11_guia_sustentacion.md) incluye los seis escenarios de autenticación. No se inició V4 ni se alteró otro proyecto.
