# Guía de Trabajo con IA — Versión 2 (`GUIA_IA2.md`)

> **Eslabón entre Especificación (`v0.2`) y Código Construido**  
> Documenta los prompts utilizados para construir el modelo relacional y las 7 tablas puente de la Entrega 2, junto con las correcciones técnicas realizadas.

---

## 1. Prompt Principal de Entidades Fuertes con FK y Tablas Puente (v0.2)

```text
Ejecuta la implementación de la Versión 2 (v0.2) del sistema MIRA respetando la regresión de la v0.1 y el Spec Kit en versiones/v0.2_relaciones/:
1. Crea database/init/03_relaciones_v0.2.sql con las 3 entidades fuertes (docente, grupo_investigacion, semillero) y las 7 tablas puente (docente_grupo, docente_semillero, grupo_linea, semillero_linea, linea_area, linea_ods, linea_area_aplicacion), todas con activo BOOLEAN NOT NULL DEFAULT TRUE.
2. En la capa Servicios/, valida que toda clave foránea exista y tenga activo = true antes de insertar o actualizar; si no existe o está inactiva, lanza ArgumentException para que el controlador retorne 400 Bad Request.
3. En la capa Repositorios/, implementa consultas enriquecidas con LEFT JOIN e INNER JOIN en Dapper para retornar los nombres de las entidades relacionadas, y usa ON CONFLICT (...) DO UPDATE SET activo = TRUE en las tablas puente.
4. En frontend/, agrega las vistas con listas desplegables (<select>) alimentadas desde la API REST y el tablero Maestro-Detalle de articulaciones.
```

## 2. Correcciones Realizadas a la IA durante la Versión 2

1. **Validación de Claves Foráneas Lógicamente Eliminadas:** PostgreSQL por sí solo permite insertar una FK hacia un registro padre que existe físicamente pero tiene `activo = FALSE`. Se corrigió inyectando los repositorios padre dentro de `DocenteService`, `GrupoInvestigacionService`, `SemilleroService` y `RelacionesService` para invocar `GetByIdAsync(...)` (que filtra por `WHERE activo = TRUE`) antes de permitir el `INSERT` o `UPDATE`.
2. **Re-vinculación en Tablas Puente con Llave Compuesta:** Cuando se eliminaba lógicamente una relación en `grupo_linea` (`activo = FALSE`) y luego se intentaba volver a crear, un `INSERT` simple fallaba con error `23505` (llave duplicada). Se corrigió implementando `ON CONFLICT (...) DO UPDATE SET activo = TRUE` en los 7 métodos de creación de `RelacionesRepository.cs`.
