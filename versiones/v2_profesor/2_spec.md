# MIRA — Versión 2 — Especificación funcional

## Alcance acumulativo
La V2 conserva la Entrega 1 (API REST y frontend para seis catálogos) y amplía la solución conforme a las indicaciones del profesor. Los requisitos de este documento se implementan y verifican, no se consideran completados solo por estar documentados.

## Corrección de alcance (vigente)
**Login, logout, JWT, tokens, bcrypt/gestión de contraseñas, registro de usuarios con credenciales, pantallas de inicio de sesión, redirecciones a `/login` y restricciones por sesión o rol NO forman parte de V1 ni de V2.** Quedan reservados para una versión futura sin fecha. La aplicación abre directamente en el dashboard y ningún módulo ni endpoint exige credenciales. El código anterior no se destruyó: sigue en el historial Git y la etiqueta `auth-futuro-v2-261872f` marca el último commit que lo contiene.

Se mantienen las medidas generales de seguridad (validación de entradas, SQL parametrizado, integridad en PostgreSQL, token CSRF en formularios). No son autenticación.

## Requerimientos funcionales de V2
- **RF-V2-FK-01:** toda clave foránea seleccionable se presenta con un control de selección que muestra valores comprensibles; nunca se pide teclear el identificador.
- **RF-V2-MD-01:** cada relación maestro-detalle identificada dispone de una operación atómica de alta mediante una rutina de PostgreSQL que inserta encabezado y todos los detalles. Quedan prohibidos INSERT separados desde la API para una misma operación de negocio.
- **RF-V2-MD-02:** modificación de maestro y colección completa de detalles mediante rutina de PostgreSQL, con control transaccional.
- **RF-V2-MD-03:** consulta individual y listado de maestro-detalle mediante rutinas de PostgreSQL; el resultado de datos compuestos se devuelve en JSONB.
- **RF-V2-MD-04:** eliminación o inactivación mediante rutina de PostgreSQL (borrado lógico cuando la metodología lo exija).
- **RF-V2-DB-01:** triggers únicamente donde existan invariantes, totales derivados o desnormalización controlada que los requieran.
- **RF-V2-DB-02:** vistas justificadas por consultas recurrentes y requerimientos reales.

## Calidad y trazabilidad
- Arquitectura Controller → Service → Repository → PostgreSQL y principios SOLID.
- Validar claves foráneas del lado del servidor, no solo en el frontend.
- No inventar entidades ni relaciones que no estén en el modelo oficial. No construir maestro-detalle ficticios a partir de las seis tablas independientes de la Entrega 1.
- La interfaz refleja el estado real: no presenta como terminadas funciones académicas de V2 que dependen del modelo oficial.

## Clasificación de alcance
| Componente | Clasificación | Estado |
|---|---|---|
| V1: 6 catálogos con CRUD, borrado lógico y frontend Flask, sin login | Obligatorio | **Hecho**; regresión automatizada y E2E |
| Dashboard con indicadores reales (conteos desde la API) | Obligatorio | Hecho |
| Modelo relacional oficial de V2 y maestro–detalle académico | Obligatorio | **Pendiente: el modelo no está en el repositorio** |
| FK mediante selectores | Obligatorio | Pendiente del modelo (no existen FK entre las tablas V1) |
| Rutinas almacenadas CRUD maestro–detalle, JSONB, atomicidad | Obligatorio | Hecho en PostgreSQL para `usuario`→`usuario_rol` (migración 003), probado; sin pantalla porque implica credenciales |
| Triggers de integridad y vistas del dominio | Obligatorio (donde existan datos derivados) | Pendiente del modelo oficial (solo existe `vw_usuarios_roles`) |
| Migraciones controladas para bases nuevas y existentes | Necesario técnicamente | Hecho (`database/migrations`) |
| Token CSRF + cookie `HttpOnly`/`SameSite=Strict` | Medida general de seguridad | Hecho; no identifica usuarios |
| Autenticación, JWT, bcrypt, roles, 401/403 | **Fuera de alcance** | Retirado del árbol activo |

## Trazabilidad requisito → implementación → prueba
| ID | Estado | Implementación | Evidencia |
|---|---|---|---|
| RF-V2-FK-01 | Pendiente del modelo oficial | — | — |
| RF-V2-MD-01 | Cumplido en PostgreSQL para usuario→roles | `sp_usuario_crear` (PROCEDURE) → `fn_usuario_crear` | `database/tests/v2_usuarios_roles.sql` (alta atómica y rollback del maestro ante rol inexistente) |
| RF-V2-MD-02 | Cumplido en PostgreSQL | `sp_usuario_actualizar` | ídem (reemplazo de detalles, rollback conserva los anteriores) |
| RF-V2-MD-03 | Cumplido en PostgreSQL | `fn_usuario_consultar`, `fn_usuario_listar` (JSONB) sobre `vw_usuarios_roles` | ídem |
| RF-V2-MD-04 | Cumplido (borrado lógico) | `sp_usuario_inactivar` | ídem |
| RF-V2-DB-01 | **Bloqueado**: sin modelo académico no hay campos derivados | — | — |
| RF-V2-DB-02 | Parcial: solo `vw_usuarios_roles` | `database/migrations/003_v2_usuarios_roles.sql` | `database/tests/v2_usuarios_roles.sql` |
| V1 sin autenticación | Cumplido | Seis controladores sin `[Authorize]`; Flask sin login | `SinAutenticacionTests`, `CatalogCrudRegressionTests` (PostgreSQL real), `frontend/tests/test_app.py`, `frontend/tests/e2e` |

## Decisión sobre las tablas de usuarios
`usuario`, `rol` y `usuario_rol` se conservan intactas (con sus datos, si existen) para la versión futura. No se eliminan tablas con información; la aplicación no las usa.
