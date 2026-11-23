# Cambios respecto a V2

Fecha: 2026-11-05. Rama `feature/mira-v3`. No se borra historial ni se modifican migraciones 001–007.

| Área / archivos | Cambio y motivo |
|---|---|
| `MIRA.Api.csproj`, `Program.cs`, `Configuracion/SeguridadConfiguration.cs` | JwtBearer/BCrypt.Net-Next, validación JWT, autorización global, límite de login, Swagger Bearer y aprovisionamiento CLI. |
| `Configuracion/SwaggerAnonymousOperationFilter.cs`, `static/favicon.svg` | Swagger deja login sin requisito Bearer; favicon local evita solicitudes inexistentes desde el navegador. |
| `Modelos/Seguridad.cs`, `Peticiones/AuthController.cs` | Contratos de autenticación/cuentas, respuestas HTTP, endpoints administradores. |
| `Servicios/Seguridad/AuthService.cs` | bcrypt, generación JWT, comprobación/revocación de sesión y coordinación de cuentas. |
| `Repositorios/Seguridad/CuentaRepository.cs` | Acceso parametrizado a cuentas/rutinas heredadas y sesiones. |
| `database/migrations/008_v3_sesiones.sql` | Registro de sesiones e invalidación atómica al modificar cuenta o contraseña. Aditiva; datos heredados no se convierten. |
| `Excepciones/ApiExceptionMiddleware.cs` | 400 de seguridad y 404 para registro inexistente P0002. |
| `frontend/app.py`, `seguridad.py`, `requirements.txt` | Sesión del servidor, Bearer centralizado, CSRF conservado, login/logout/cuentas, errores de formularios con estado real. |
| `templates/login.html`, `usuarios.html`, `base.html` | Acceso institucional, gestión de cuentas y sesión visible. |
| `static/mira.css`, plantillas de catálogos e investigación | Identidad monocromática, responsive conservado, controles según permisos, SVG centralizados. |
| `templates/includes/icons.html`, `static/icons/*` | Tabler 3.34.1 local con licencia; elimina dependencia de Bootstrap Icons. React no se introduce. |
| `tests/MIRA.Api.Tests/SeguridadTests.cs`, `SeguridadDatabaseTests.cs`, `TestHostFactory.cs` | Pruebas del pipeline real y persistencia; regresiones antiguas usan identidad administradora explícita. Los casos antiguos que exigían ausencia de login se sustituyen por requisitos V3. |
| `frontend/tests/test_app.py`, `test_investigacion.py`, `test_seguridad.py`, `tests/e2e/*`, `pytest.ini` | Conserva flujos de negocio y añade autenticación, sesión, CSRF y errores HTTP. pytest funciona también desde raíz. |
| `.env.example`, `docker-compose.yml`, `scripts/start-local.ps1`, `bootstrap-admin.ps1`, verificadores | Configuración JWT y aprovisionamiento seguro; base de validación separada. Se conserva nombre/volumen Compose para evitar perder datos. |
| `docs/V3_*`, `versiones/v3/2_spec.md`, README | Auditoría previa, contratos, matriz, arquitectura, sustentación y evidencia verificable. |

## Cambios de contrato

Las rutas y JSON de datos V1/V2 se conservan. Ahora requieren Bearer: clientes antiguos reciben 401 hasta autenticarse; operaciones sin permisos reciben 403. Se añaden rutas `/api/auth/*` y `/login`, `/logout`, `/usuarios`. Formularios V1 que fallaban devolviendo 200 ahora propagan 400/409/502/503 según el fallo. Swagger conserva URL del documento `/swagger/v2/swagger.json` para clientes existentes, con versión descriptiva v3 y esquema Bearer.

## Credenciales heredadas y operación

No se convierte ni se publica ningún password existente. Los hashes bcrypt admitidos se verifican; otros formatos se rechazan. Un administrador debe restablecer la contraseña por `/usuarios` o PUT `/api/auth/usuarios/{id}/password`, que guarda bcrypt y revoca sesiones. No se implementa recuperación por correo ni registro público. El administrador inicial se aprovisiona una sola vez con un procedimiento con bloqueo transaccional. Gestión de cuentas y personas académicas siguen separadas.

Si no hay un administrador con credencial verificable, la recuperación operativa de un administrador activo se realiza por CLI (`--reset-admin-password`, o `bootstrap-admin.ps1 -ResetPassword`) con acceso a PostgreSQL. No añade un endpoint remoto de recuperación.

## Conservación y límites

Los seis catálogos, once recursos, cinco asociaciones, ficha maestro-detalle, SQL parametrizado y triggers V2 se conservan. Las limitaciones académicas previas, incluida C06, se mantienen documentadas en `criterios-v2.md`; la seguridad V3 no resuelve discrepancias del SQL oficial. Resultados y límites de aceptación en V3_PRUEBAS.md.
