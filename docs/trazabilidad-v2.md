# Trazabilidad MIRA V1 y dominio V2

Fuente funcional: prompt definitivo y confirmación posterior del SQL oficial por el usuario. Fuente física: `docs/fuentes/sql/`. Especificación previa: `docs/integracion-modelo-oficial.md`; requisitos e historias de trabajo: `versiones/v2_profesor/2_spec.md`. No se atribuyen estos identificadores a documentos no aportados del profesor.

| Requisito → Historia | Controller → Service → Repository | PostgreSQL | Interfaz | Prueba |
|---|---|---|---|---|---|
| RF-MIRA-CAT-01–06 / RF-V2-SP-01 → HU-MIRA-CAT-01–06 | Seis `*Controller`, `*Service`, `*Repository` existentes | `sp_{listar,consultar,crear,actualizar,eliminar}_{catalogo}` en 004 | `frontend/app.py`, `templates/modulos/` | `CatalogCrudRegressionTests`, `test_catalog_create_update_inactivate_persist`, Edge |
| RF-V2-ACA-01 → HU-V2-ACA-01 | `InvestigacionController.Listar/Obtener/Crear/Actualizar/Eliminar` → `InvestigacionService` → `Repositorios/Investigacion/InvestigacionRepository` | Cinco `sp_*_v2_{recurso}` por cada uno de once recursos en 007 | `investigacion.py:listing/new/edit/remove`, `investigacion/list.html` y `form.html` | `ElevenResources_AllFiveOperationsPersistAndFilterRemovedRows`, `test_investigacion.py` HTTP |
| RF-V2-FK-01 → HU-V2-REF-01 | `RecursosInvestigacion`, Service.Validar, Controller.Usuarios | FK de 006; `sp_referencias_v2_usuarios` y listados de referencia | `options_for`, `_campos.html`; nombres reales sin códigos tecleados | 11 casos Flask, HTTP y Edge; nulabilidad auditada en SQL |
| RF-V2-ASOC-01 → HU-V2-ASOC-01 | `ObtenerPuente/Crear/RetirarPuente` → Service → Repository | `sp_{listar,consultar,crear,eliminar}_v2_{puente}` para cinco pares PK | `associations/remove_association`, `asociaciones.html` | `AllFiveBridges_AssignQueryDuplicateConflictAndRemove`, cinco casos HTTP, Edge grupo–línea |
| RF-V2-MD-01–04 → HU-V2-FICHA-01 | `CrearFicha/ActualizarFicha/Ficha` → Service.GuardarFicha/Ficha → Repository.GuardarFicha/Ficha | `sp_guardar_v2_ficha_grupo`, `sp_consultar_v2_ficha_grupo` | `group_form`, `grupo.html`, `investigacion.js` | SQL rollback tardío, .NET comparación completa tras fallo, HTTP y Edge varios renglones |
| RF-V2-TR-01 → HU-MIRA-CAT-03 / HU-V2-FICHA-01 | Service deja actuar DB | Trigger normalizador 005; `semilleros_grupo_activo_trigger` oficial en 006/007 | Alta/edición V1 y ficha V2 | SQL, .NET, HTTP y Edge |
| RF-V2-HTTP-01 → HU-MIRA-ERR-01 | `ApiExceptionMiddleware.InvokeAsync`, validación Service | Restricciones reales SQLSTATE 23503/23505/23514 | `app.py:error_message`, flash preservado en formularios | `ErroresPersistenciaTests`, FK real y duplicados de puentes |
| RF-MIRA-CSRF-01 / RF-V2-UI-01–02 | Cliente Flask y plantillas | Sin SQL desde Flask | CSRF, escape Jinja, búsqueda, modales y estados de envío | 63 Flask/arquitectura, 46 HTTP y 14 comprobaciones Edge |

Once recursos: sedes, docentes, grupos_investigacion, semilleros, lineas_investigacion, grupo_docentes, semillero_tutores, areas_conocimiento, ods, areas_aplicacion y palabras_clave. Puentes: grupo_lineas, semillero_lineas, linea_disciplinas, linea_ods y linea_aplicacion. Sus campos exactos están en `RecursosInvestigacion.cs` y su correspondencia con la fuente se compara en `v2_modelo_oficial.sql`.

Las pruebas SQL reconstruyen las 19 tablas originales en un esquema temporal, comparan columnas/restricciones/índices y revierten todo. Las de .NET viven en `tests/MIRA.Api.Tests/InvestigacionIntegrationTests.cs`; Flask en `frontend/tests/test_investigacion.py`; HTTP en `frontend/tests/e2e/test_investigacion.py`; navegador en `scripts/verify-browser.mjs`.

La atomicidad usuario→roles de 003 sigue probándose por separado; no acredita el maestro-detalle académico ni crea una pantalla de credenciales. C06 sigue bloqueado: SQL confirmado sin FK opcional. [Matriz completa](criterios-v2.md).
