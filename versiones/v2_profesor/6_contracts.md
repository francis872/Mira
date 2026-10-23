# Contratos API / base de datos V2

## Patrón propuesto (contrato, no tabla existente)
POST /api/{maestros}  -> servicio -> repositorio -> rutina SQL crear_maestro_con_detalles(maestro..., detalles jsonb)
PUT /api/{maestros}/{id} -> rutina actualizar_maestro_con_detalles(id, encabezado..., detalles jsonb)
GET /api/{maestros}/{id} -> rutina consultar_maestro_con_detalles(id), resultado JSONB
GET /api/{maestros} -> rutina listar_maestros_con_detalles(filtros...), resultado JSONB
DELETE /api/{maestros}/{id} -> rutina eliminar_o_inactivar_maestro_con_detalles(id)

Se sustituirán los nombres genéricos una vez identificado el modelo real. Los endpoints no harán dos escrituras independientes para la misma transacción de negocio.

### Estructura ilustrativa de entrada
```json
{"maestro":{"campo":"valor"},"detalles":[{"referenciaId":1,"cantidad":2}]}
```
La validación de entrada debe fallar antes de persistir o causar rollback.

## Contratos implementados (verificados)
Ninguna ruta de la API exige credenciales ni token de autenticación. Los errores controlados usan `{"mensaje": "..."}`. La validación automática devuelve HTTP 400 con `mensaje` y diccionario `errors`, ambos con textos en español; Flask muestra esos mensajes. El middleware traduce 23503/23505 a 409, 23502/22023/22001 a 400 y errores inesperados a 500, sin publicar detalles internos.

| Método y ruta | Éxito | Errores |
|---|---|---|
| `GET /health` | 200 | — |
| `GET /api/{catálogo}` y `/{id}` (6 catálogos V1; `termino_clave` usa el término como clave) | 200 | 404 |
| `POST /api/{catálogo}` | 201 + `Location` | 400 (validación), 409 (`termino_clave` duplicado) |
| `PUT /api/{catálogo}/{id}` | 200 | 400, 404 |
| `DELETE /api/{catálogo}/{id}` (borrado lógico) | 200 | 404 |

No existen `/api/auth/*`, `/login`, `/logout` ni `/usuarios` (404).

Procedimientos PostgreSQL disponibles solo en la base (sin ruta HTTP): `CALL sp_usuario_crear(correo, hash, roles jsonb, INOUT resultado jsonb)`, `sp_usuario_actualizar(id, correo, roles, INOUT resultado)`, `sp_usuario_inactivar(id, INOUT inactivado boolean)`, `sp_usuario_bootstrap_admin(correo, hash, INOUT resultado)`. SQLSTATE: `23505` duplicado, `23503`/`22023` datos o roles inválidos, `P0002` usuario inexistente. Prueba: `database/tests/v2_usuarios_roles.sql`.

Flask: `GET /` (dashboard), `/{catálogo}`, `/{catálogo}/nuevo`, `POST /{catálogo}/crear`, `/{catálogo}/editar/{id}` (GET/POST), `POST /{catálogo}/eliminar/{id}`. Todo POST exige `csrf_token` (400 si falta o no coincide).

## Contratos de rutinas CRUD V1

La migración 004 implementa cinco funciones por cada una de las seis tablas. Todas se invocan con SELECT; no son procedimientos CALL pese al prefijo `sp_` solicitado:

| Función | Parámetros | Retorno |
|---|---|---|
| `sp_listar_<tabla>()` | Ninguno | SETOF tabla, solo activos, orden por ID o término |
| `sp_consultar_<tabla>(clave)` | ID integer; término text en `termino_clave` | SETOF tabla, cero o una fila activa |
| `sp_crear_<tabla>(campos...)` | Campos de entrada text, orden de la migración | Fila persistida incluyendo valores del trigger; activo=true |
| `sp_actualizar_<tabla>(clave, campos...)` | Clave y campos editables; no cambia término PK | Boolean de modificación de fila activa |
| `sp_eliminar_<tabla>(clave)` | Clave | Boolean de inactivación, sin borrado físico |

Ejemplo real Repository: `SELECT * FROM public.sp_crear_universidad(@Nombre, @Tipo, @Ciudad);`. Dapper mapea columnas con underscore a propiedades por `DefaultTypeMap.MatchNamesWithUnderscores = true`. No hay consultas CRUD directas sobre tablas en C#.

Campos por alta: área de conocimiento `(gran_area, area, disciplina)`; ODS `(nombre, categoria)`; área de aplicación `(nombre)`; término `(termino, termino_ingles)`; universidad `(nombre, tipo, ciudad)`; línea `(nombre, descripcion)`. Las modificaciones anteponen la clave y usan esos campos salvo término, que solo modifica `termino_ingles`.

La migración 005 normaliza el nombre de área antes de persistirlo. El POST/PUT de Flask y `AreaAplicacionService` lo envían sin recortar; el trigger realiza la transformación y la API de alta devuelve el resultado final. Las tablas legadas usuario→roles de 003 conservan su contrato CALL y no se exponen.

## Medidas y límites de entrada
- Límites de texto alineados con la migración 001: área de conocimiento (150 por campo), ODS (nombre 200, categoría 100), área de aplicación (200), término y equivalente inglés (150), universidad (nombre 200, tipo y ciudad 100), línea (nombre 200; descripción TEXT). Excederlos devuelve 400 en POST/PUT.
- Duplicar un término activo, inactivo o creado concurrentemente devuelve 409; no se reactiva el registro.
- Una respuesta no JSON o con estructura incompatible se presenta como información no disponible en el frontend; los valores ingresados se conservan al fallar una escritura.
- Sin autenticación: reservada para una versión futura.
- Validación de entrada y SQL parametrizado; respuestas sin cadenas de conexión ni datos secretos.
- El frontend llenará las FK del dominio académico con GET a catálogos, mostrando nombre y enviando ID (pendiente del modelo oficial).
