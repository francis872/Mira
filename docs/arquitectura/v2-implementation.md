# Implementación V2: usuarios, roles y seguridad

## Entregado en la rama feature/v2-profesor
- database/init/03_v2_usuarios_roles.sql: tablas reales rol, usuario, usuario_rol; vista agregada JSONB; funciones de alta, consulta, listado, actualización e inactivación.
- MIRA.Api/Seguridad: contratos, repositorio Dapper, login con bcrypt y token JWT, rutas administrativas protegidas.
- Program.cs: middleware de autenticación y autorización; 401 frente a falta de credenciales y 403 para rol insuficiente.
- docker-compose.yml y .env.example: variables para firma JWT y sesión Flask.

## Importante: puesta en marcha
- En una instalación Docker nueva, el script 03 se ejecuta al inicializar el volumen PostgreSQL.
- En una instalación con volumen existente, aplicar el SQL 03 mediante psql de forma explícita; Docker no vuelve a ejecutar initdb.
- Configurar Jwt__Secret con valor criptográficamente aleatorio de al menos 32 bytes. No usar el valor de .env.example ni publicarlo.
- El frontend Flask V1 todavía no tiene pantalla de login ni formulario administrativo para asignar roles. La API expone GET /api/auth/roles para llenar un selector (protegido como Administrador).
- Es necesario dar de alta al primer usuario administrador mediante una operación de provisión fuera de la API y con hash bcrypt (NO hay endpoint público de autorregistro). No escribir contraseñas en SQL ni en el repositorio.
- No confundir el esquema usuario/roles con el modelo maestro-detalle completo de investigación: faltan tablas oficiales de la entrega 2.

## Rutas API implementadas (requieren migración y configuración)
- POST /api/auth/login (pública) => token o 401.
- GET /api/auth/roles (Administrador) => opciones de rol.
- GET /api/auth/usuarios (Administrador) => JSON agregado.
- GET /api/auth/usuarios/{id} (Administrador).
- POST /api/auth/usuarios (Administrador) => maestro/detalle en una función SQL.
- PUT /api/auth/usuarios/{id} (Administrador) => reemplazo atómico del detalle.
- DELETE /api/auth/usuarios/{id} (Administrador) => borrado lógico.

## Limitaciones actuales y QA pendiente
- Compilación y pruebas ejecutadas en .NET 10 (28 pruebas .NET, 13 Flask) y smoke E2E contra PostgreSQL 16 real; ver `versiones/v2_profesor/7_quickstart.md`.
- El frontend Flask ya tiene login/logout, control por rol y administración de usuarios con selector de roles (superseded: el punto de "sin pantalla de login" de arriba ya no aplica). Provisión del primer administrador: `--bootstrap-admin`.
- Pendiente: token CSRF por formulario, regresión manual en navegador de los 6 formularios V1, concurrencia.
- Bloqueado: maestro-detalle del dominio académico y triggers derivados; requieren el modelo oficial de la Entrega 2. No crear triggers decorativos.
