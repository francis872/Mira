# Investigación y Decisiones Arquitectónicas (ADRs) — MIRA v0.4 (`4_research.md`)

> **Registro de Decisiones Técnicas para Analítica Multitabla, Visualización y Marca (`v0.4`)**

---

## Tabla Comparativa de Decisiones Técnicas (ADRs)

| # | Decisión | Opción Elegida | Alternativa Descartada | Justificación Técnica y Consecuencias |
| :-: | :--- | :--- | :--- | :--- |
| **ADR-1** | Motor de Consultas Multitabla (4+ tablas) | **Consultas SQL nativas con `JOIN` y `GROUP BY` en Dapper (`ReporteRepository.cs`)** | Cruzar tablas en memoria con bucles `foreach` en C# o Python | Hacer múltiples consultas y unirlas en memoria causa el antipatrón $N+1$ y alto consumo de red. El motor relacional PostgreSQL optimiza los planes de ejecución de `JOIN` entre 4 y 7 tablas en una sola consulta. |
| **ADR-2** | Librería de Gráficos en el Frontend | **Chart.js 4.x (Canvas HTML5 responsive)** | Gráficos estáticos generados como imágenes PNG en el servidor (`matplotlib`) | Chart.js permite tooltips interactivos, leyendas clicables, redimensionamiento automático en móviles y uso directo de los colores corporativos de `marca.css`. |
| **ADR-3** | Arquitectura de Estilos del Manual de Marca | **Variables CSS Nativas (`:root` en `static/css/marca.css`)** |Valores hexadecimales repetidos en atributos `style="..."` | Centralizar la paleta y tipografía en `marca.css` convierte al `MANUAL_DE_MARCA.md` en una restricción técnica verificable en un único archivo. |
| **ADR-4** | Soporte Móvil y PWA | **Web App Manifest (`manifest.json`) + Service Worker (`sw.js`) + Bootstrap 5 Grid** | Aplicación móvil nativa separada | Permite instalar MIRA en dispositivos móviles y escritorio desde el navegador cumpliendo el estándar PWA sin duplicar el código del Frontend. |
