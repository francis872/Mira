# Database

PostgreSQL 16 assets for MIRA.

## Structure

- migrations/: ordered, idempotent SQL scripts (`NNN_name.sql`) and `apply.sh`, which records each applied script in `schema_migrations`.
  - 001: V1 catalog tables. 002: V1 initial data (inserted only when missing). 003: `usuario`/`rol`/`usuario_rol`, view and stored procedures (database-only; kept with its data for a future version).
- init/00-migrar.sh: runs `apply.sh` when the Docker container initializes an empty volume.
- tests/: SQL tests for the stored procedures.

## Existing database (no data loss)

    docker compose exec mira-postgres sh /migrations/apply.sh

Only pending migrations run. Never edit an applied migration; add a new numbered one.

Do not add fictional tables. The data model must come from the official source documents and the active version specification.
