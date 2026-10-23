# MIRA — Versión 2 — Especificación funcional

## Alcance acumulativo
La V2 conserva la Entrega 1 (API REST y frontend para seis catálogos) y amplía la solución conforme a las indicaciones del profesor. Los requisitos de este documento se implementan y verifican, no se consideran completados solo por estar documentados.

## Corrección de alcance (vigente)
**Login, logout, JWT, tokens, bcrypt/gestión de contraseñas, registro de usuarios con credenciales, pantallas de inicio de sesión, redirecciones a `/login` y restricciones por sesión o rol NO forman parte de V1 ni de V2.** Quedan reservados para una versión futura sin fecha. La aplicación abre directamente en el dashboard y ningún módulo ni endpoint exige credenciales. El código anterior no se destruyó: sigue en el historial Git y la etiqueta `auth-futuro-v2-261872f` marca el último commit que lo contiene.

Se mantienen las medidas generales de seguridad (validación de entradas, SQL parametrizado, integridad en PostgreSQL, token CSRF en formularios). No son autenticación.

## Requerimientos funcionales de V2
- **RF-V2-SP-01 (C03):** las cinco operaciones de los seis catálogos V1 se ejecutan exclusivamente mediante funciones PostgreSQL `sp_listar_*`, `sp_consultar_*`, `sp_crear_*`, `sp_actualizar_*` y `sp_eliminar_*`. Repository solo realiza invocaciones parametrizadas, sin CRUD directo sobre tablas. Se conservan contratos HTTP y datos.
- **RF-V2-TR-01 (C04):** un trigger BEFORE INSERT/UPDATE normaliza los espacios extremos de `area_aplicacion.nombre`, regla ya existente en Service, y rechaza nombres vacíos. Se centraliza esa normalización en PostgreSQL y se devuelve la fila persistida; no se añaden columnas ni reglas académicas desconocidas. La prueba desde formulario verifica el cambio real y la definición del trigger.
- **RF-V2-HTTP-01 (C07 parcial):** SQLSTATE 23503/23505 producen 409 con mensaje en español; 23502/22023/22001 producen 400; errores inesperados producen 500 sin trazas ni cadenas de conexión en la respuesta. La traducción técnica no acredita una FK académica integrada que aún no existe.
- **RF-V2-UI-01:** navegación con página activa, acceso por teclado, búsqueda local de filas, mensajes de resultado y estado al enviar formularios; conservar Bootstrap, los seis CRUD y conteos reales, sin añadir analítica ni autenticación.

Los criterios C01–C12 proceden del prompt definitivo recibido del usuario, conservado en `docs/prompts/PROMPT_MAESTRO_DEFINITIVO_MIRA_V2.md`. `VERSION_2.md`, el modelo oficial y los nombres de los nueve documentos del curso aún deben localizarse o aportarse; no se afirma haberlos leído. Matriz: `docs/criterios-v2.md`.
- **RF-V2-FK-01:** toda clave foránea seleccionable se presenta con un control de selección que muestra valores comprensibles; nunca se pide teclear el identificador.
- **RF-V2-MD-01:** cada relación maestro-detalle identificada dispone de una operación atómica de alta mediante una rutina de PostgreSQL que inserta encabezado y todos los detalles. Quedan prohibidos INSERT separados desde la API para una misma operación de negocio.
- **RF-V2-MD-02:** modificación de maestro y colección completa de detalles mediante rutina de PostgreSQL, con control transaccional.
- **RF-V2-MD-03:** consulta individual y listado de maestro-detalle mediante rutinas de PostgreSQL; el resultado de datos compuestos se devuelve en JSONB.
- **RF-V2-MD-04:** eliminación o inactivación mediante rutina de PostgreSQL (borrado lógico cuando la metodología lo exija).
- **RF-V2-DB-01:** triggers únicamente donde existan invariantes, totales derivados o desnormalización controlada que los requieran.
- **RF-V2-DB-02:** vistas justificadas por consultas recurrentes y requerimientos reales.

