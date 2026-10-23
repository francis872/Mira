# Fuentes recuperadas del historial

Recuperadas el 23 de octubre de 2026 desde el commit `157f61f137ab13ea1f1535724fe532595f360588`, conservando exactamente los bytes de los blobs Git originales. Estos archivos no estaban en la rama activa; sí existían en versiones anteriores del repositorio.

| Archivo | Tipo | Blob Git original |
|---|---|---|
| Plan de desarrollo Módulo de Investigación - Plataforma Académica (1).docx | Documento fuente histórico | `f229241d7f7e634d202aaafcb9b1c48c5365f906` |
| Historias de usuario micro proyecto gestión del desempeño V 1.0 (1).docx | Documento fuente histórico | `6eb64308cea6fb834083a01370903fc89ec340c1` |
| Cronograma Módulo de Investigación - Plataforma Académica .xlsx | Documento fuente histórico | `e39c871cfa0888fc10517e869706b5b574077aa7` |
| plan_desarrollo_extraido.txt | Extracción de texto ya presente en ese commit | `5d439c6b254219a46a929fd552b4fcbf26e0b527` |
| historias_extraido.txt | Extracción de texto ya presente en ese commit | `088d8541e41d5f5721aac2bc28956aa619583902` |
| cronograma_extraido.txt | Extracción de texto ya presente en ese commit | `b3d6c793af5614525189627e4da7de8d498f4b27` |

## Qué aclaran y qué no

Las historias describen grupos vinculados a sedes, semilleros vinculados a grupos activos, perfiles docentes, líneas y asociaciones con catálogos. El plan incluye decisiones históricas que difieren del stack y alcance actuales, incluido Oracle. Son evidencia del proyecto histórico; no establecen automáticamente el esquema físico de Entrega 2 ni sustituyen las instrucciones posteriores del curso.

También se revisó la migración `database/migrations/002-complete-research-model.sql` del commit `880ac75`. Es código histórico de otra revisión: usa `sedes`, catálogos plurales y docentes ligados a `usuarios`, con diferencias frente a la rama actual. No se restauró al directorio activo de migraciones ni se ejecutó como si fuera el modelo oficial.

No se localizaron `VERSION_2.md`, `0_METODOLOGIA.md`, `0_mapa_versiones.md`, `modulo_*.md`, `proyecto_completo.md` ni un archivo oficial de esquema relacional en los objetos Git disponibles. Las ramas remotas consultadas coinciden con las referencias inspeccionadas; la búsqueda de `VERSION_2 modelo relacional` en GitHub no devolvió un documento adicional.

Ante estas diferencias se consultó al usuario, quien eligió **«Usar únicamente el modelo oficial»**. No se deducen nuevas columnas, FK opcionales, reglas de retiro o pantallas académicas a partir del esquema antiguo. Para cerrar V2 falta el esquema y mapa de versiones aplicables a la entrega actual.
