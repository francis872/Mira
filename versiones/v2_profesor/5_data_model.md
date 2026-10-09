# Modelo de datos V2

## Estado verificable de V1
Los scripts `database/migrations/001_v1_catalogos_tablas.sql` y `002_v1_catalogos_datos_iniciales.sql` definen seis entidades independientes: area_conocimiento, objetivo_desarrollo_sostenible, area_aplicacion, termino_clave, universidad y linea_investigacion. Esta entrega no define FK ni pares maestro-detalle entre ellas.

## Migraciones
`database/migrations/NNN_*.sql` + `apply.sh` (registro en `schema_migrations`). En una base nueva Docker las aplica `database/init/00-migrar.sh`; en una existente: `docker compose exec mira-postgres sh /migrations/apply.sh`. Son idempotentes y aditivas: no borran tablas ni datos.

## Pares maestro–detalle identificados
`usuario`/`usuario_rol` (migración 003) existe **solo en la base de datos**: se conserva con sus datos y sus rutinas almacenadas, pero queda fuera de la aplicación porque implica credenciales (fuera de alcance). No se expone en la API ni en el frontend.
| Maestro | Detalle | PK maestro | FK detalle | Reglas de eliminación | Reglas de cálculo |
|---|---|---|---|---|---|
| `usuario` | `usuario_rol` | `usuario.id` (BIGINT) | `usuario_rol.usuario_id → usuario.id`; `usuario_rol.rol_id → rol.id` | Lógica (`usuario.activo`); sin `ON DELETE CASCADE` | Ninguna |

Tipos reales: `usuario.id BIGINT`, `rol.id INTEGER`. Las tablas V1 (`area_conocimiento`, etc.) no tienen FK.

## Pendiente antes de migrar el dominio académico
Incorporar las tablas de la siguiente entrega desde el modelo oficial (**no disponible en el repositorio**) y completar para cada par:
| Maestro | Detalle | PK maestro | FK detalle | Reglas de eliminación | Reglas de cálculo |
|---|---|---|---|---|---|
| Por identificar | Por identificar | Pendiente | Pendiente | Pendiente | Pendiente |

## Normas
- FK con restricciones PostgreSQL reales y selección mediante catálogos activos.
- Validación de existencia y autorización del FK en backend.
- Evitar almacenar subtotales redundantes salvo justificación; si se almacenan, definir trigger de consistencia.
- Diseñar migraciones aditivas, con respaldo previo y pruebas de reversión.
