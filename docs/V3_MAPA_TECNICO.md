# Mapa técnico V3

> Seguimiento local 2026-11-07: [cierre académico y propuesta sin publicar](V3_CIERRE_ACADEMICO.md). Este documento conserva su alcance histórico; el PR #7 publicado contiene ocho commits V3 más 43 anteriores. Las nuevas correcciones y CI requieren autorización de publicación.

La [matriz final R01–R30 y S01–S08](V3_REVISION_FINAL.md) amplía este mapa con capa, cobertura y evidencias de estabilización. Producción: `DbConnectionFactory` constructor valida TLS sin abrir conexión; `Program` resuelve PORT; `seguridad.configure_session` impone HTTPS/directorio privado; `gunicorn.conf.py` fija un worker/cuatro threads. Pruebas: ProductionConfigurationTests, test_production.py y verificadores Docker/Production. No hay validación remota Render/Neon.

Las rutas de archivos son relativas a la raíz. La evidencia ejecutada y sus límites se registran en V3_PRUEBAS.md.

| Requisito | Archivo / clase / método | Responsabilidad y dependencias | Endpoint | Prueba | Estado |
|---|---|---|---|---|---|
| Login | `MIRA.Api/Peticiones/AuthController.cs`, AuthController.Login | Contrato HTTP, AuthService | POST /api/auth/login | SeguridadTests.BadLoginIs401 / Login_Read_ForbiddenWrite_Logout_Revoked | Automatizado: aprobado |
| Validación de entrada | `MIRA.Api/Modelos/Seguridad.cs`, LoginRequest/CrearCuentaRequest/PasswordRequest | DataAnnotations; MVC serializa JSON | login/usuarios | SeguridadDatabaseTests.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration | Automatizado: aprobado |
| bcrypt | `MIRA.Api/Servicios/Seguridad/AuthService.cs`, PasswordService.Hash/Verify | BCrypt.Net-Next; coste 12, límites UTF-8 | login/crear/restablecer | SeguridadTests.BcryptUsesSaltAndRejectsPlaintextAndWrongPassword | Automatizado: aprobado |
| Generación JWT | `MIRA.Api/Servicios/Seguridad/AuthService.cs`, AuthService.GenerarJwt | JwtSecurityTokenHandler + JwtSettings + repositorio | login | SeguridadTests.Login_Read_ForbiddenWrite_Logout_Revoked | Automatizado: aprobado |
| Firma y claims | `MIRA.Api/Servicios/Seguridad/AuthService.cs`, JwtSettings.Validation | TokenValidationParameters | todos los protegidos | SeguridadTests.InvalidJwtClaimsAre401 | Automatizado: aprobado |
| Validación y revocación | `MIRA.Api/Configuracion/SeguridadConfiguration.cs`, AddSeguridad / OnTokenValidated; AuthService.ValidarSesion/Revocar | JWT bearer y CuentaRepository.Sesion/Revocar | me/logout/datos | SeguridadTests.InactiveAccountAndRoleChangesInvalidateSession; SeguridadDatabaseTests | Automatizado: aprobado |
| RBAC | `MIRA.Api/Configuracion/SeguridadConfiguration.cs`, AddSeguridad, política Datos; AuthController atributos Authorize | ASP.NET Core Authorization | GET/POST/PUT/DELETE | SeguridadTests.RoleMatrix | Automatizado: aprobado |
| Acceso PostgreSQL | `MIRA.Api/Repositorios/Seguridad/CuentaRepository.cs`, Buscar/Sesion/Crear/Editar/Inactivar/Restablecer | IDbConnectionFactory, Dapper, procedimientos | usuarios/roles | SeguridadDatabaseTests | Automatizado: aprobado |
| Aprovisionamiento | AuthService.Bootstrap; CuentaRepository.Bootstrap; `MIRA.Api/Program.cs` rama --bootstrap-admin | sp_usuario_bootstrap_admin, variables externas | CLI, sin endpoint | ejecución aislada de verify-postgres16.ps1 | Automatizado: aprobado |
| Persistencia de jti | `database/migrations/008_v3_sesiones.sql` | tabla sesion_acceso, índices, procedimientos sp_v3_usuario_actualizar/password | interno | migración en base aislada; SeguridadDatabaseTests | Automatizado: aprobado |
| Gestión de cuentas | AuthController.Usuarios/Crear/Editar/Inactivar/Restablecer/Roles | AuthService, repositorio | /api/auth/usuarios, /roles | SeguridadDatabaseTests y SeguridadTests.RoleMatrix | Automatizado: aprobado |
| Sesión Flask | `frontend/seguridad.py`, configure_session / register_security.login / protect_routes / logout | Flask-Session, CacheLib, api_call | /login /logout y rutas privadas | frontend/tests/test_seguridad.py: test_login_rotates_identifier_and_never_exposes_jwt / test_anonymous_and_expired_sessions_redirect_to_login / test_logout_revokes_before_clearing_session | Automatizado: aprobado |
| Errores del cliente | `frontend/app.py`, api_call/error_message | requests, sesión, SesionVencida | todos | test_app.py / test_seguridad.py | Automatizado: aprobado |
| Administración web | `frontend/seguridad.py`, usuarios/editar_usuario/inactivar_usuario/password_usuario; `frontend/templates/usuarios.html` | API centralizada, CSRF | /usuarios | test_users_page_never_renders_password_hash | Automatizado: aprobado |
| Iconografía | `frontend/templates/includes/icons.html`, macro icon; `frontend/static/icons/tabler.svg` | SVG Tabler local | HTML | verificación de plantillas y navegador | Navegador: ver V3_PRUEBAS.md |
| Diseño | `frontend/templates/base.html`, `frontend/templates/login.html`, `frontend/static/mira.css` | Jinja/Bootstrap, foco, responsive | todas | navegador, V3_PRUEBAS | Navegador: ver V3_PRUEBAS.md |
| Errores HTTP | `MIRA.Api/Excepciones/ApiExceptionMiddleware.cs`, InvokeAsync; `MIRA.Api/Program.cs` InvalidModelStateResponseFactory | excepciones de dominio/PostgreSQL | todos | ErroresPersistenciaTests | Automatizado: aprobado |
| Límite login | `MIRA.Api/Configuracion/SeguridadConfiguration.cs`, AddSeguridad | FixedWindowRateLimiter por IP | login | SeguridadTests.LoginRateLimitReturns429 | Automatizado: aprobado |
| Swagger Bearer y login público | `MIRA.Api/Configuracion/SwaggerAnonymousOperationFilter.cs`, SwaggerAnonymousOperationFilter.Apply; `MIRA.Api/Program.cs`, AddSwaggerGen | Microsoft.OpenApi y Swashbuckle | /swagger/v2/swagger.json | SeguridadTests.SwaggerDocumentsBearerAndAnonymousLogin | Automatizado: aprobado |
| Recuperación operativa heredada | `MIRA.Api/Servicios/Seguridad/AuthService.cs`, AuthService.RestablecerAdmin; `MIRA.Api/Program.cs`, rama --reset-admin-password; `scripts/bootstrap-admin.ps1`, -ResetPassword | CuentaRepository.Restablecer; acceso PostgreSQL del operador | CLI, sin endpoint público | SeguridadDatabaseTests.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration | Servicio y persistencia aprobados |
| Catálogos V1 | seis controladores en Peticiones, sus servicios y repositorios | rutinas 004; trigger 005 | /api/area_conocimiento, /objetivo_desarrollo_sostenible, /area_aplicacion, /termino_clave, /universidad, /linea_investigacion | CatalogCrudRegressionTests | Automatizado: aprobado |
| Investigación V2 | `MIRA.Api/Peticiones/InvestigacionController.cs`, Listar/Crear/Actualizar/Eliminar/CrearFicha; `MIRA.Api/Servicios/InvestigacionService.cs`; `MIRA.Api/Repositorios/Investigacion/InvestigacionRepository.cs` | rutinas 007, modelo 006 | /api/investigacion/* | InvestigacionIntegrationTests; frontend/tests/e2e/test_investigacion.py | Automatizado: aprobado |

Las regresiones .NET V1/V2 usan `TestHostFactory.Create(authenticated:true)` con identidad Administrador explícita para probar reglas de negocio sin depender del login. Las pruebas de seguridad usan `authenticated:false` con el pipeline Bearer real, y la integración de seguridad usa PostgreSQL real. No hay bypass de autorización en la aplicación productiva.

## Navegación exacta de catálogos conservados

Los controladores siguientes exponen GetAll/GetById/Create/Update/Delete; sus servicios y repositorios exponen GetAllActivosAsync/GetByIdAsync/CreateAsync/UpdateAsync/SoftDeleteAsync. Las capas dependen de sus interfaces I* y de IDbConnectionFactory, respectivamente. CatalogCrudRegressionTests verifica sus operaciones contra PostgreSQL y frontend/tests/e2e/test_local_platform.py verifica sus formularios. Estado: aprobado en la pasada integrada V3.

| Recurso / endpoint base | Controlador | Servicio | Repositorio |
|---|---|---|---|
| /api/area_conocimiento | `MIRA.Api/Peticiones/AreaConocimientoController.cs` (AreaConocimientoController) | `MIRA.Api/Servicios/AreaConocimientoService.cs` (AreaConocimientoService) | `MIRA.Api/Repositorios/AreaConocimientoRepository.cs` (AreaConocimientoRepository) |
| /api/objetivo_desarrollo_sostenible | `MIRA.Api/Peticiones/ObjetivoDesarrolloSostenibleController.cs` (ObjetivoDesarrolloSostenibleController) | `MIRA.Api/Servicios/ObjetivoDesarrolloSostenibleService.cs` (ObjetivoDesarrolloSostenibleService) | `MIRA.Api/Repositorios/ObjetivoDesarrolloSostenibleRepository.cs` (ObjetivoDesarrolloSostenibleRepository) |
| /api/area_aplicacion | `MIRA.Api/Peticiones/AreaAplicacionController.cs` (AreaAplicacionController) | `MIRA.Api/Servicios/AreaAplicacionService.cs` (AreaAplicacionService) | `MIRA.Api/Repositorios/AreaAplicacionRepository.cs` (AreaAplicacionRepository) |
| /api/termino_clave | `MIRA.Api/Peticiones/TerminoClaveController.cs` (TerminoClaveController) | `MIRA.Api/Servicios/TerminoClaveService.cs` (TerminoClaveService) | `MIRA.Api/Repositorios/TerminoClaveRepository.cs` (TerminoClaveRepository) |
| /api/universidad | `MIRA.Api/Peticiones/UniversidadController.cs` (UniversidadController) | `MIRA.Api/Servicios/UniversidadService.cs` (UniversidadService) | `MIRA.Api/Repositorios/UniversidadRepository.cs` (UniversidadRepository) |
| /api/linea_investigacion | `MIRA.Api/Peticiones/LineaInvestigacionController.cs` (LineaInvestigacionController) | `MIRA.Api/Servicios/LineaInvestigacionService.cs` (LineaInvestigacionService) | `MIRA.Api/Repositorios/LineaInvestigacionRepository.cs` (LineaInvestigacionRepository) |
