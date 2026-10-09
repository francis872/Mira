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
- **.NET 10 LTS:** migración desde .NET 7 (EOL); Swashbuckle 10 exige OpenAPI 2 (`OpenApiSecuritySchemeReference`).
- **Sesión Flask:** el JWT se guarda en la cookie de sesión firmada (HttpOnly, `SameSite=Strict`, 60 min); 401 de la API cierra la sesión; 403 muestra página de acceso denegado. CSRF mediante verificación de `Origin`/`Referer`; el token CSRF por formulario queda pendiente.
- **Primer administrador:** `dotnet MIRA.Api.dll --bootstrap-admin` con `MIRA_BOOTSTRAP_ADMIN_EMAIL` y `MIRA_BOOTSTRAP_ADMIN_PASSWORD` en el entorno del proceso; solo funciona si no hay administrador activo (bloqueo `pg_advisory_xact_lock`).
- **Entorno con poco disco:** `SatelliteResourceLanguages=en` reduce la salida de compilación.

## Seguridad
bcrypt es un hash adaptativo, no cifrado reversible. Utilizar una librería mantenida, salt generado por la librería, Verify para comparación y trabajo calibrado; no afirmar que sea invulnerable.
- 401 es fallo de autenticación o ausencia de autenticación válida.
- 403 es usuario autenticado pero no autorizado.
- No devolver información que facilite enumerar cuentas.
