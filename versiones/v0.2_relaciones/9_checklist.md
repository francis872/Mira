# Lista de Chequeo de Requisitos (Compuerta 3) — MIRA v0.2 (`9_checklist.md`)

> **Estado de la Compuerta 3:** ✅ APROBADA (En Verde)  
> Incluye regresión completa de `v0.1` y verificación de las Historias de Usuario HU 2 a HU 9.

---

## 1. Verificación de las Tres Compuertas SDD y Regresión

- [x] **Regresión `v1`:** Los 30 endpoints de los 6 catálogos base siguen respondiendo correctamente sin regresiones.
- [x] **Compuerta 1 (Clarificaciones en `2_spec.md`):** Definida la llave primaria institucional `cedula` en `docente` y el comportamiento de reactivación `ON CONFLICT DO UPDATE SET activo = TRUE` en las 7 tablas puente.
- [x] **Compuerta 2 (Chequeo de Constitución en `3_plan.md`):** Validación previa de claves foráneas activas ubicada exclusivamente en la capa `Servicios/` y consultas enriquecidas con `LEFT JOIN` / `INNER JOIN` en `Repositorios/`.
- [x] **Compuerta 3 (Requisitos Medibles):** Verificados todos los flujos de éxito (`200`/`201`) y rechazo por FK inexistente o inactiva (`400 Bad Request`).

---

## 2. Matriz de Verificación de Entidades con FK y Tablas Puente (v0.2)

- [x] `docente` (HU 3, HU 4, HU 7): CRUD por `cedula`, validación de `lineaInvestigacionPrincipal` activa y `LEFT JOIN` con `linea_investigacion`.
- [x] `grupo_investigacion` (HU 5, HU 7): CRUD con validación de `universidadId` y `docenteLiderCedula` activos e `INNER JOIN` enriquecido.
- [x] `semillero` (HU 6, HU 7): CRUD con validación de `grupoInvestigacionId` activo e `INNER JOIN` enriquecido.
- [x] 7 Tablas Puente (`docente_grupo`, `docente_semillero`, `grupo_linea`, `semillero_linea`, `linea_area`, `linea_ods`, `linea_area_aplicacion` — HU 5, 6, 8, 9): Vinculación idempotente (`ON CONFLICT DO UPDATE`) y desvinculación lógica (`activo = FALSE`) por *Query Parameters*.
- [x] Formularios en Flask con listas desplegables `<select>` cargadas dinámicamente desde la API REST.
