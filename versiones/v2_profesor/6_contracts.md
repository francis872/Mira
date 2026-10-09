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

## Seguridad
- POST /api/auth/login: 200 si válido; 401 si credenciales inválidas.
- Rutas protegidas: 401 sin identidad válida, 403 con identidad válida sin permiso.
- Respuestas sin contraseñas, hashes, cadenas de conexión ni datos secretos.
- El frontend llena FK con GET a catálogos permitidos, mostrando nombre y enviando ID.
