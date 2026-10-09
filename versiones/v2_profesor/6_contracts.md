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
Ninguna ruta exige credenciales ni token. Los errores usan `{"mensaje": "..."}`.

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

## Seguridad (medidas generales)
- Sin autenticación: reservada para una versión futura.
- Validación de entrada y SQL parametrizado; respuestas sin cadenas de conexión ni datos secretos.
- El frontend llenará las FK del dominio académico con GET a catálogos, mostrando nombre y enviando ID (pendiente del modelo oficial).
