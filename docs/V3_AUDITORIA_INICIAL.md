# Auditoría inicial MIRA V3

Fecha: 2026-11-05. Rama: `feature/mira-v3`. Árbol de trabajo limpio al inicio. Auditoría anterior a cambios de aplicación.

## Estado observado

- `MIRA.sln`: API ASP.NET Core net10.0 y xUnit. Dapper 2.1.89, Npgsql 10.0.3, Swashbuckle 10.3.0; no ORM ni migraciones EF.
- `frontend`: Flask 3.1.3, Werkzeug 3.1.9, requests 2.32.5, Jinja, Bootstrap 5.3.3 e iconos Bootstrap. No React ni package.json.
- `MIRA.Api/Peticiones`: seis controladores de catálogos y `InvestigacionController`. Servicios y repositorios separados. CRUD de catálogos invoca 30 funciones de 004; investigación usa rutinas JSONB de 007.
- PostgreSQL: migraciones ordenadas 001–007 y `schema_migrations`; scripts transaccionales. 003 contiene `usuario/rol/usuario_rol`, vista, funciones y procedimientos maestro-detalle; roles Administrador, Investigador y Coordinador. 006 contiene otro modelo, `usuarios/roles/usuario_roles`; `docentes` referencia `usuarios`. Son identidades diferentes y no deben fusionarse por coincidencia de ID.
- Triggers: `tr_area_aplicacion_normalizar` (005) y `semilleros_grupo_activo_trigger` (006). Once recursos, cinco asociaciones y ficha grupo-semilleros, además de seis catálogos V1.
- `frontend/app.py::api_call` centraliza requests y lo recibe `investigacion.register_investigacion`. No hay token, autenticación ni autorización. CSRF por formulario, cookie firmada HttpOnly/SameSite Strict, Secure configurable.
- `ApiExceptionMiddleware` traduce errores PostgreSQL sin exponer SQL; servicios validan entradas. `/health` y Swagger en Development.
- `.env`, `.env.local` y `.local` son locales. No se imprimieron secretos durante la auditoría.

## Pruebas iniciales ejecutadas

- `dotnet test MIRA.sln --no-restore`: 14 aprobadas, 13 omitidas, 0 fallidas. Integraciones omitidas por `MIRA_TEST_DB` ausente.
- Desde `frontend`: `python -m pytest tests --ignore=tests/e2e -q`: 63 aprobadas. Desde raíz falla la importación `app`; ejecutar en frontend hasta añadir configuración de pytest.
- No se ha verificado aún el estado de los procesos locales ni las migraciones en una base aislada. Evidencias históricas de V2 no equivalen a verificaciones de esta ejecución.

## Riesgos y decisiones

1. Todas las operaciones son públicas. Añadir autenticación por defecto y RBAC en backend; actualizar pruebas que exigen acceso anónimo, manteniendo regresiones CRUD.
2. No transportar JWT en cookie firmada legible ni localStorage. Usar Flask-Session con almacenamiento del servidor, rotación de sesión, CSRF y caducidad absoluta.
3. Utilizar cuentas de acceso de 003, conservando personas académicas de 006 sin cambios. Documentar ambas identidades. No convertir hashes desconocidos: requieren restablecimiento administrativo; no habilitar texto plano.
4. Conservar roles: Investigador consulta; Coordinador consulta/crea/modifica; Administrador añade eliminación y gestión de cuentas. Es una matriz inicial explícita para V3.
5. Token de corta duración, firma HS256 con clave externa, emisor/audiencia/expiración. Implementar registro de sesiones para revocación real, con comprobación de cuenta y roles en cada solicitud.
6. No instalar React para iconos. Centralizar iconografía SVG compatible con Jinja basada en Tabler y conservar arquitectura.
7. Dependencias nuevas necesitan restauración; acceso de red restringido. PostgreSQL local 16 está disponible en `.local/postgresql16`, pendiente de validar ejecución.

## Etapas y archivos previstos

1. Especificación: este documento y `versiones/v3/2_spec.md`; matriz de permisos y decisiones.
2. Seguridad: paquetes API, `Program.cs`, `Servicios/Seguridad`, `Repositorios/Seguridad`, `Peticiones/AuthController.cs`, migración aditiva 008. No reescribir migraciones V2.
3. Integración: `frontend/app.py`, módulo de sesión/cuentas y nuevas plantillas; requisitos y configuración de ejecución. JWT solo en servidor.
4. Diseño: base, dashboard, CSS y iconos compartidos; mantener formularios, búsqueda, tablas y modales existentes.
5. Calidad: pruebas de seguridad .NET/Flask, regresiones con identidad de pruebas, base aislada, SQL y navegador cuando estén disponibles.
6. Documentación: mapa técnico, arquitectura, sustentación, pruebas y cambios V3 con resultados observados y pendientes explícitos.
