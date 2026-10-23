# Trazabilidad MIRA V1 + V2 parcial

Las especificaciones son documentos SDD mantenidos en `versiones/`; no se acredita una instalación de GitHub Spec Kit. IDs CAT/DASH/OPEN: `versiones/v0.1_api_inicial/2_spec.md`. IDs restantes: `versiones/v2_profesor/2_spec.md`. Esta matriz registra el alcance probado, no aprobación de documentos oficiales ausentes.

| Requisito | Especificación | Código | Prueba | Estado |
|---|---|---|---|---|
| RF-MIRA-CAT-01 | V1, requisitos | `AreaConocimientoController`, `AreaConocimientoService`, `AreaConocimientoRepository`; `area_conocimiento_*` | `FullLifecycle_CreateReadListUpdateLogicalDelete`, `test_catalog_create_update_inactivate_persist[area_conocimiento]` | Verificado |
| RF-MIRA-CAT-02 | V1, requisitos | `ObjetivoDesarrolloSostenibleController/Service/Repository`; `ods_*` | mismos casos para `objetivo_desarrollo_sostenible` | Verificado |
| RF-MIRA-CAT-03 | V1, requisitos | `AreaAplicacionController/Service/Repository`; `area_aplicacion_*` | mismos casos para `area_aplicacion` | Verificado |
| RF-MIRA-CAT-04 | V1, requisitos | `TerminoClaveController/Service/Repository`; `termino_clave_*` | mismos casos para `termino_clave` | Verificado en entradas probadas |
| RF-MIRA-CAT-05 | V1, requisitos | `UniversidadController/Service/Repository`; `universidad_*` | mismos casos para `universidad` | Verificado |
| RF-MIRA-CAT-06 | V1, requisitos | `LineaInvestigacionController/Service/Repository`; `linea_investigacion_*` | mismos casos para `linea_investigacion` | Verificado |
| RF-MIRA-DASH-01 | V1, requisitos | `frontend/app.py:index`, `_count`, `api_call`; `templates/index.html` | `test_dashboard_counts_match_the_listing`, `test_dashboard_handles_invalid_api_payload` | Verificado |
| RF-MIRA-OPEN-01 | V1, requisitos | `Program.cs`, controladores, rutas Flask | `SinAutenticacionTests`, `test_there_is_no_authentication_surface` | Verificado |
| RF-MIRA-ERR-01 | V2, calidad | `frontend/app.py:error_message`, `list_items`, `api_call`, crear/editar | `test_validation_message_survives_real_response_truthiness`, `test_list_reports_http_failure`, `test_non_json_api_error_preserves_form_values` | Verificado |
| RF-MIRA-VAL-01 | V2, calidad | `MIRA.Api/Modelos/*.cs`, atributos `StringLength` | `OversizedText_ReturnsBadRequest_OnCreateAndUpdate`, `test_oversized_form_preserves_data_and_explains_limit` | Verificado en casos de longitud probados |
| RF-MIRA-TC-01 | V2, calidad | `TerminoClaveRepository.CreateAsync`, SQLSTATE 23505 | `InactiveAndConcurrentDuplicateKeyword_ReturnsConflict`, `test_duplicate_keyword_shows_conflict` | Verificado |
| RF-MIRA-CSRF-01 | V2, calidad | `frontend/app.py:verify_csrf`, `csrf_input` | `test_unicode_csrf_is_rejected_without_api_call`, `test_unicode_csrf_is_rejected`, pruebas de token ausente e incorrecto | Verificado |
| RNF-MIRA-OPS-01 | V2, calidad | `scripts/start-local.ps1`, `stop-local.ps1`, `verify-local.ps1` | Ejecución integral y comprobación de reinicio descritas en `docs/pruebas-v2.md` | Verificado localmente |
| RNF-MIRA-ENV-01 | V2, calidad | `frontend/requirements*.txt` | Comparación con `importlib.metadata` | Versiones declaradas verificadas; Docker pendiente |
| RF-V2-MD-01 | V2, requerimientos | `003_v2_usuarios_roles.sql:sp_usuario_crear` / `fn_usuario_crear` | `database/tests/v2_usuarios_roles.sql`, alta y error FK | Verificado solo usuario→roles en PostgreSQL |
| RF-V2-MD-02 | V2, requerimientos | `sp_usuario_actualizar`, `fn_usuario_actualizar` | mismo archivo: reemplazo y comparación tras fallo | Verificado solo en PostgreSQL |
| RF-V2-MD-03 | V2, requerimientos | `fn_usuario_consultar`, `fn_usuario_listar` | mismo archivo: consulta y presencia en arreglo JSONB | Verificado solo en PostgreSQL |
| RF-V2-MD-04 | V2, requerimientos | `sp_usuario_inactivar`, `fn_usuario_inactivar` | mismo archivo: indicador de inactivación | Verificado solo en PostgreSQL |
| RF-V2-FK-01 | V2, requerimientos | Sin implementación académica | Sin prueba | Bloqueado por modelo oficial |
| RF-V2-DB-01 | V2, requerimientos | Sin triggers académicos definidos | Sin prueba | Bloqueado por reglas y modelo oficial |
| RF-V2-DB-02 | V2, requerimientos | `vw_usuarios_roles` | SQL de rutinas sobre la vista | Parcial; vistas académicas bloqueadas |

Rutas de las pruebas: `tests/MIRA.Api.Tests/CatalogCrudRegressionTests.cs`, `SinAutenticacionTests.cs`, `frontend/tests/test_app.py` y `frontend/tests/e2e/test_local_platform.py`. Los nombres abreviados de las capas corresponden a carpetas `Peticiones/`, `Servicios/` y `Repositorios/` de `MIRA.Api/`.
