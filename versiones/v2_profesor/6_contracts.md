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
| Método y ruta | Acceso | Éxito | Errores |
|---|---|---|---|
| `POST /api/auth/login` | Público | 200 `{accessToken, tokenType, expiresAtUtc, roles}` | 401 |
| `GET /api/auth/roles` | Administrador | 200 | 401, 403 |
| `GET /api/auth/usuarios` y `/{id}` | Administrador | 200 | 401, 403, 404 |
| `POST /api/auth/usuarios` `{correo, password(≥12), roles[]}` | Administrador | 201 + `Location` | 400, 401, 403, 409 |
| `PUT /api/auth/usuarios/{id}` `{correo, roles[]}` | Administrador | 200 | 400, 401, 403, 404 |
| `DELETE /api/auth/usuarios/{id}` | Administrador | 204 | 401, 403, 404 |
| `GET /api/{catalogo}` y `/{id}` (6 catálogos V1) | Cualquier usuario autenticado | 200 | 401, 404 |
| `POST/PUT/DELETE /api/{catalogo}` | Administrador o Coordinador | 201/200 | 400, 401, 403, 404 |

Procedimientos PostgreSQL: `CALL sp_usuario_crear(correo, hash, roles jsonb, INOUT resultado jsonb)`, `sp_usuario_actualizar(id, correo, roles, INOUT resultado)`, `sp_usuario_inactivar(id, INOUT inactivado boolean)`, `sp_usuario_bootstrap_admin(correo, hash, INOUT resultado)`. SQLSTATE: `23505` duplicado, `23503`/`22023` datos o roles inválidos, `P0002` usuario inexistente.

Flask (cookie de sesión): `GET/POST /login`, `POST /logout`, `/usuarios` (solo Administrador).

## Seguridad
- POST /api/auth/login: 200 si válido; 401 si credenciales inválidas.
- Rutas protegidas: 401 sin identidad válida, 403 con identidad válida sin permiso.
- Respuestas sin contraseñas, hashes, cadenas de conexión ni datos secretos.
- El frontend llena FK con GET a catálogos permitidos, mostrando nombre y enviando ID.
