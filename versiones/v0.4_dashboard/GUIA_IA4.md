# Guía de Trabajo con IA — Versión 4 (`GUIA_IA4.md`)

> **Eslabón entre Especificación (`v0.4`) y Código Construido**  
> Documenta los prompts utilizados para construir el cierre del aplicativo (10 consultas multitabla, Dashboard con Chart.js, Manual de Marca, Páginas Corporativas y PWA) y las correcciones aplicadas.

---

## 1. Prompt Principal de Analítica Multitabla, Dashboard y Cierre Corporativo (v0.4)

```text
Implementa la Versión 4 (v0.4) completa del proyecto MIRA cumpliendo la metodología SDD (0_METODOLOGIA.md) y el Spec Kit en versiones/v0.4_dashboard/:
1. Crea el Manual de Marca en MANUAL_DE_MARCA.md y vincula todos los estilos al archivo frontend/static/css/marca.css usando variables CSS y verificando relaciones de contraste WCAG AA.
2. En MIRA.Api, implementa ReporteMultitabla.cs, IReporteRepository, ReporteRepository, IReporteService, ReporteService y ReportesController exponiendo /api/reportes/dashboard y /api/reportes/consulta/{1..10}, donde cada una de las 10 consultas SQL en Dapper une un mínimo de 4 tablas (entre 4 y 7 tablas) filtrando registros activos.
3. En frontend/, construye la vista /dashboard con gráficos interactivos en Chart.js y pestañas para explorar las 10 consultas multitabla, las 5 páginas corporativas (Inicio, Servicios, Nosotros, Soporte, Contacto) y el soporte PWA (manifest.json y sw.js).
```

## 2. Correcciones Realizadas a la IA durante la Versión 4

1. **Uso de `LEFT JOIN` y `COALESCE` en Agregaciones Multitabla de 5 a 7 Tablas:** En consultas que cruzan catálogos y múltiples tablas puente (como la Consulta 9 de Mapa Integral de Líneas o la Consulta 8 de Capacidad por Universidad), un `INNER JOIN` estricto ocultaría universidades o líneas válidas que aún no tienen semilleros en una de las ramas. Se corrigió combinando `INNER JOIN` en las relaciones obligatorias y `LEFT JOIN` con `COUNT(DISTINCT ...)` en las agregaciones para no perder filas activas ni duplicar conteos por producto cartesiano.
2. **Contraste del Color Ámbar (`--mira-accent: #D97706`) según WCAG AA:** Se verificó mediante la fórmula de luminancia relativa que `#D97706` sobre blanco tiene ratio `3.10:1` (insuficiente para texto pequeño blanco). Se documentó en `MANUAL_DE_MARCA.md` y se aplicó en `marca.css` que los botones/badges ámbar siempre utilicen texto oscuro `#0F172A` (ratio `8.9:1`).
