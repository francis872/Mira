# MIRA V1 — Spec Kit vs Code Audit

| Requisito | Documento | Implementación | Estado |
|---|---|---|---|
| MIRA V1 es catalogos y no auth | `versiones/v1_catalogos/2_spec.md` | API y frontend limitados a catalogos y sedes | PASS |
| Base de datos real valida | `versiones/v1_catalogos/5_data_model.md` | `database/init/01-init.sql` | PASS |
| Arquitectura layered | `docs/1_constitution.md` | Frontend → Controller → Service → Repository → PostgreSQL | PASS |
| Repositorio como frontera de persistencia | `docs/1_constitution.md` | `MIRA.Api/Repositorios/*` con Npgsql y SQL | PASS |
| SQL parametrizado | `docs/1_constitution.md` | NpgsqlCommand con `@param` | PASS |
| Borrado lógico para V1 | `versiones/v1_catalogos/6_contracts.md` | `activo` / `activa` + `DELETE` HTTP que desactiva | PASS |
| Swagger activo | `docs/1_constitution.md` | `app.UseSwagger()` + `UseSwaggerUI()` | PASS |
| Docker Compose local | `versiones/v1_catalogos/7_quickstart.md` | `docker-compose.yml` con API/Postgres/Frontend | PASS |
| .NET 10 | `docs/1_constitution.md` | `MIRA.Api/MIRA.Api.csproj` target net10.0 | PASS |
| CRUD de sedes | `versiones/v1_catalogos/2_spec.md` | `SedesController` + `SedeServicio` + `SedeRepositorio` | PASS |
| CRUD de catalogos | `versiones/v1_catalogos/2_spec.md` | `CatalogosController` + `CatalogoServicio` + `CatalogoRepositorio` | PASS |
| Frontend local sin DB | `versiones/v1_catalogos/2_spec.md` | `frontend/app.js` consume API HTTP | PASS |
| Exclusive V1 auth exclusion | `versiones/v1_catalogos/4_research.md` | Auth code exists in repo but not in active flow | PASS |
| Guardar documentos fuente | `docs/fuentes/README.md` | `docs/fuentes/*` preserved as source documents | PASS |

## Observaciones

- El repositorio incluye artefactos heredados de autenticación en `UsuarioRepositorio`, `IUsuarioRepositorio` y `AsignarRolRequest`, pero no forman parte del flujo activo de V1.
- La documentación del proyecto y la implementación real están alineadas para la versión funcional actual.
- La validación de runtime se realizó con PostgreSQL real y no con mocks.
