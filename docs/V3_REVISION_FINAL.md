# Revisión final de estabilización — MIRA V3

> Seguimiento local 2026-11-07: [cierre académico y propuesta sin publicar](V3_CIERRE_ACADEMICO.md). Este documento conserva su alcance histórico; el PR #7 publicado contiene ocho commits V3 más 43 anteriores. Las nuevas correcciones y CI requieren autorización de publicación.

Fecha: 2026-11-05, America/Bogota. Rama `feature/mira-v3`; HEAD de partida `95e2e0bf00d02497d2c8a42d1efe814a6b551d3a`. Sin commits, staging Git, push ni despliegue. Cambios previos conservados. La estabilización corrige pendientes existentes; no crea una fase funcional nueva.

**Dictamen:** preparada para sustentación local con observaciones académicas y manuales declaradas. Preparada técnicamente para iniciar un despliegue de pruebas controlado, sujeto a configurar y comprobar recursos reales. **Producción no validada ni aprobada.** C06 requiere aceptación del profesor; no se ha demostrado una FK opcional ausente del modelo oficial.

## Evidencia ejecutada

| ID | Artefacto o procedimiento | Resultado y alcance |
|---|---|---|
| E1 | `scripts/verify-postgres16.ps1 -Browser` → `verify-local.ps1`; TRX privado `tests/MIRA.Api.Tests/TestResults/v3-backend.trx` | 71 .NET, ninguna omitida; 78 Flask; 43 E2E aprobadas. PostgreSQL 16.15 exclusivo. El navegador inicial falló en el verificador y se repitió por separado, E3/E9. |
| E2 | `database/tests/*.sql`, migraciones 001–008 en bases nuevas | Cinco suites SQL aprobadas, incluidas cuatro de regresión V2 y la consulta C06: 22 columnas FK públicas, ninguna opcional. |
| E3 | [browser-results.json](evidencias/v3/browser-results.json), PNG del mismo directorio | 50 combinaciones de página/ancho: login en dos tamaños y ocho páginas en 1440/1024/768/640/390/320 px. Cero errores de consola y sin desbordamiento del documento. Edge simulado. |
| E4 | [accessibility-results.json](evidencias/v3/accessibility-results.json) | 18 comprobaciones axe-core 4.10.3, cero infracciones automáticas finales; 10 instancias incomplete (color-contrast, th-has-data-cells) con targets/motivo. No certifica WCAG. |
| E5 | [osv-estabilizado.json](evidencias/v3/osv-estabilizado.json) y [hallazgo anterior](evidencias/v3/dependencias-python.json) | OSV respondió desde Linux: 18 distribuciones runtime, cero avisos conocidos. Los ocho GHSA anteriores quedan corregidos para estas versiones; no se borra su evidencia histórica. |
| E6 | `dotnet list MIRA.sln package --vulnerable --include-transitive --no-restore` | Consulta NuGet previa de la misma fecha: API/pruebas sin avisos reportados. Paquetes .NET no cambiaron durante estabilización. |
| E7 | [secret-scan.json](evidencias/v3/secret-scan.json) | Revisión del árbol pendiente, comparación sin imprimir secretos locales y exclusiones Git. Literales ficticios de pruebas separados de secretos reales. No cubre todo el historial. |
| E8 | [revision-final.json](evidencias/v3/revision-final.json) | Resumen saneado con conteos y limitaciones; logs y configuración privada permanecen ignorados en .local. |
| E9 | [keyboard-results.json](evidencias/v3/keyboard-results.json) | Seis modales: Enter, Tab/Shift+Tab, Escape y retorno de foco; tabla desplazable con flecha; foco de validación y alta/baja de detalle; zoom visual 2x y reflow 640/320. Zoom de escritorio y lector pendientes. |
| E10 | [docker-images.json](evidencias/v3/docker-images.json), [docker-session-results.json](evidencias/v3/docker-session-results.json); `scripts/verify-docker.ps1` | Ambas imágenes construidas y sin root; export de /app sin rutas privadas ni literales JWT/claves privadas detectados. 43 E2E repetidas sobre Gunicorn/Linux/PG16. Reinicio conserva sesión; logout local y revocación del mismo JWT comprobados. Transporte HTTP local aislado. |
| E11 | [production-smoke.json](evidencias/v3/production-smoke.json); `scripts/verify-production-smoke.py` | Arranque Production local en PORT=10000, API health 200/datos sin token 401/Swagger 404; Flask health/login 200 y cookie Secure/HttpOnly/Strict. Destinos ficticios: no contacta BD/API ni prueba TLS real. |
| E12 | [render-schema.json](evidencias/v3/render-schema.json), [ejemplo Render](../deploy/render.yaml.example) | YAML validado contra JSON Schema público oficial. Sin validación de cuenta, branch remoto, costes, permisos ni aplicación del manifiesto. |

