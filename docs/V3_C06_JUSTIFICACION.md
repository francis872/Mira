# C06 — justificación para evaluación académica

Revisión local del 2026-11-07. Propuesta sin publicar; no constituye aceptación del profesor ni cambia el esquema.

## Enunciado y evidencia

La fuente docente `docs/prompts/PROMPT_MAESTRO_DEFINITIVO_MIRA_V2.md` indica:

- Línea 406: «Permitir null cuando la clave foránea sea opcional.»
- Línea 707: «C06. Las claves opcionales admiten null.»

La condición es que exista una relación opcional. Las migraciones confirmadas 001–008 y el catálogo PostgreSQL 16.15 muestran **22 columnas de claves foráneas públicas, todas NOT NULL**. Incluyen el dominio académico y las cuentas/sesiones; no se atribuyen las 22 exclusivamente al modelo docente. La consulta usa pg_constraint, conkey y pg_attribute.attnotnull.

Evidencia reproducible: `database/tests/v3_c06_nulabilidad.sql`, ejecutada de nuevo con las otras cuatro suites SQL; [metadatos por tabla y columna](evidencias/v3/cierre-2026-11-07/c06-schema.json). `scripts/ci/prepare_database.py --report <archivo>` permite obtener ese inventario después de migrar una base vacía exclusiva; exige loopback, MIRA_CI=1, PGUSER=mira_ci y PGDATABASE=mira_ci_*. No se consultaron datos institucionales.

Implementación relacionada: `MIRA.Api/Servicios/InvestigacionService.cs`, clase InvestigacionService, método Validar, capa de servicio. Convierte valores vacíos a null **solo** para CampoInvestigacion.Opcional. Los campos opcionales de texto/fecha no prueban que una FK admita null; el test NullsDatesAndEnumerationsAreValidated_AndPersonReferenceDoesNotExposeCredentials no debe presentarse como una prueba de FK nullable.

El script SQL actual verifica el supuesto del modelo (no hay FK opcionales). Si ese supuesto cambia, falla y obliga a revisar la fuente; no demuestra el comportamiento de una FK opcional inexistente.

## Dictamen técnico y alternativas

**Técnicamente, la condición de C06 no tiene una relación a la cual aplicarse en el esquema confirmado. Académicamente permanece pendiente.** No se cambió una restricción ni se inventó una relación para producir una prueba artificial.

1. El profesor puede aceptar la no aplicabilidad al modelo confirmado, dejando constancia de la fuente y el inventario.
2. Si existe otra fuente docente que marque una relación concreta como opcional, identificarla y aprobar el cambio de dominio antes de implementarlo. Preparar una migración nueva; conservar 001–008 y schema_migrations. Actualizar contrato, servicio, formulario y rutina, y probar NULL permitido, referencia válida, referencia inexistente rechazada, demás campos obligatorios y regresiones de triggers/ficha. No seleccionar arbitrariamente una relación.
3. Mientras no exista esa decisión o fuente adicional, mantener C06 como observación académica abierta; no cambiar su estado a aprobado.

## Explicación breve para la sustentación

«El criterio exige NULL cuando la relación es opcional. Revisé el SQL entregado y el catálogo real: ninguna de las 22 columnas FK públicas está declarada opcional. Por eso conservé el modelo y registré la condición como no aplicable técnicamente a esta versión, pendiente de su aceptación. Si usted identifica una relación opcional en otra fuente, la formalizaremos y probaremos sin alterar las migraciones históricas.»

Responsable de cierre: profesor y equipo. Criterio: decisión escrita o fuente/cambio de dominio aprobado y pruebas correspondientes. No se obtiene aceptación simplemente porque la consulta SQL pase.
