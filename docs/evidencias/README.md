# Evidencia visual de MIRA V2

Capturas reales obtenidas con Edge headless sobre la plataforma local después de aplicar migraciones 004/005 y el rediseño autorizado, el 23 de octubre de 2026:

- [Dashboard de escritorio, viewport 1440 × 1000](mira-v2-desktop.png).
- [Área de aplicación y menú móvil, viewport 390 × 844](mira-v2-mobile.png).
- [Formulario de universidad, viewport 1440 × 1000](mira-v2-form.png).

Se capturó la página completa, por lo que la altura del archivo puede superar la del viewport. No son imágenes generadas ni maquetas. Muestran datos reales almacenados localmente y las semillas del proyecto. Los registros `e2e-browser-*` de la comprobación se retiraron mediante el formulario antes de capturar.

`scripts/verify-browser.mjs` comprueba diez aspectos con CDP y APIs incorporadas de Node 24, incluido el ciclo real de formularios, teclado y 52 combinaciones de página/viewport. El menú móvil se abre y cierra en la prueba; la captura muestra el contenido después de cerrarlo. No requiere instalar Playwright o Selenium. Es una comprobación adicional a las 31 pruebas HTTP. Dependencias y reproducción: [scripts/README.md](../../scripts/README.md). Resultados y alcance: [diseño](../diseno-v2.md) y [pruebas](../pruebas-v2.md).
