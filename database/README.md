# Database

This directory contains the PostgreSQL schema assets used by MIRA.

## Structure

- init/: complete initialization scripts executed by the PostgreSQL container on first startup.
- migrations/: incremental scripts for an already initialized local database.

## Current model

- `init/01-init.sql` creates the complete 19-table model documented in the historical v0.1 data model.
- `migrations/002-complete-research-model.sql` adds the 11 research-domain tables to an existing V1 volume without deleting data.
- No research-domain seed data is inserted by the migration.

The active V1 API still exposes only catalog and sede endpoints. The research-domain tables are present as database structure for the next implementation phase and do not imply new API endpoints in this phase.

The existing V1 volume uses integer identifiers for `usuarios`; the migration preserves that live type for compatibility, while the historical v0.1 document describes UUID identifiers. This divergence remains documented and must be resolved before implementing the related API contracts.
