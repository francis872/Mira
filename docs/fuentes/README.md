# Documentos fuente del proyecto

- [SQL oficial confirmado por el usuario](sql/README.md): `01-init.sql` y `002-complete-research-model.sql`, recuperados sin modificar desde `repair/v1-catalogos`, commit `880ac75`.
- [Plan, historias y cronograma históricos](historicos/README.md), conservados con sus blobs originales.

El usuario confirmó expresamente ambos SQL el 23 de octubre de 2026 después de indicar que el modelo estaba en el repositorio. Esta confirmación corrige la incertidumbre documentada en la auditoría anterior. La incorporación aditiva está en la migración 006 y las rutinas en 007. No se deducen nuevas columnas ni FK opcionales.

Siguen sin localizarse `VERSION_2.md`, el mapa de versiones y los nombres oficiales de los nueve documentos del curso. Los prompts auténticos están en `docs/prompts/`; no se alteran para hacerlos coincidir retrospectivamente con la implementación.
