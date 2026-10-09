# Modelo de datos V2

## Estado verificable de V1
El archivo database/init/01_create_tables.sql define seis entidades independientes: area_conocimiento, objetivo_desarrollo_sostenible, area_aplicacion, termino_clave, universidad y linea_investigacion. Esta entrega no define FK ni pares maestro-detalle.

## Pendiente antes de migrar
Incorporar las tablas de la siguiente entrega desde el modelo oficial y completar para cada par:
| Maestro | Detalle | PK maestro | FK detalle | Reglas de eliminación | Reglas de cálculo |
|---|---|---|---|---|---|
| Por identificar | Por identificar | Pendiente | Pendiente | Pendiente | Pendiente |

## Normas
- FK con restricciones PostgreSQL reales y selección mediante catálogos activos.
- Validación de existencia y autorización del FK en backend.
- Evitar almacenar subtotales redundantes salvo justificación; si se almacenan, definir trigger de consistencia.
- Diseñar migraciones aditivas, con respaldo previo y pruebas de reversión.
