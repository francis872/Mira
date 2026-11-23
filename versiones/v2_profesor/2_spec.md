# MIRA V2 — Especificación funcional

## Fuente y alcance

El usuario confirmó como modelo oficial los SQL de `repair/v1-catalogos`, commit `880ac75`, preservados en `docs/fuentes/sql/`. La especificación previa de integración está en `docs/integracion-modelo-oficial.md`. Se conservan C#, ASP.NET Core .NET 10, Flask y PostgreSQL; V1/V2 siguen sin autenticación. No se implementan login, JWT ni CRUD de usuarios/roles.

## Requisitos implementados

| Requisito e historia de trabajo | Comportamiento |
|---|---|
| RF-V2-SP-01 / HU-MIRA-CAT-01–06 | Cinco operaciones de los seis catálogos V1 por funciones; contratos conservados |
| RF-V2-ACA-01 / HU-V2-ACA-01 | Administrar las once entidades editables del SQL confirmado con sus campos y límites originales |
| RF-V2-FK-01 / HU-V2-REF-01 | Seleccionar cada FK con opciones reales de API, nombres comprensibles, vacíos y errores de carga |
| RF-V2-ASOC-01 / HU-V2-ASOC-01 | Asignar, consultar y retirar los cinco puentes; duplicados devuelven 409; parejas PK sin edición |
| RF-V2-MD-01–04 / HU-V2-FICHA-01 | Crear/consultar/editar grupo y colección de semilleros en una sola pantalla y una sola función JSONB; retiro lógico de detalles omitidos y rollback completo ante fallo |
| RF-V2-TR-01 / HU-MIRA-CAT-03 | Conservar normalización de área V1 y demostrar trigger oficial que rechaza grupo inactivo para semilleros |
| RF-V2-HTTP-01 / HU-MIRA-ERR-01 | 400 validación, 404 ausencia, 409 integridad/duplicados y 500 inesperado seguro, mensajes españoles |
| RF-V2-UI-01–02 | Navegación, formularios, búsqueda, confirmaciones, estados y adaptación móvil; conteos reales V1 |
| RF-V2-DB-01–02 | Restricciones y trigger oficiales; vistas con nombres de sede/persona/grupo evitan mostrar solo identificadores |
| RF-MIRA-CSRF-01 | POST de formularios requiere token, sin credenciales ni identidad de usuario |
| RNF-MIRA-OPS-01 | Migraciones aditivas registradas, respaldo previo local, pruebas en bases aisladas y datos existentes preservados |

Las historias de esta tabla identifican el trabajo de implementación y no se atribuyen a integrantes ni a documentos del profesor que no estén disponibles. Los documentos originales y prompts conservan su procedencia.

## Criterios específicos del modelo

Los catálogos plurales no sustituyen a las seis tablas singulares de V1. Grupo referencia sede; semillero referencia grupo; docentes referencia usuarios plurales, mediante selección de personas existentes de solo lectura. No se crea una relación universidad→grupo o línea→docente inexistente en el SQL.

Todos los FK oficiales son obligatorios. Texto/fechas opcionales vacíos se envían como null. C06 requiere aclaración de aplicabilidad del profesor; no se cambia la nulabilidad para aparentar una aprobación. [Matriz C01–C12](../../docs/criterios-v2.md).

Activa/activo implica retiro lógico y filtrado. Donde el esquema no tiene estado se elimina físicamente y se respetan las cascadas originales. No se añade una prohibición de retirar padres con detalles que el modelo no establece.

## Aceptación y pruebas

La regresión V1, CRUD V2, cinco puentes, trigger, referencias y atomicidad pasan pruebas reales de SQL/API/Flask/HTTP. C09 se demuestra también con Edge mediante dos semilleros, edición y retiro de renglones. La aceptación completa sigue condicionada a C06 y a los documentos del curso ausentes; no se acredita producción ni Docker.
