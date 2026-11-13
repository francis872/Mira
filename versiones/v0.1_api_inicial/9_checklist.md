# Lista de Chequeo de Requisitos (Compuerta 3) — MIRA v0.1 (`9_checklist.md`)

> **Estado de la Compuerta 3:** ✅ APROBADA (En Verde)  
> Cada requisito funcional y no funcional de la Versión 1 ha sido verificado como único, medible y comprobable contra el sistema en ejecución.

---

## 1. Verificación de las Tres Compuertas SDD

- [x] **Compuerta 1 (Clarificaciones en `2_spec.md`):** Sin etiquetas `[NECESITA ACLARACIÓN]` pendientes; definido el manejo de PK alfanumérica en `termino_clave` y el tipo `BOOLEAN DEFAULT TRUE` para `activo`.
- [x] **Compuerta 2 (Chequeo de Constitución en `3_plan.md`):** Respetada la separación en 4 capas (`Flask` → `Controller` → `Service` → `Repository` con Dapper) y prohibición de `DELETE` físico.
- [x] **Compuerta 3 (Lista de Requisitos en `9_checklist.md`):** Todos los criterios de aceptación de las 6 tablas catálogo base pasan en verde.

---

## 2. Matriz de Verificación por Catálogo (v0.1)

| Catálogo | GET Activos (`200`) | GET por ID (`200`/`404`) | POST Creación (`201`/`400`) | PUT Edición (`200`/`404`) | DELETE Lógico (`activo=false`) | Vista Web Flask |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| `area_conocimiento` | [x] | [x] | [x] | [x] | [x] | [x] |
| `objetivo_desarrollo_sostenible` | [x] | [x] | [x] | [x] | [x] | [x] |
| `area_aplicacion` | [x] | [x] | [x] | [x] | [x] | [x] |
| `termino_clave` (PK `string`) | [x] | [x] | [x] | [x] | [x] | [x] |
| `universidad` | [x] | [x] | [x] | [x] | [x] | [x] |
| `linea_investigacion` | [x] | [x] | [x] | [x] | [x] | [x] |
