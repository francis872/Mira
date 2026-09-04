# Modelo de datos MIRA V1

Fuente unica: `database/init/01-init.sql`.

## Tablas sin FK saliente

| Tabla | Columnas reales | PK | FK saliente | Estado |
|---|---|---|---|---|
| usuarios | id SERIAL, nombre VARCHAR(120), correo VARCHAR(160), password_hash VARCHAR(200), created_at TIMESTAMP | id | No | No aplica en V1 por excluir autenticacion |
| roles | id SERIAL, nombre VARCHAR(50) | id | No | No aplica |
| sedes | id SERIAL, nombre VARCHAR(120), ciudad VARCHAR(120), activa BOOLEAN, created_at TIMESTAMP | id | No | activa |
| areas_conocimiento | id SERIAL, nombre VARCHAR(160), activo BOOLEAN | id | No | activo |
| ods | id SERIAL, nombre VARCHAR(160), activo BOOLEAN | id | No | activo |
| areas_aplicacion | id SERIAL, nombre VARCHAR(160), activo BOOLEAN | id | No | activo |
| palabras_clave | id SERIAL, nombre VARCHAR(160), activo BOOLEAN | id | No | activo |

## Tabla excluida

`usuario_roles` tiene FK a `usuarios` y `roles`; no es catalogo V1.

## Restricciones

Los nombres son NOT NULL y unicos en los cuatro catalogos. `sedes.nombre` y `sedes.ciudad` son NOT NULL. Las tablas de catalogo usan estado booleano para borrado logico.

La API no rediseña columnas ni crea relaciones nuevas.