Logs privados: `.local/v3-stabilization-final.log` (SQL/.NET/Flask/E2E y primer fallo de navegador), `.local/v3-browser-final.log` (repetición final), `.local/v3-docker-final.log`, logs de build y OSV. No publicar logs, env, sesiones o TRX sin saneamiento. Los 43 E2E de Docker repiten el mismo conjunto: no son 86 casos distintos.

## Matriz Spec Kit V3 → implementación real

Fuente: `versiones/v3/2_spec.md`; plan: `versiones/v3/3_plan.md`. R01–R30 son identificadores de trazabilidad. TS = `tests/MIRA.Api.Tests/SeguridadTests.cs`; TD = `tests/MIRA.Api.Tests/SeguridadDatabaseTests.cs`; TF = `frontend/tests/test_seguridad.py`. La matriz identifica archivo, capa, clase/método, prueba y evidencia; una inspección estática no se presenta como test dedicado.

| ID / requisito | Archivo exacto | Capa | Clase / método o función | Prueba existente | Evidencia / estado |
|---|---|---|---|---|---|
| R01. Mantener tres capas | `MIRA.Api/Peticiones/AuthController.cs`, `MIRA.Api/Servicios/Seguridad/AuthService.cs`, `MIRA.Api/Repositorios/Seguridad/CuentaRepository.cs` | HTTP / servicio / repositorio | AuthController.Login → AuthService.Login → CuentaRepository.Buscar | `frontend/tests/test_architecture.py::test_catalog_repositories_only_call_stored_routines`; TD.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration | E1; código inspeccionado. Cumplido localmente. |
| R02. Correo/contraseña y respuesta 200 | `MIRA.Api/Modelos/Seguridad.cs`, `MIRA.Api/Peticiones/AuthController.cs`, `MIRA.Api/Servicios/Seguridad/AuthService.cs` | HTTP / servicio | LoginRequest; AuthController.Login; AuthService.Login | TS.Login_Read_ForbiddenWrite_Logout_Revoked; TD.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration | E1, 200 y JSON comprobados. |
| R03. Credencial incorrecta/inactiva 401 | `MIRA.Api/Servicios/Seguridad/AuthService.cs`, `MIRA.Api/Peticiones/AuthController.cs` | HTTP / servicio | AuthService.Login; AuthController.Login | TS.BadLoginIs401; TS.InactiveAccountAndRoleChangesInvalidateSession | E1. Cumplido. |
| R04. Límite de intentos | `MIRA.Api/Configuracion/SeguridadConfiguration.cs`, `MIRA.Api/Configuracion/LoginProtection.cs`, `frontend/login_protection.py` | middleware | LoginProtectionMiddleware; LoginOriginAuth; cuotas por origen autenticado/cuenta y concurrencia | LoginProtectionTests; frontend/tests/test_login_protection.py; regresión integrada D01 | Fase 3 local sin publicar: D01 corregido y reproducido con clientes independientes; diez solicitudes/minuto por origen y cuenta; cuatro trabajos concurrentes. [Condiciones y evidencia](V3_D01_LIMITACION_LOGIN.md). |
| R05. me con identidad actual | `MIRA.Api/Peticiones/AuthController.cs`, `MIRA.Api/Servicios/Seguridad/AuthService.cs` | HTTP / middleware | AuthController.Me; AuthService.ValidarSesion | TS.InactiveAccountAndRoleChangesInvalidateSession; TD.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration | E1. La revalidación ocurre antes del controlador. |
| R06. Logout con revocación real | `MIRA.Api/Servicios/Seguridad/AuthService.cs`, `MIRA.Api/Repositorios/Seguridad/CuentaRepository.cs` | servicio / repositorio / SQL | AuthService.Revocar; CuentaRepository.Revocar/Sesion | TS.Login_Read_ForbiddenWrite_Logout_Revoked; TD.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration | E1: mismo JWT pasa a 401 tras logout 200. |
| R07. Cuentas/roles solo administrador | `MIRA.Api/Peticiones/AuthController.cs` | HTTP / RBAC | Usuarios/Crear/Editar/Inactivar/Restablecer/Roles; atributos Authorize | TS.RoleMatrix; TD.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration; TF.test_users_page_never_renders_password_hash; TS.EveryAdministrativeActionRequiresAdministrator | E1/E3: seis acciones administrativas denegadas a Investigador y Coordinador; operaciones de administrador cubiertas por integración. Cumplido localmente. |
| R08. Hash bcrypt coste 12 | `MIRA.Api/Servicios/Seguridad/AuthService.cs` | servicio | PasswordService.Hash | TS.BcryptUsesSaltAndRejectsPlaintextAndWrongPassword; TD.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration | E1: sal diferente, verificación y persistencia, sin texto plano. |
| R09. Mínimo 12 caracteres / máximo 72 bytes | `MIRA.Api/Modelos/Seguridad.cs`, `MIRA.Api/Servicios/Seguridad/AuthService.cs` | DTO / servicio | CrearCuentaRequest/PasswordRequest; PasswordService.Hash/Verify | TS.BcryptUsesSaltAndRejectsPlaintextAndWrongPassword; TS.PasswordBoundariesAreEnforced | E1: fronteras 11/12/72/73 y exceso UTF-8 comprobados. Cumplido. |
| R10. No devolver hashes | `MIRA.Api/Modelos/Seguridad.cs`, `MIRA.Api/Repositorios/Seguridad/CuentaRepository.cs` | DTO / repositorio | Cuenta/SesionRespuesta; Listar/Crear/Editar | TD.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration; TF.test_users_page_never_renders_password_hash | E1. Hash solo en Credencial para servicio/repositorio. |
| R11. Bootstrap local y atómico | `MIRA.Api/Program.cs`, `MIRA.Api/Servicios/Seguridad/AuthService.cs`, `database/migrations/003_v2_usuarios_roles.sql` | CLI / servicio / SQL | rama --bootstrap-admin; AuthService.Bootstrap; sp_usuario_bootstrap_admin/fn_usuario_bootstrap_admin | ejecución real en `scripts/verify-postgres16.ps1`; `database/tests/v2_usuarios_roles.sql` | E1/E2. Sin endpoint público; procedimiento usa bloqueo transaccional. |
| R12. Recuperar administrador heredado | `MIRA.Api/Program.cs`, `MIRA.Api/Servicios/Seguridad/AuthService.cs`, `scripts/bootstrap-admin.ps1` | CLI / servicio / SQL | rama --reset-admin-password; AuthService.RestablecerAdmin; -ResetPassword | TD.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration | E1: servicio/SQL probados con hash inválido. El asistente interactivo PowerShell no se recorrió manualmente. |
| R13. health público / datos privados | `MIRA.Api/Program.cs` | HTTP / middleware | MapGet /health; MapControllers.RequireAuthorization("Datos") | TS.MissingOrInvalidTokenIs401; `frontend/tests/e2e/test_local_platform.py::test_login_surface_and_protected_api` | E1: health 200 y catálogo 401. Health es liveness, no comprueba Neon. |
| R14. Swagger Bearer en Development | `MIRA.Api/Program.cs`, `MIRA.Api/Configuracion/SwaggerAnonymousOperationFilter.cs` | HTTP / OpenAPI | AddSwaggerGen; SwaggerAnonymousOperationFilter.Apply | TS.SwaggerDocumentsBearerAndAnonymousLogin | E1: Bearer y login anónimo documentados; selector dice v3, URL v2 conservada por compatibilidad. E11: Swagger devuelve 404 en Production. |
| R15. Matriz de tres roles | `MIRA.Api/Configuracion/SeguridadConfiguration.cs`, `MIRA.Api/Servicios/Seguridad/AuthService.cs` | middleware / servicio | AddSeguridad, política Datos; RolesPermitidos | TS.RoleMatrix; TF.test_read_only_identity_hides_mutations_and_denies_direct_routes; TF.test_coordinator_cannot_delete_or_manage_users | E1: Investigador 403 en escritura; Coordinador 201/403; Administrador 201/200. |
| R16. Roles desconocidos no conceden acceso | `MIRA.Api/Servicios/Seguridad/AuthService.cs`, `MIRA.Api/Configuracion/SeguridadConfiguration.cs` | servicio / middleware | AuthService.Login; RolesPermitidos; política Datos | TS.UnknownRoleCannotLogin | E1: rol desconocido no obtiene JWT. Cumplido. |
| R17. Cuenta activa y roles actuales | `MIRA.Api/Repositorios/Seguridad/CuentaRepository.cs`, `MIRA.Api/Servicios/Seguridad/AuthService.cs`, `database/migrations/008_v3_sesiones.sql` | repositorio / SQL / middleware | CuentaRepository.Sesion; AuthService.ValidarSesion; sp_v3_usuario_actualizar | TS.InactiveAccountAndRoleChangesInvalidateSession; TD.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration | E1: modificación de roles invalida sesión; actividad se consulta en cada petición. |
| R18. Claims y firma JWT | `MIRA.Api/Servicios/Seguridad/AuthService.cs` | servicio / criptografía | AuthService.GenerarJwt; JwtSettings.Validation | TS.Login_Read_ForbiddenWrite_Logout_Revoked; TS.InvalidJwtClaimsAre401 | E1: sub/email/role/iat/exp/iss/aud/jti; HS256; firma, emisor, audiencia y tiempo comprobados. |
| R19. Clave externa, tolerancia mínima | `MIRA.Api/Configuracion/SeguridadConfiguration.cs`, `MIRA.Api/Servicios/Seguridad/AuthService.cs` | configuración / middleware | AddSeguridad; JwtSettings.Validation | TS.InvalidJwtClaimsAre401; configuración inspeccionada | E1 y estático: clave mínima, sin valor productivo por defecto; ClockSkew cero. No valida entropía de una clave elegida por un operador. |
| R20. JWT solo en servidor | `frontend/seguridad.py`, `frontend/app.py` | frontend / sesión | configure_session; register_security.login; _headers/api_call | TF.test_login_rotates_identifier_and_never_exposes_jwt; `frontend/tests/test_app.py::test_bearer_is_sent_only_from_server_to_api` | E1; no localStorage ni sessionStorage en frontend. |
| R21. Rotación, HttpOnly/Strict, Secure con HTTPS | `frontend/seguridad.py`, `frontend/app.py` | frontend / sesión | configure_session; register_security.login; configuración Flask | TF.test_login_rotates_identifier_and_never_exposes_jwt; `frontend/tests/test_app.py::test_session_cookie_is_httponly_and_samesite_strict`; frontend/tests/test_production.py::test_secure_cookie_is_httponly_strict_and_not_jwt | E1/E10/E11: rotación, flags HTTPS simulados y flags emitidos por Gunicorn comprobados; sesión persiste tras reinicio local. TLS real de Render pendiente. |
| R22. CSRF en todos los POST | `frontend/app.py` | frontend / HTTP | verify_csrf; _csrf_token; csrf_input | TF.test_login_error_and_csrf_are_preserved; `frontend/tests/test_app.py::test_every_rendered_form_embeds_the_csrf_token`; E2E CSRF | E1. Hook global también alcanza login y logout. |
| R23. Caducidad y 401/403 comprensibles | `frontend/seguridad.py`, `frontend/app.py` | frontend / middleware | protect_routes/expired/forbidden; api_call | TF.test_anonymous_and_expired_sessions_redirect_to_login; test_backend_401_clears_local_session; test_backend_forbidden_is_not_rendered_as_success | E1: expiración simulada y respuestas reales; reloj de producción no probado. |
| R24. Sin redirección externa | `frontend/seguridad.py` | frontend / navegación | login/expired/logout, redirect(url_for(...)) | frontend/tests/test_production.py::test_login_ignores_external_redirect | E1: next externo ignorado y destino fijo /. Cumplido. |
| R25. Salida local aun con API fallida | `frontend/seguridad.py` | frontend / servicio | logout; protect_routes exceptúa logout del me previo | TF.test_server_outage_during_logout_reports_revocation_uncertainty; test_logout_revokes_before_clearing_session | E1: advertencia honesta si no puede confirmarse revocación. |
| R26. Sin refresh automático | `frontend/seguridad.py`, `MIRA.Api/Peticiones/AuthController.cs` | frontend / HTTP | protect_routes/login; rutas AuthController | TF.test_anonymous_and_expired_sessions_redirect_to_login | E1 y estático: se exige iniciar sesión nuevamente; no endpoint refresh. |
| R27. Identidad monocromática / responsive | `frontend/static/mira.css`, `frontend/templates/base.html`, `frontend/templates/login.html` | presentación / CSS | reglas CSS; plantillas Jinja | `scripts/verify-browser-v3.mjs::check/shot`; keyboard-results.json | E3/E4/E9: 50 combinaciones; seis modales por teclado, tablas y formulario; cero infracciones automáticas finales. Lectores y dispositivos reales pendientes. |
| R28. SVG Tabler sin React | `frontend/templates/includes/icons.html`, `frontend/static/icons/tabler.svg`, `frontend/static/icons/LICENSE` | presentación / SVG | macro icon; 31 symbol SVG | `scripts/verify-browser-v3.mjs::check` valida getBBox | E3: geometría renderizada; MIT conservada. |
| R29. Mantener V1/V2, asociaciones y triggers | `MIRA.Api/Peticiones/InvestigacionController.cs`, `MIRA.Api/Servicios/InvestigacionService.cs`, `MIRA.Api/Repositorios/Investigacion/InvestigacionRepository.cs`; catálogos detallados en V3_MAPA_TECNICO | HTTP / servicio / repositorio / SQL | CRUD; InvestigacionService.GuardarFicha; rutinas 004–007 | CatalogCrudRegressionTests; InvestigacionIntegrationTests; ambos archivos E2E | E1/E2: seis catálogos, once recursos, cinco puentes, ficha y rollback. No cambia 001–007 ni controladores/servicios/repositorios V1/V2. |
| R30. Mantener separados cuentas/personas | `MIRA.Api/Repositorios/Seguridad/CuentaRepository.cs`, `database/migrations/003_v2_usuarios_roles.sql`, `database/migrations/006_v2_modelo_oficial.sql` | repositorio / SQL | CuentaRepository.Buscar usa usuario; modelo docente usa usuarios | InvestigacionIntegrationTests.NullsDatesAndEnumerationsAreValidated_AndPersonReferenceDoesNotExposeCredentials | E1/E2 y estático. No se fusionan identificadores ni se crea docente al crear cuenta. |

