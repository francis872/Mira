# Especificación MIRA V3

Evolución aditiva de V1/V2, autorizada por el prompt maestro del 2026-11-05. Mantiene .NET 10 → servicios → repositorios → PostgreSQL y Flask/Jinja.

## Contratos

- POST `/api/auth/login`: correo y contraseña, 200 con token, expiración y usuario; credenciales inválidas/inactivas 401 uniforme. Límite de intentos por IP.
- GET `/api/auth/me`: identidad y roles actuales; 401 si sesión revocada, cuenta inactiva o token inválido.
- POST `/api/auth/logout`: revocación persistente de jti, 200. Sin renovación automática; iniciar sesión después de expirar.
- GET/POST `/api/auth/usuarios`, PUT/DELETE `/api/auth/usuarios/{id}`, GET `/api/auth/roles`: solo Administrador, sin exponer hashes. Contraseñas bcrypt coste 12; entrada 12 caracteres mínimo y máximo 72 bytes UTF-8.
- Aprovisionamiento inicial exclusivamente CLI `--bootstrap-admin` con correo/contraseña en variables de entorno; usa el procedimiento atómico de 003. Nunca endpoint público.
- Recuperación de un administrador activo con hash heredado inválido: CLI `--reset-admin-password` con acceso operativo a PostgreSQL, nuevo bcrypt y revocación; sin ruta pública de recuperación.
- `/health` público; todos los controladores de datos requieren autenticación. Swagger documenta Bearer en Development.

| Rol existente | Consultar V1/V2 | Crear/modificar V1/V2 | Eliminar V1/V2 | Cuentas/roles |
|---|---|---|---|---|
| Investigador | Sí | No | No | No |
| Coordinador | Sí | Sí | No | No |
| Administrador | Sí | Sí | Sí | Sí |

Los permisos son globales por operación, sin propiedad por proyecto porque el modelo actual no la define. Roles desconocidos no conceden acceso. La cuenta debe estar activa y conservar los roles del JWT; un cambio obliga a iniciar sesión nuevamente.

## Sesión y presentación

Flask guarda token e identidad en sesión del servidor CacheLib, cookie opaca HttpOnly/Strict, Secure en HTTPS. Rotación después de login, CSRF en todos los POST incluido login/logout, caducidad absoluta igual al JWT, redirección a login por 401 y página comprensible por 403. No aceptar destinos externos en redirecciones. Al salir se revoca el JWT y se borra la sesión local; si la API falla se informa que el token podría durar hasta su vencimiento.

Interfaz monocromática, foco visible, navegación móvil, tablas desplazables, etiquetas y controles accesibles. SVG Tabler compartidos con Jinja, sin migración React. Mantener todos los módulos existentes. Cuentas de 003 y personas de 006 son dominios separados.

## Verificación

Pruebas 200/401/403, firma/emisor/audiencia/expiración, bcrypt, estado de cuenta, revocación, CSRF, expiración y rotación de sesión; regresiones de catálogos, once recursos, asociaciones y triggers. Evidencia ejecutada en `docs/V3_PRUEBAS.md`; cualquier limitación impide certificar el criterio afectado.
