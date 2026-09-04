# Contratos MIRA V1 - Catalogos

Todos los endpoints son publicos en esta V1, sin login ni JWT. El prefijo comun es `/api`.

## Recursos

Los recursos son `usuarios`, `roles`, `sedes`, `areas-conocimiento`, `ods`, `areas-aplicacion` y `palabras-clave`.

Cada recurso expone:

- `GET /api/{recurso}`: lista activos; 200.
- `GET /api/{recurso}/{id}`: consulta por ID; 200 o 404.
- `POST /api/{recurso}`: crea; body con campos editables; 201, 400 o 409.
- `PUT /api/{recurso}/{id}`: reemplazo completo; 200, 400 o 404.
- `PATCH /api/{recurso}/{id}`: actualizacion parcial; 200, 400 o 404.
- `DELETE /api/{recurso}/{id}`: actualiza estado a inactivo; 204 o 404.

No existe DELETE SQL fisico.

## Bodies

`sedes`: `{ "nombre": "Sede Medellin", "ciudad": "Medellin", "activa": true }`.

Los otros seis recursos usan `{ "nombre": "Valor", "activo": true }`, excepto `usuarios`, que queda documentado para consulta administrativa pero no activa autenticacion.

## Errores

400 para body invalido o ID invalido, 404 para recurso inexistente, 409 para nombre duplicado y 500 para error no controlado. No se usa body en GET.

## Ejemplo

```http
GET /api/areas-conocimiento/1
```

```json
{
  "id": 1,
  "nombre": "Ingenieria de Software",
  "activo": true
}
```

Swagger expone y permite probar los contratos implementados.
