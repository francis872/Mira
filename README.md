# MIRA V3

Módulo de Investigación para la Gestión de Proyectos Académicos · Universidad de San Buenaventura Medellín.

V3 evoluciona V1/V2 con autenticación bcrypt/JWT, permisos en backend, sesiones del servidor y una interfaz institucional monocromática. Se conservan los seis catálogos, once recursos de investigación, cinco asociaciones, la ficha grupo-semilleros y las rutinas/triggers PostgreSQL. Rama: `feature/mira-v3`.

Cierre de estabilización del 2026-11-05: **71 .NET, 78 Flask, 43 E2E y cinco suites SQL aprobadas**; los mismos E2E pasan en Linux/Gunicorn/Docker. 50 combinaciones de página/ancho, seis modales por teclado y 18 comprobaciones axe sin infracciones automáticas finales. OSV: 18 paquetes Python de la imagen final, cero avisos conocidos. Preparada para sustentación local con observaciones C06 y revisión asistiva. **Render/Neon reales y producción no validados.** Ver [informe y matriz](docs/V3_REVISION_FINAL.md). Sin commits ni despliegues automáticos.

## Arquitectura

ASP.NET Core (.NET 10) → servicios → repositorios Dapper/Npgsql → PostgreSQL 16. Frontend Flask 3.1/Jinja y Bootstrap 5; SVG Tabler locales, sin React. Solo los repositorios acceden a SQL.

Las cuentas `usuario/rol/usuario_rol` (migración 003) autentican. Las personas `usuarios` (006), vinculadas a docentes, siguen siendo un dominio separado. No se fusionan datos ni se infiere correspondencia entre sus identificadores.

| Rol | Consultar | Crear/modificar | Eliminar | Gestionar cuentas |
|---|---|---|---|---|
| Investigador | Sí | No | No | No |
| Coordinador | Sí | Sí | No | No |
| Administrador | Sí | Sí | Sí | Sí |

## Ejecutar en Windows

