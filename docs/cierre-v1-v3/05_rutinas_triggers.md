# Rutinas Fase 5 y conservación histórica

`tr_area_aplicacion_normalizar`, `semilleros_grupo_activo_trigger` y `sp_guardar_v2_ficha_grupo` conservan sus archivos originales y contratos. Las diez suites SQL y las regresiones integradas comprueban normalización, grupo activo, integridad y rollback maestro/detalle. Los objetos `sp_*` añadidos son **funciones PostgreSQL**, no procedimientos CALL.

009 introduce preflight y CHECKs V1. 010 redefine funciones de sede/palabra para nuevos campos y añade guardas de sede/línea activa, `sp_asignar_v2_lineas`, `sp_resumen_v2_lineas`, `sp_filtrar_v2_semilleros`. 011 añade tres funciones de reportes. 012 reemplaza creación/edición de línea por una operación atómica y aplica mínimos al COMMIT con constraint triggers DEFERRABLE INITIALLY DEFERRED. Bloquea el padre al cambiar puentes para impedir dos retiradas concurrentes que vacíen el conjunto. Una referencia inválida tardía revierte línea y ambos conjuntos.

013 permite listar inactivas y cambiar estado sin borrar asociaciones; la API exige Administrador. 014 solo añade rótulos oficiales ausentes al catálogo plural ODS. [Diez suites y casos](evidencias/fase5-20261107/resultados.json), [fuentes preservadas](evidencias/fase5-20261107/preservacion.json).

---

## Corte previo conservado como antecedente

# Triggers, funciones, procedimientos y transacciones

Un prefijo `sp_` no convierte una función en procedimiento. El catálogo `pg_proc.prokind` y las llamadas reales determinan la clasificación. [116 funciones y 6 procedimientos](evidencias/esquema.json), dos triggers de usuario.

| Migración | Responsabilidad comprobada |
|---|---|
| 001 | Seis tablas V1 |
| 002 | Datos iniciales V1 |
| 003 | Identidad singular, roles, funciones de consulta y cuatro procedimientos de cuentas/roles |
| 004 | Treinta funciones V1: listar, consultar, crear, actualizar, eliminar × seis |
| 005 | fn_area_aplicacion_normalizar y trigger BEFORE INSERT/UPDATE OF nombre |
| 006 | Modelo académico confirmado, índices y trigger de semillero con grupo activo |
| 007 | 79 funciones de investigación, vistas y refuerzo del trigger con bloqueo compartido/SQLSTATE 23503 |
| 008 | sesion_acceso y dos procedimientos V3 que revocan sesiones al actualizar/restablecer |

`tr_area_aplicacion_normalizar` invoca `fn_area_aplicacion_normalizar`, normaliza nombre antes de persistir y la respuesta muestra el valor realmente guardado. Ver `CatalogCrudRegressionTests.AreaName_IsNormalizedByDatabaseTrigger_AndReturnedPersisted` y E2E `test_area_form_uses_trigger_and_displays_persisted_value`.

`semilleros_grupo_activo_trigger` se dispara antes de INSERT o UPDATE OF grupo_id. Invoca `mira_validar_grupo_activo_semillero`; 007 conserva la regla, usa FOR SHARE sobre el padre y lanza 23503 para traducirla a 409 comprensible. No es un trigger genérico sobre toda actualización. Test: `InvalidReferenceAndInactiveParentTrigger_ReturnConflictWithoutInternals`; SQL `v2_investigacion.sql`.

`sp_guardar_v2_ficha_grupo(integer,jsonb)` es una **función RETURNS jsonb** llamada mediante SELECT en `InvestigacionRepository.GuardarFicha`, no CALL. Valida/crea o bloquea maestro, recorre detalles, bloquea detalles existentes y comprueba pertenencia, crea nuevos, actualiza existentes e inactiva omitidos. Una sola llamada participa en la transacción del statement; una excepción tardía revierte maestro y detalles. No se simula atomicidad con varias llamadas HTTP o commits parciales.

Demostración: `SheetCreatesManyRowsThenRollsBackAllChangesWhenLastRowBelongsElsewhere` compara el estado completo antes/después de un detalle ajeno; el E2E crea, edita y retira varios renglones. En 007 los 79 incluyen 55 operaciones para once recursos, 20 para cinco puentes y cuatro de referencia/ficha. El total 116 del catálogo incluye las funciones de otras migraciones, sin sumar dos veces la definición reemplazada del trigger.

Los procedimientos de cuenta se invocan con CALL y parámetros Dapper. `CuentaRepository.Crear/Bootstrap/Editar/Restablecer` usa `sp_usuario_crear`, `sp_usuario_bootstrap_admin`, `sp_v3_usuario_actualizar` y `sp_v3_usuario_password`. Bcrypt se calcula antes, en C#; PostgreSQL no recibe contraseña clara para convertirla. Consultar la firma exacta de los seis procedimientos en el inventario siguiente.

| Procedimiento | Firma PostgreSQL |
|---|---|
| `sp_usuario_actualizar` | `IN p_id bigint, IN p_correo text, IN p_roles jsonb, INOUT p_resultado jsonb` |
| `sp_usuario_bootstrap_admin` | `IN p_correo text, IN p_hash text, INOUT p_resultado jsonb` |
| `sp_usuario_crear` | `IN p_correo text, IN p_hash text, IN p_roles jsonb, INOUT p_resultado jsonb` |
| `sp_usuario_inactivar` | `IN p_id bigint, INOUT p_inactivado boolean` |
| `sp_v3_usuario_actualizar` | `IN p_id bigint, IN p_correo text, IN p_roles jsonb, INOUT p_resultado jsonb` |
| `sp_v3_usuario_password` | `IN p_id bigint, IN p_hash text` |


Los ocho scripts históricos se aplicaron sobre bases nuevas aisladas; no se modificaron ni se ejecutaron contra Neon. Las cinco suites SQL aprueban, pero los contraejemplos históricos también son reproducibles: esas suites reflejan el alcance implementado y no cubrían todos los criterios originales. No se oculta esa diferencia.
