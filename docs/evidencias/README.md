# Evidencia visual real

Capturas obtenidas con Edge headless/CDP sobre la plataforma local, 23 de octubre de 2026. Son páginas completas, no maquetas:

- [Inicio en escritorio](mira-v2-desktop.png).
- [Catálogo en móvil](mira-v2-mobile.png).
- [Formulario V1](mira-v2-form.png).
- [Ficha de grupo con dos semilleros](mira-v2-grupo-semilleros.png).

El verificador `scripts/verify-browser.mjs` pasó 14 controles, 72 combinaciones de página/ancho y cero excepciones JavaScript. Comprueba V1, menú, teclado, nueva sede, selector de sede API, creación de dos semilleros, edición/retiro, asignación/duplicado/retiro del puente grupo–línea y diseño móvil de investigación. Los registros `e2e-browser-*` se retiran al terminar, incluidos los detalles; la captura de ficha documenta el estado previo a su retiro.

La ficha fue inspeccionada visualmente: controles de renglón visibles, edición accesible, tabla desplazable en móvil y acciones legibles. Complementa las pruebas SQL/.NET/Flask/HTTP; no certifica accesibilidad exhaustiva. [Resultados](../pruebas-v2.md).
