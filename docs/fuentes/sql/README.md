# SQL identificado por el usuario como modelo oficial

El 23 de octubre de 2026 el usuario confirmó expresamente que estos dos archivos son los SQL del modelo oficial que debe utilizar MIRA. Se recuperaron sin modificar desde `repair/v1-catalogos`, commit `880ac758e322ff51d9e4f3cc54d6f7f8ee50c640`.

| Archivo original | Blob Git |
|---|---|
| `database/init/01-init.sql` | `c0c7cd87509ce815bec845b8c252fcb6e87628d1` |
| `database/migrations/002-complete-research-model.sql` | `87a11bbd0fbf57330eeb08026ba056502775e0f3` |

[Inicialización en GitHub](https://github.com/francis872/Mira/blob/880ac758e322ff51d9e4f3cc54d6f7f8ee50c640/database/init/01-init.sql) · [Migración en GitHub](https://github.com/francis872/Mira/blob/880ac758e322ff51d9e4f3cc54d6f7f8ee50c640/database/migrations/002-complete-research-model.sql).

Estas copias son fuentes, no migraciones automáticas. La incorporación incremental está en `database/migrations/006_v2_modelo_oficial.sql`; las rutinas están en la migración siguiente. No se reemplazan las tablas singulares de V1 ni la migración 003. Los nombres plurales y las referencias a `sedes`/`usuarios` son los del SQL confirmado.