1. Instalar .NET 10, Python y PostgreSQL. `dotnet restore MIRA.sln`; `python -m pip install -r frontend/requirements-dev.txt`.
2. Configurar `.env.local` con POSTGRES_DB, POSTGRES_USER, POSTGRES_PASSWORD y POSTGRES_PORT. Ver [scripts](scripts/README.md). No versionar el archivo.
3. `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-local.ps1`. Crea una base local si falta, aplica migraciones pendientes y genera una clave JWT aleatoria persistente en `.local/jwt-key` si no se define JWT_KEY. El script admite `-PostgresBin`.
4. Primera instalación: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/bootstrap-admin.ps1`. Solicita correo y contraseña privada. El procedimiento PostgreSQL impide aprovisionar otro administrador inicial cuando ya existe uno activo.
5. Abrir [MIRA](http://localhost:5000/login) y [Swagger](http://localhost:8081/swagger). Crear cuentas y asignar roles desde **Cuentas de acceso** con un administrador.

Para recuperar un administrador activo con contraseña heredada desconocida, el operador local puede usar `scripts/bootstrap-admin.ps1 -ResetPassword`; requiere acceso a la configuración de PostgreSQL y revoca sus sesiones.

Detener con `scripts/stop-local.ps1`. Los datos se conservan. Si ya hay procesos V2 ejecutándose, detenerlos con ese script y volver a iniciar para cargar V3.

## Docker Compose

Copiar `.env.example` a `.env`; configurar POSTGRES_PASSWORD, JWT_KEY aleatoria (32 bytes o más) y SECRET_KEY. `docker compose up -d --build`.

En una base existente: `docker compose exec mira-postgres sh /migrations/apply.sh`. Solo aplica migraciones pendientes; 008 añade sesiones, sin convertir credenciales ni borrar datos. Se conserva el nombre de proyecto Compose `mira-v2` para conservar su volumen.

Para aprovisionar el administrador con Docker, definir MIRA_ADMIN_EMAIL y MIRA_ADMIN_PASSWORD por variables privadas de proceso y pasarlas al contenedor con `docker compose exec -e MIRA_ADMIN_EMAIL -e MIRA_ADMIN_PASSWORD mira-api dotnet MIRA.Api.dll --bootstrap-admin`. Retirarlas después. Nunca usar contraseña literal en el historial del terminal.

El frontend usa Gunicorn, un worker con cuatro threads, y el volumen mira_frontend_sessions para una instancia. No usar réplicas con archivos de sesión independientes.

Los puertos siguen siendo 5000 (frontend), 8081 (API) y 5544 (PostgreSQL), solo en loopback. No se despliega infraestructura. Un despliegue de producción necesita HTTPS y configuración de sesiones/secretos apropiada: [arquitectura](docs/V3_ARQUITECTURA.md).

## API y seguridad

- POST `/api/auth/login`: correo/password → 200 con token, expiraEn y usuario; credenciales inválidas → 401.
- GET `/api/auth/me`, POST `/api/auth/logout`: requieren Bearer. Logout revoca jti en PostgreSQL.
- GET/POST `/api/auth/usuarios`, PUT/DELETE `/api/auth/usuarios/{id}`, PUT `/api/auth/usuarios/{id}/password`, GET `/api/auth/roles`: solo Administrador.
- Los endpoints V1/V2 conservan sus contratos de datos, ahora con Bearer y RBAC. Sin identidad: 401; sin permisos: 403. Swagger documenta Authorize/Bearer en Development.
- bcrypt coste 12. JWT de 20 minutos, firma/emisor/audiencia/expiración validados. No hay refresh automático. No se aceptan contraseñas en texto plano heredadas: restablecerlas desde Cuentas.
- El navegador recibe una cookie de sesión opaca HttpOnly/SameSite Strict; el JWT queda en el servidor. Todos los POST tienen CSRF, incluidos login/logout. SESSION_COOKIE_SECURE=true con HTTPS; MIRA_ENV=production exige Secure, SECRET_KEY externa >=32 caracteres, API_URL HTTPS y SESSION_DIRECTORY absoluta.
- Configuración API: Jwt__Key, Jwt__Issuer (MIRA), Jwt__Audience (MIRA.Frontend), Jwt__Minutes (20, entre 1 y 60). Flask: API_URL, SECRET_KEY, SESSION_DIRECTORY y SESSION_COOKIE_SECURE.

## Verificar

```powershell
dotnet test MIRA.sln
python -m pytest frontend/tests --ignore=frontend/tests/e2e -q
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-postgres16.ps1 -Browser
```

La verificación PostgreSQL usa una instancia/base aisladas, aplica migraciones y SQL, ejecuta regresiones y pruebas E2E con identidades temporales, y detiene sus procesos. `-Browser` requiere Edge y Node. Datos locales y logs permanecen en `.local/pg16-validation`; no publicarlos, pues contienen configuración privada.

Para E2E contra una plataforma de pruebas ya preparada: definir MIRA_E2E_URL, MIRA_E2E_API_URL, MIRA_E2E_EMAIL y MIRA_E2E_PASSWORD de una cuenta administradora de pruebas, y ejecutar `python -m pytest frontend/tests/e2e -q`. Nunca apuntar estas pruebas a datos institucionales. `MIRA_TEST_DB` habilita integraciones .NET; sin ella se omiten explícitamente.

## Documentación y sustentación

- [Auditoría inicial y plan](docs/V3_AUDITORIA_INICIAL.md), [especificación](versiones/v3/2_spec.md).
- [Arquitectura y decisiones](docs/V3_ARQUITECTURA.md), [mapa técnico por archivo/método/prueba](docs/V3_MAPA_TECNICO.md).
- [Guía de sustentación y simulacro](docs/V3_GUIA_SUSTENTACION.md).
- [Revisión final, riesgos, Render/Neon y plan de commits](docs/V3_REVISION_FINAL.md), [resumen saneado](docs/evidencias/v3/revision-final.json).
- [Pruebas ejecutadas y limitaciones](docs/V3_PRUEBAS.md), [cambios de contrato](docs/V3_CAMBIOS.md).
- Antecedentes V2: [arquitectura](docs/arquitectura-v2.md), [criterios académicos](docs/criterios-v2.md), [fuente SQL oficial](docs/fuentes/sql/README.md), [prompts históricos](docs/prompts/README.md).

Los documentos V2 describen su entrega sin autenticación. La especificación V3 reemplaza ese alcance para esta rama. No se declara aceptación académica de las discrepancias previas C06 ni certificación completa de accesibilidad. Consultar la evidencia V3 para distinguir resultados ejecutados de verificaciones pendientes.


## Preparación de producción sin despliegue

Consultar [deploy/README.md](deploy/README.md) y [render.yaml.example](deploy/render.yaml.example), validado solo contra JSON Schema. Dos imágenes Docker sin root, Gunicorn y PORT configurables; API Production exige TLS Npgsql VerifyFull/Channel Binding Require y errores detallados desactivados. El perfil frontend requiere disco privado persistente y una instancia; su eventual provisión puede generar coste. No se crearon servicios ni se usaron credenciales remotas.

Instalar `frontend/requirements.lock` para runtime y `requirements-dev.txt` para pruebas en entorno limpio. Los ocho avisos anteriores están corregidos para las versiones auditadas; no se actualizó Python global ni se auditó todo el SO base. [OSV final](docs/evidencias/v3/osv-estabilizado.json). Para verificar contenedores locales: construir etiquetas mira-v3-api:stabilization/mira-v3-frontend:stabilization y ejecutar scripts/verify-docker.ps1; crea un proyecto de pruebas exclusivo. scripts/verify-production-smoke.py usa destinos ficticios: sus health/cookies no certifican TLS real ni conexión Neon.
