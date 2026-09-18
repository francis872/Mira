# MIRA V1 — Spec Kit vs Code Audit

| Requisito | Documento | Implementación | Estado |
|---|---|---|---|
| MIRA V1 es catalogos y no auth | `versiones/v1_catalogos/2_spec.md` | API y frontend limitados a catalogos y sedes | PASS |
| Base de datos V1 y modelo histórico | `versiones/v1_catalogos/5_data_model.md`, `versiones/v0.1_api_inicial/5_data_model.md` | `database/init/01-init.sql` y `database/migrations/002-complete-research-model.sql` | PASS con divergencia de tipos heredados |
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
| Exclusive V1 auth exclusion | `versiones/v1_catalogos/4_research.md` | Las tablas heredadas de auth no tienen seed ni flujo activo | PASS |
| Guardar documentos fuente | `docs/fuentes/README.md` | `docs/fuentes/*` preserved as source documents | PASS |

## Observaciones

- El repositorio incluye artefactos heredados de autenticación en `UsuarioRepositorio`, `IUsuarioRepositorio` y `AsignarRolRequest`, pero no forman parte del flujo activo de V1.
- El esquema conserva `usuarios`, `roles` y `usuario_roles` por compatibilidad estructural, sin credenciales iniciales ni endpoints públicos.
- La documentación del proyecto y la implementación real están alineadas para la versión funcional actual.
- La estructura física histórica de 19 tablas ya está creada; los endpoints de investigación permanecen fuera de esta fase.
- La BD contiene el modelo físico completo de investigación, pero la API activa V1 expone únicamente sedes y catálogos definidos por `versiones/v1_catalogos`.
- La presencia de tablas `docentes`, `grupos_investigacion`, `semilleros` y `lineas_investigacion` no significa que sus endpoints REST estén implementados.
- La auditoría de backend confirma que Docente, Grupo, Semillero y Línea no tienen componentes C# en V1, de forma consistente con la exclusión explícita de la Spec activa.
- La divergencia UUID/integer queda cerrada a favor de integer para la base viva; no se alteraron PK existentes.
- La validación de runtime se realizó con PostgreSQL real y no con mocks.
