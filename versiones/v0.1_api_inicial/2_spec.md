# MIRA v0.1 — Specification

## Purpose

Gestionar seis catálogos de apoyo a la investigación académica. Esta revisión documenta retrospectivamente el código existente; no sustituye los documentos oficiales que faltan en `docs/fuentes/`.

## Scope

API REST, frontend Flask y persistencia PostgreSQL para los seis catálogos independientes de la migración 001.

## Included

Consulta de activos, alta, edición y borrado lógico. Dashboard con conteos obtenidos de la API. Formularios protegidos con token CSRF.

## Explicitly excluded

- Autenticación y autorización: fuera de V1/V2, conforme a `docs/1_constitution.md`.
- Entidades y relaciones del modelo académico V2: dependen del documento oficial de la Entrega 2.

## Functional requirements

| ID | Comportamiento |
|---|---|
| RF-MIRA-CAT-01 | CRUD con inactivación de áreas de conocimiento (gran área, área y disciplina). |
| RF-MIRA-CAT-02 | CRUD con inactivación de ODS (nombre y categoría). |
| RF-MIRA-CAT-03 | CRUD con inactivación de áreas de aplicación (nombre). |
| RF-MIRA-CAT-04 | CRUD con inactivación de términos clave (clave textual inmutable y equivalente inglés opcional). |
| RF-MIRA-CAT-05 | CRUD con inactivación de universidades (nombre, tipo y ciudad). |
| RF-MIRA-CAT-06 | CRUD con inactivación de líneas de investigación (nombre y descripción opcional). |
| RF-MIRA-DASH-01 | Mostrar conteos reales y señalar información no disponible sin inventar números. |
| RF-MIRA-OPEN-01 | Acceso directo al dashboard y a catálogos sin credenciales; no existen rutas de login. |

## Non-functional requirements

Controller → Service → Repository → PostgreSQL, inyección de dependencias, SQL parametrizado, escape HTML y CSRF. Límites de texto, manejo de errores y operación local se detallan en `versiones/v2_profesor/2_spec.md`.

## Acceptance criteria

Los seis ciclos de alta, lectura desde un cliente nuevo, modificación e inactivación pasan contra PostgreSQL real. La fila inactiva permanece en la base y deja de aparecer en las consultas de activos. El dashboard coincide con los listados.

## Definition of Done

Compilación y pruebas documentadas en `docs/pruebas-v2.md`; trazabilidad en `docs/trazabilidad-v2.md`. La auditoría local no cierra la entrega académica V2 ni acredita producción.
