# Investigación y decisiones técnicas V2

## PostgreSQL
- Preferir JSONB para colecciones de detalles y jsonb_agg/jsonb_build_object para respuestas.
- Las funciones PostgreSQL pueden devolver JSONB directamente; un PROCEDURE invocado mediante CALL puede exponer argumentos OUT/INOUT. Elegir firma por operación documentando la diferencia.
- Una operación maestro-detalle debe ser atómica. Invocarla en una transacción apropiada; fallos hacen rollback de todo el conjunto.
- En UPDATE con reemplazo completo de detalles, DELETE + INSERT solo es correcto si no rompe auditoría, identidad externa, relaciones hijas o trazabilidad. Donde esos efectos existan, aplicar diff/actualización granular.
- Para totales desnormalizados, definir claramente triggers BEFORE/AFTER, concurrencia e invariantes; evitar recálculo duplicado desde API.
- Borrado físico vs lógico presenta contradicción en notas orales del curso; seguir metodología escrita aplicable y verificar antes de migrar.

## Decisiones tomadas en la implementación
- **PROCEDURE vs FUNCTION:** las escrituras (`sp_usuario_crear/actualizar/inactivar/bootstrap_admin`) son `CREATE PROCEDURE` invocados con `CALL`; devuelven el JSONB mediante parámetros `INOUT`. Delegan en funciones `fn_usuario_*` y todo se ejecuta en una sola transacción. Las lecturas son funciones (`SELECT`).
- **Reemplazo de detalles con DELETE + INSERT:** aceptable en `usuario_rol` porque la tabla puente no tiene identidad propia, auditoría ni hijos. No debe copiarse a detalles con identidad.
- **Modelo canónico de usuarios:** `usuario`/`rol`/`usuario_rol` (V2); ver 2_spec.md.
- **.NET 10 LTS:** migración desde .NET 7 (EOL).
- **Autenticación retirada del alcance:** el profesor aclaró que login, logout, JWT, bcrypt, registro con credenciales y restricciones por sesión/rol no se desarrollan en V1 ni V2. El código (AuthController/AuthService/AuthRepository, JWT, bcrypt, pantallas de login y de usuarios) se eliminó del árbol activo; el historial se conserva y la etiqueta `auth-futuro-v2-261872f` apunta al último commit que lo contiene. Las tablas `usuario`, `rol`, `usuario_rol` y sus rutinas se mantienen sin cambios en PostgreSQL para no perder datos.
- **CSRF sin sesión de usuario:** la cookie firmada de Flask solo guarda el token CSRF y los mensajes flash. No identifica a nadie ni restringe el acceso.
- **Entorno con poco disco:** `SatelliteResourceLanguages=en` reduce la salida de compilación.

## Seguridad (medidas generales vigentes)
- Validación de entradas en los servicios, SQL parametrizado con Dapper/Npgsql, restricciones de integridad en PostgreSQL, borrado lógico, escape automático de HTML en Jinja y token CSRF en los POST.
- No devolver cadenas de conexión ni detalles internos en los errores.
