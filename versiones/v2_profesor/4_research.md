# Investigación y decisiones técnicas V2

## PostgreSQL
- Preferir JSONB para colecciones de detalles y jsonb_agg/jsonb_build_object para respuestas.
- Las funciones PostgreSQL pueden devolver JSONB directamente; un PROCEDURE invocado mediante CALL puede exponer argumentos OUT/INOUT. Elegir firma por operación documentando la diferencia.
- Una operación maestro-detalle debe ser atómica. Invocarla en una transacción apropiada; fallos hacen rollback de todo el conjunto.
- En UPDATE con reemplazo completo de detalles, DELETE + INSERT solo es correcto si no rompe auditoría, identidad externa, relaciones hijas o trazabilidad. Donde esos efectos existan, aplicar diff/actualización granular.
- Para totales desnormalizados, definir claramente triggers BEFORE/AFTER, concurrencia e invariantes; evitar recálculo duplicado desde API.
- Borrado físico vs lógico presenta contradicción en notas orales del curso; seguir metodología escrita aplicable y verificar antes de migrar.

## Seguridad
bcrypt es un hash adaptativo, no cifrado reversible. Utilizar una librería mantenida, salt generado por la librería, Verify para comparación y trabajo calibrado; no afirmar que sea invulnerable.
- 401 es fallo de autenticación o ausencia de autenticación válida.
- 403 es usuario autenticado pero no autorizado.
- No devolver información que facilite enumerar cuentas.