Los treinta requisitos tienen implementación identificada; las limitaciones operativas de R04/R19/R21 y la revisión humana de R27 siguen explícitas. El cumplimiento local no equivale a aceptación académica ni certificación cloud.

## Correcciones de los pendientes

| ID | Archivo / clase / método / capa | Prueba y evidencia | Estado |
|---|---|---|---|
| S01 Dependencias | `frontend/requirements.txt`, `requirements.lock`; runtime | E1/E5/E10; resolución fijada y pip check en build | Corregido y auditado Linux. |
| S02 Docker | ambos `.dockerignore`, Dockerfiles; build/runtime; `verify-image-content.py::main` | E10: contextos excluyen .local/.env/sesiones/dumps/keys, COPY limitado; export sin hallazgos y usuario no root | Corregido localmente. No es auditoría de CVE del SO. |
| S03 WSGI | `frontend/gunicorn.conf.py`; configuración Linux / Gunicorn | `test_production.py::test_gunicorn_uses_assigned_port_and_one_worker`, E10/E11 | Gunicorn, 1 worker/4 threads; puerto configurable. app.run bloqueado en producción. |
| S04 Sesiones | `seguridad.py::configure_session`; CacheLib / volumen Compose | tests de reinicio y cookies en test_production; E10/E11 | Directorio privado explícito, volumen, una instancia. Réplicas no soportadas. |
| S05 Producción | `Program.cs`, `DbConnectionFactory` constructor; configuración | `ProductionConfigurationTests`, E11/E12 | PORT y guardas VerifyFull/ChannelBinding Require/ErrorDetail false comprobados. TLS Neon real pendiente. |
| S06 C06 | fuente docente, `database/tests/v3_c06_nulabilidad.sql`; SQL | E2, catálogo pg_constraint/pg_attribute | No hay FK opcional en el esquema confirmado; aceptación académica pendiente. |
| S07 Accesibilidad | `mira.css`, plantillas/listas/_campos, `investigacion.js`; presentación | E3/E4/E9, tests Flask | Nombres de modales, foco, tablas y contraste de transición corregidos. Revisión asistiva pendiente. |
| S08 Cobertura | `SeguridadTests`, `ProductionConfigurationTests`, `test_production.py`; pruebas | E1 | Roles desconocidos, todas las acciones admin, fronteras password, next externo y configuración. |