## Calidad y trazabilidad
- **RF-MIRA-ERR-01:** el frontend conserva y muestra los mensajes de validación/conflicto de la API, incluidos HTTP 400 y 409; una respuesta no JSON o con estructura incompatible no causa un error 500 en la interfaz. Los listados muestran un aviso cuando los datos no están disponibles.
- **RF-MIRA-VAL-01:** la API rechaza con 400 los textos que superan los límites VARCHAR de la migración 001, antes de intentar persistirlos.
- **RF-MIRA-TC-01:** crear un término cuya clave ya existe, incluso inactiva o insertada concurrentemente, devuelve 409; no reactiva ni sobrescribe registros.
- **RF-MIRA-CSRF-01:** un POST sin token, con token incorrecto o con caracteres no ASCII en el token devuelve 400 sin ejecutar escrituras.
- **RNF-MIRA-OPS-01:** el arranque y la parada locales conservan la información; las pruebas de base de datos se ejecutan en una base exclusiva de pruebas.
- **RNF-MIRA-ENV-01:** las versiones directas declaradas de Python y Werkzeug coinciden con el entorno utilizado para la verificación; una comprobación local no acredita el despliegue Docker ni la ausencia de vulnerabilidades.
- Arquitectura Controller → Service → Repository → PostgreSQL y principios SOLID.
- Validar claves foráneas del lado del servidor, no solo en el frontend.
- No inventar entidades ni relaciones que no estén en el modelo oficial. No construir maestro-detalle ficticios a partir de las seis tablas independientes de la Entrega 1.
- La interfaz refleja el estado real: no presenta como terminadas funciones académicas de V2 que dependen del modelo oficial.

## Historias y aceptación de la corrección de persistencia

| Historia | Necesidad | Aceptación |
|---|---|---|
| HU-MIRA-CAT-01 | Como usuario, administrar áreas de conocimiento. | RF-MIRA-CAT-01: ciclo de cinco operaciones, inactivación y validaciones, ahora mediante rutinas. |
| HU-MIRA-CAT-02 | Como usuario, administrar ODS. | RF-MIRA-CAT-02 con el mismo ciclo y datos persistidos. |
| HU-MIRA-CAT-03 | Como usuario, administrar áreas de aplicación con nombres sin espacios extremos. | RF-MIRA-CAT-03 y RF-V2-TR-01; el formulario envía el valor original y PostgreSQL devuelve el normalizado. |
| HU-MIRA-CAT-04 | Como usuario, administrar términos clave sin duplicar claves activas/inactivas. | RF-MIRA-CAT-04, RF-MIRA-TC-01; conflicto 409 sin sobreescritura. |
| HU-MIRA-CAT-05 | Como usuario, administrar universidades. | RF-MIRA-CAT-05 con validaciones de nombre, tipo y ciudad. |
| HU-MIRA-CAT-06 | Como usuario, administrar líneas de investigación. | RF-MIRA-CAT-06 y descripción opcional persistida. |
| HU-MIRA-ERR-01 | Como usuario, entender errores sin recibir detalles internos. | RF-V2-HTTP-01, RF-MIRA-ERR-01; mensajes españoles y estados comprobados. |

Estas historias formalizan el alcance existente y la nueva instrucción del usuario; no se atribuyen a un documento oficial no aportado. Las historias de grupos, semilleros, docentes y asociaciones académicas están bloqueadas por el modelo oficial. No se inventan columnas para redactarlas.

## Estado acumulativo
| Componente | Clasificación | Estado |
|---|---|---|
| V1: 6 catálogos con CRUD, borrado lógico y frontend Flask, sin login | Obligatorio | **Hecho**; regresión automatizada y E2E |
| Dashboard con indicadores reales (conteos desde la API) | Obligatorio | Hecho |
| Cinco operaciones V1 por rutinas PostgreSQL | Obligatorio (prompt definitivo C03) | Hecho: migración 004, 30 funciones y seis repositorios |
| Trigger de regla existente demostrable | Obligatorio (C04) | Hecho: normalización de área en 005, probada por SQL/API/formulario/navegador |
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
