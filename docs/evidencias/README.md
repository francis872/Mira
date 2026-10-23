# Evidencia visual de MIRA V2

Capturas reales obtenidas con Edge headless sobre la plataforma local después de aplicar migraciones 004/005, el 23 de octubre de 2026:

- [Dashboard de escritorio, viewport 1440 × 1000](mira-v2-desktop.png).
- [Área de aplicación y menú móvil, viewport 390 × 844](mira-v2-mobile.png).

Se capturó la página completa, por lo que la altura del archivo puede superar la del viewport. No son imágenes generadas ni maquetas. Muestran datos reales almacenados localmente y las semillas del proyecto. Los registros `e2e-browser-*` de la comprobación se retiraron mediante el formulario antes de capturar.

`scripts/verify-browser.mjs` comprueba siete recorridos con CDP y APIs incorporadas de Node 24. No requiere instalar Playwright o Selenium. Es una comprobación de navegador real adicional a las 31 pruebas HTTP. Dependencias y reproducción: [scripts/README.md](../../scripts/README.md). Resultados: [pruebas-v2.md](../pruebas-v2.md).
