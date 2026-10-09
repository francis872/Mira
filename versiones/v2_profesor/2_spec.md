# MIRA — Versión 2 — Especificación funcional

## Alcance acumulativo
La V2 conserva la Entrega 1 (API REST y frontend para seis catálogos) y amplía la solución conforme a las indicaciones del profesor. Los requisitos de este documento se implementan y verifican, no se consideran completados solo por estar documentados.

## Requerimientos funcionales nuevos
- **RF-V2-FK-01:** toda clave foránea seleccionable se presenta con un control de selección que muestra valores comprensibles; nunca se pide teclear el identificador.
- **RF-V2-MD-01:** cada relación maestro-detalle identificada dispone de una operación atómica de alta mediante una rutina de PostgreSQL que inserta encabezado y todos los detalles. Quedan prohibidos INSERT separados desde la API para una misma operación de negocio.
- **RF-V2-MD-02:** modificación de maestro y colección completa de detalles mediante rutina de PostgreSQL, con control transaccional.
- **RF-V2-MD-03:** consulta individual y listado de maestro-detalle mediante rutinas de PostgreSQL; el resultado de datos compuestos se devuelve en JSONB.
- **RF-V2-MD-04:** eliminación o inactivación mediante rutina de PostgreSQL. Se respetará el borrado lógico cuando la metodología de la asignatura lo exija; confirmar la regla por entidad antes de implementar.
- **RF-V2-DB-01:** incorporar triggers únicamente donde existan invariantes, totales derivados o desnormalización controlada que los requieran.
- **RF-V2-DB-02:** crear vistas justificadas por consultas recurrentes y requerimientos reales.
- **RF-V2-SEC-01:** autenticar mediante usuario/correo y contraseña, guardando exclusivamente hashes bcrypt con factor de costo configurado y adecuado.
- **RF-V2-SEC-02:** credenciales ausentes/inválidas => HTTP 401; identidad autenticada sin permisos => HTTP 403.
- **RF-V2-SEC-03:** autorización basada en roles aplicada del lado del servidor. Evitar MD5/SHA1 como hashes de contraseña.

## Calidad y trazabilidad
- Mantener arquitectura Controller -> Service -> Repository -> PostgreSQL y principios SOLID.
- Validar claves foráneas y roles del lado del servidor, no solo en frontend.
- No construir relaciones maestro-detalle ficticias a partir de las seis tablas independientes de la Entrega 1: partir del modelo oficial de la siguiente entrega.
- La V2 requiere pruebas de atomicidad, integridad, permisos, resultados JSON y regresión de V1.

## Clasificación de alcance
Solo la columna "Obligatorio" proviene del profesor. Las decisiones técnicas del desarrollo no son requisitos académicos.

| Componente | Clasificación | Nota |
|---|---|---|
| V1: 6 catálogos con CRUD y borrado lógico, frontend Flask | Obligatorio | Prioridad 1; regresión automatizada |
| Modelo relacional oficial de V2 y maestro–detalle | Obligatorio | **Pendiente: modelo no entregado** |
| FK mediante selectores | Obligatorio | Hecho para `usuario_rol.rol_id` |
| Procedimientos almacenados CRUD maestro–detalle, JSON/JSONB, atomicidad | Obligatorio | Hecho para usuario→roles |
| Triggers de integridad y vistas necesarias | Obligatorio (donde existan datos derivados) | Pendiente del modelo oficial |
| Autenticación usuario/contraseña, bcrypt, roles, HTTP 401 y 403 | Obligatorio | Hecho |
| Token JWT firmado (60 min) | Necesario técnicamente: transporta la identidad entre Flask y la API | **No es un requisito del profesor.** Se mantiene en su forma mínima: un secreto, sin refresh ni revocación |
| Comando `--bootstrap-admin` | Necesario técnicamente: sin un primer administrador nadie puede autenticarse | No hay autorregistro público |
| Pantalla de administración de usuarios con filas de rol | Necesario técnicamente: es el único maestro–detalle real hoy y usa selectores | — |
| Cookie de sesión Flask `HttpOnly` + `SameSite=Strict` | Necesario técnicamente | — |
| Costo bcrypt, emisor y audiencia JWT configurables; verificación `Origin`; token CSRF; refresh/revocación de tokens; documento duplicado `v2-implementation.md` | Opcional / fuera de alcance | Retirados o no desarrollados |

## Trazabilidad requisito → implementación → prueba
| ID | Estado | Implementación | Evidencia (prueba ejecutada) |
|---|---|---|---|
| RF-V2-FK-01 | Cumplido para el único FK existente (`usuario_rol.rol_id`) | `frontend/templates/usuarios/form.html` (select cargado desde `GET /api/auth/roles`) | `test_user_form_uses_select_populated_from_api_not_typed_ids` |
| RF-V2-MD-01 | Cumplido para usuario→roles | `sp_usuario_crear` (PROCEDURE) → `fn_usuario_crear` | `Create_InsertsMasterAndAllDetailsAtomically`, `Create_WithNonexistentRole_RollsBackTheMaster`, `test_user_creation_is_single_atomic_api_call_with_role_collection` |
| RF-V2-MD-02 | Cumplido para usuario→roles | `sp_usuario_actualizar` | `Update_ReplacesDetailsAndMissingUserRaisesP0002`, `Update_WithInvalidRole_KeepsPreviousDetails` |
| RF-V2-MD-03 | Cumplido para usuario→roles | `fn_usuario_consultar`, `fn_usuario_listar` (JSONB) sobre `vw_usuarios_roles` | `database/tests/v2_usuarios_roles.sql` |
| RF-V2-MD-04 | Cumplido (borrado lógico) | `sp_usuario_inactivar` | `Deactivate_IsLogicalAndIdempotentlyReportsMissing` |
| RF-V2-DB-01 | **Bloqueado**: sin modelo académico oficial no hay campos derivados que justifiquen triggers | — | — |
| RF-V2-DB-02 | Parcial: solo `vw_usuarios_roles` | `database/init/03_v2_usuarios_roles.sql` | `database/tests/v2_usuarios_roles.sql` |
| RF-V2-SEC-01 | Cumplido | `AuthService` (bcrypt, costo configurable) | `LoginVerifiesBcryptAndReturnsRoleClaims`, `UserCreationStoresBcryptHashInsteadOfPassword`; hash `$2a$12$` verificado en BD |
| RF-V2-SEC-02 | Cumplido | `[Authorize]` + token firmado | `AdminRoutes_RequireAuthentication`, `AuthenticatedUserWithoutAdminRole_ReceivesForbidden`, `ExpiredToken_ReturnsUnauthorized`, smoke HTTP real |
| RF-V2-SEC-03 | Cumplido en API y Flask | `RolesAcceso`, `enforce_access` | `CatalogAuthorizationTests`, `test_read_only_role_can_list_but_not_write` |
| V1 (regresión) | Cumplido | 6 controladores sin cambios funcionales; solo se añadió `[Authorize]` | `CatalogCrudRegressionTests` (19 pruebas contra PostgreSQL: crear, consultar, listar, actualizar, borrado lógico, 404, 400, 409) |

## Decisión de modelo canónico de usuarios
En esta rama el modelo canónico es `usuario` / `rol` / `usuario_rol` (V2). El par `usuarios`/`roles` pertenece a otra línea de trabajo (`repair/v1-catalogos`) y no existe aquí; no se crean tablas duplicadas. Si ambas líneas se unifican deberá hacerse mediante una migración explícita aprobada.