### Dependencias antes y después

| Paquete | Antes | Final verificado |
|---|---|---|
| requests | 2.32.5 | 2.33.0 |
| urllib3 | 2.6.3 | 2.8.0 |
| idna | 3.11 | 3.15 |
| certifi | 2026.2.25 | 2026.10.18 |

Se conservaron Flask 3.1.3 y Werkzeug 3.1.9. Se añadió Gunicorn 26.2.0 para Linux y se fijaron transitivas en requirements.lock; no se cambió Python global. La imagen inicial también contenía pip/setuptools con avisos: se retiraron instaladores y wheel del runtime después de instalar y ejecutar pip check. OSV final cubre los 18 paquetes Python presentes, no todo el sistema operativo ni futuras vulnerabilidades. El entorno Windows global antiguo no se actualizó: instalar el lock en un entorno limpio antes de ejecutar la entrega.

Las condiciones de los avisos anteriores incluían extracción ZIP de Requests, almacén de confianza certifi, IDNA con entrada especialmente construida y operaciones de proxy/redirección/streaming urllib3. No se afirmó explotación en MIRA. Fuentes y aliases conservados en el reporte anterior; [Requests](https://github.com/psf/requests/security/advisories/GHSA-gc5v-m9x4-r6x2), [certifi/OSV](https://osv.dev/vulnerability/GHSA-6q48-6c7w-9gw2), [idna/OSV](https://osv.dev/vulnerability/GHSA-65pc-fj4g-8rjx), [urllib3](https://github.com/urllib3/urllib3/security/advisories/GHSA-8988-9cw3-xx77).

### C06 sin alterar el modelo docente

`docs/prompts/PROMPT_MAESTRO_DEFINITIVO_MIRA_V2.md:406` exige «Permitir null cuando la clave foránea sea opcional.»; línea 707: «C06. Las claves opcionales admiten null.» El criterio es condicional. El SQL confirmado no declara FK opcionales; E2 verifica 22 columnas FK públicas NOT NULL, incluyendo cuentas/sesiones, no solo entidades docentes. `InvestigacionService.Validar` admite null para campos marcados opcionales, pero el test de texto/fechas opcionales no prueba una FK opcional. No se inventó una FK, ni se reescribieron migraciones 001–007 para aparentar cumplimiento. Si el profesor identifica una relación opcional distinta del SQL confirmado, habrá que formalizarla y probarla; hasta entonces no se declara aceptación de C06.

## Revisión de cambios y conservación V1/V2

Se revisaron archivos pendientes, diffs, configuración y evidencias. Git mantiene ignorados .env/.env.local/.local y resultados privados. Los ejemplos usan placeholders; fixtures usan dominios invalid.test y contraseñas ficticias. Las imágenes no incluyen esos fixtures. La eliminación de SinAutenticacionTests sustituye el contrato de acceso público previo por el contrato V3 autorizado; las reglas de negocio heredadas siguen cubiertas.

No cambiaron migraciones 001–007 ni controladores, servicios y repositorios V1/V2. E1/E2/E10 verifican seis catálogos, once recursos, cinco puentes, ficha maestro-detalle, triggers, integridad y rollback, con cuentas y personas en tablas distintas. No se aplicó SQL a la base habitual ni remota.

## Riesgos residuales y pendientes

| Prioridad | Riesgo / límite | Acción verificable pendiente |
|---|---|---|
| Antes de producción | Render/Neon no contactados; TLS, permisos, secretos remotos, pooler y dominio no probados | Staging autorizado: TLS válido/inválido, login/logout/401/403, cookie Secure en HTTPS, migraciones y reinicio. |
| Antes de producción | Sesiones filesystem de una instancia, capacidad limitada y disco sujeto a permisos/ciclo de vida | Confirmar disco privado escribible por UID 10001, persistencia tras redeploy y pérdida controlada de sesión; no escalar réplicas independientes. |
| Antes de producción | Sin prueba de carga, backup/restauración ni auditoría de CVE del SO/base completa | Validar recuperación, alertas, límites, parches base y comportamiento bajo concurrencia. |
| P2 operación | D01 corregido localmente: origen firmado, cuenta y concurrencia; confianza real de proxy y contadores por instancia pendientes | Configurar clave y proxies conocidos, probar topología real; no aceptar headers arbitrarios. [Detalle](V3_D01_LIMITACION_LOGIN.md). |
| P2 disponibilidad | /health es liveness y no comprueba BD/API; dependencia de CDN Bootstrap conservada | Probar readiness operacional, indisponibilidad/arranque frío y acceso CDN. |
| P2 reproducibilidad | Las bases Docker usan etiquetas; su contenido puede cambiar al reconstruir | Revisar CVE del SO y fijar digests al promover una imagen aprobada; IDs locales registrados en E10. |
| Académico | C06 condicional sin relación opcional en SQL oficial | Obtener decisión explícita del profesor; registrar relación/fuente si cambia la interpretación. |
| Manual | Lectores, zoom escritorio 200/400%, Safari/iOS/Android reales y reglas axe incompletas | Recorrido humano de la guía con resultado y dispositivo/lector concretos. |

Incidencias de ejecución registradas: DNS host OSV intermitente (consulta completada dentro de Docker), daemon Docker detenido (reiniciado), temporal pytest con ACL ajena (ruta exclusiva .local), espera de recarga/transición de Bootstrap corregida en el verificador. Npgsql 10 emite aviso recuperable de GSSAPI ausente en Linux: bootstrap/consultas sí completaron; PowerShell debe comprobar exit code sin tratar cualquier stderr como fallo. [Explicación del mantenedor](https://www.npgsql.org/doc/release-notes/10.0.html). El perfil Neon TLS puede usar GSS Encryption Mode=Disable; no se desactiva TLS.

## Preparación Render/Neon sin despliegue

[deploy/README.md](../deploy/README.md) describe variables, disco, roles, migraciones y verificación pendiente. [render.yaml.example](../deploy/render.yaml.example) define dos servicios Docker con autoDeployTrigger off, una instancia y secretos sync:false. Su plan/disco generarían coste si se aplicara: el ejemplo no fue aplicado. No se copiaron credenciales reales ni se crearon recursos.

API: Production, Jwt__Key externa >=32 bytes, issuer/audience coherentes, PORT=10000. ConnectionStrings__PostgreSql en formato Npgsql con VerifyFull, Channel Binding=Require e Include Error Detail=false. Frontend: MIRA_ENV=production, API_URL HTTPS con /api, SECRET_KEY externa, Secure true y SESSION_DIRECTORY absoluta persistente. Swagger no se ofrece por defecto en producción; la URL OpenAPI v2 local conserva compatibilidad y la etiqueta visual dice v3.

Render requiere bind 0.0.0.0 y expone PORT; los discos restringen el servicio a una instancia. [Servicios web](https://render.com/docs/web-services), [discos](https://render.com/docs/disks), [Flask](https://render.com/docs/deploy-flask), [Blueprint](https://render.com/docs/blueprint-spec). Neon/Npgsql documentan validación TLS: [Neon](https://neon.com/docs/guides/dotnet-npgsql), [Npgsql](https://www.npgsql.org/doc/security.html). E11/E12 son validación local/estructural, no validación del panel cloud.

## Plan de commits registrado antes de su publicación

| Orden | Título | Responsabilidad |
|---|---|---|
| 1 | docs(v3): registrar especificación y auditoría de partida | versiones/v3 y decisiones iniciales. |
| 2 | feat(api): añadir JWT revocable y autorización por roles | AuthController/servicio/repositorio/modelos/configuración, migración 008, paquetes .NET y pruebas asociadas. Preservar 001–007. |
| 3 | feat(frontend): integrar sesiones privadas y cuentas | seguridad.py, Bearer/CSRF/login/usuarios y regresiones. |
| 4 | style(ui): aplicar identidad y accesibilidad | Tabler/licencia/CSS/plantillas, foco, modales, reflow y verificaciones navegador. |
| 5 | fix(deps): corregir avisos Python y fijar runtime | requirements/lock/dev y auditor OSV. |
| 6 | chore(deploy): endurecer Docker y preparar WSGI Render | Dockerignore/Dockerfiles/Gunicorn/volúmenes, PORT/TLS guardas y pruebas; ejemplo Render y guía operativa. |
| 7 | test(local): consolidar verificación aislada | bootstrap/verificadores SQL/.NET/Flask/Docker, C06 y evidencias saneadas. |
| 8 | docs(review): cerrar matriz y sustentación | README, guía de diez minutos, revisión, pruebas, mapa y resultados finales. |

Varios archivos comparten responsabilidades; preparar hunks explícitos y revisar cada diff antes de cualquier futuro staging. No usar git add . ni adjuntar .local. La revisión original quedó sin commits; después se publicaron los ocho commits autorizados, hasta 3105273. La propuesta nueva del cierre académico sigue sin commits/push.

## Recomendación de entrega

**Sustentación local:** sí, con guía reproducible, seis casos de seguridad, preguntas fundadas en código y observaciones C06/accesibilidad declaradas. **Despliegue de pruebas:** preparado para iniciarlo con autorización y configuración privada; no existe staging validado. **Producción:** no lista para aprobación mientras falte evidencia real de TLS, Neon, almacenamiento/redeploy, operación y controles manuales. No quedan los ocho avisos Python conocidos en el runtime auditado; eso no certifica seguridad absoluta.
