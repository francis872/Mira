# Modelo de datos MIRA V1

Fuente unica: `database/init/01-init.sql`.

## Tablas sin FK saliente

| Tabla | Columnas reales | PK | FK saliente | Estado |
|---|---|---|---|---|
| usuarios | id SERIAL, nombre VARCHAR(120), correo VARCHAR(160), password_hash VARCHAR(200), created_at TIMESTAMP | id | No | Estructura heredada; sin API ni seed en V1 |
| roles | id SERIAL, nombre VARCHAR(50) | id | No | Estructura heredada; sin API ni seed en V1 |
| sedes | id SERIAL, nombre VARCHAR(120), ciudad VARCHAR(120), activa BOOLEAN, created_at TIMESTAMP | id | No | activa |
| areas_conocimiento | id SERIAL, nombre VARCHAR(160), activo BOOLEAN | id | No | activo |
| ods | id SERIAL, nombre VARCHAR(160), activo BOOLEAN | id | No | activo |
| areas_aplicacion | id SERIAL, nombre VARCHAR(160), activo BOOLEAN | id | No | activo |
| palabras_clave | id SERIAL, nombre VARCHAR(160), activo BOOLEAN | id | No | activo |

## Tabla excluida

`usuario_roles` tiene FK a `usuarios` y `roles`; no es catalogo V1.

Las tablas `usuarios`, `roles` y `usuario_roles` se conservan para no romper la base inicial, pero no forman parte de los contratos ni del flujo funcional de V1.

## Restricciones

Los nombres son NOT NULL y unicos en los cuatro catalogos. `sedes.nombre` y `sedes.ciudad` son NOT NULL. Las tablas de catalogo usan estado booleano para borrado logico.

La API no rediseña columnas ni crea relaciones nuevas.

## Coherencia con el modelo físico ampliado

La base local también contiene las tablas de investigación preparadas por la migración `database/migrations/002-complete-research-model.sql`. Estas tablas no forman parte del backend ni de los contratos de V1.

La base viva consolida identificadores `INTEGER`/`SERIAL`. El modelo histórico v0.1 describe `usuarios.id` y `docentes.usuario_id` como `UUID`, pero esa migración no se aplica sobre las PK existentes porque rompería datos y relaciones. Cuando se implemente el módulo de investigación deberá usarse `INTEGER` mientras no exista una migración formal de identidad aprobada.
