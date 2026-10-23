# Database

PostgreSQL 16 assets for MIRA.

## Structure

- migrations/: ordered, idempotent SQL scripts (`NNN_name.sql`) and `apply.sh`, which records each applied script in `schema_migrations`.
  - 001: V1 catalog tables. 002: V1 initial data (inserted only when missing). 003: `usuario`/`rol`/`usuario_rol`, view and stored procedures (database-only; kept with its data for a future version).
  - 004: 30 funciones CRUD, invocadas con SELECT por los seis repositorios. Listar/consultar/crear devuelven filas; actualizar/eliminar devuelven boolean.
  - 005: trigger de normalización de `area_aplicacion.nombre`, sin columnas nuevas ni reescritura de datos existentes.
  - 006: 19 tablas del SQL confirmado por el usuario, recuperado desde `repair/v1-catalogos`/`880ac75`. Se conservan las tablas singulares V1. Fuente exacta: `docs/fuentes/sql/`.
  - 007: 78 funciones académicas y once vistas con nombres de referencia; CRUD de once entidades, cinco puentes y ficha atómica grupo→semilleros. Regeneración: `python scripts/generate-research-routines.py`.
- init/00-migrar.sh: runs `apply.sh` when the Docker container initializes an empty volume.
- tests/: SQL tests for the stored procedures.

`tests/v2_catalogos_rutinas.sql` comprueba V1 y su trigger; `v2_usuarios_roles.sql` conserva pruebas legadas. `v2_investigacion.sql` verifica dominio académico, integridad y rollback tardío; `v2_modelo_oficial.sql` reconstruye la fuente en un esquema aislado y compara columnas, nulabilidad, claves, restricciones e índices. Los cuatro hacen rollback y se ejecutan en base exclusiva. No hay FK nullable en el modelo oficial.

## Existing database (no data loss)

    docker compose exec mira-postgres sh /migrations/apply.sh

Only pending migrations run. Never edit an applied migration; add a new numbered one.

Do not add fictional tables. The data model must come from the official source documents and the active version specification.
