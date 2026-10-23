# Rediseño y actualización de MIRA en GitHub

23 de octubre de 2026. El usuario solicitó mejorar el diseño y autorizó nuevos commits y push. Confirmó que «desplegar» se refiere a actualizar el repositorio GitHub, sin publicar una aplicación con URL pública. Destino: `francis872/Mira`, rama `feature/mira-v1-v2-integracion`.

## Cambios visibles

- Navegación lateral en escritorio, página activa y menú plegable en móvil.
- Encabezado contextual según el catálogo y acceso a documentación API con etiqueta accesible.
- Dashboard con tarjetas uniformes, acciones «Ver listado»/«Nuevo» y conteos reales; estado técnico del proyecto en un bloque desplegable accesible por teclado.
- Tipografía, colores, contraste y espacios consistentes en los seis catálogos.
- Búsqueda y resultados agrupados, formularios con indicación de campos obligatorios y límites HTML alineados con el servidor.
- Confirmaciones de retiro con explicación en lenguaje común, sin exponer campos internos de la base.

Se conserva Flask/Jinja/Bootstrap, las rutas y operaciones CRUD, la protección CSRF y las validaciones del servidor. El encabezado reutiliza metadatos de los catálogos: no hace llamadas adicionales a la API. No se añadieron métricas, datos ficticios, autenticación ni entidades académicas nuevas.

## Archivos principales

`frontend/templates/base.html`, `index.html`, seis formularios y seis listados, `includes/catalog_toolbar.html`, `frontend/static/mira.css` y `frontend/app.py:inject_globals`. `scripts/verify-browser.mjs` amplía la verificación del diseño.

## Verificación ejecutada

`scripts/verify-local.ps1`: **41 pruebas .NET, 42 Flask/arquitectura y 31 E2E HTTP aprobadas**, con ambos SQL exitosos y base de pruebas exclusiva `mira_audit_20261023144423_9628`.

`node scripts/verify-browser.mjs`: **10 comprobaciones en Edge real**, sin excepciones JavaScript capturadas. Incluyen el ciclo de alta/edición/retiro desde formulario, búsqueda, modales, apertura/cierre de menú móvil, teclado y **52 combinaciones de página y ancho**: inicio, seis listados y seis formularios en 390, 768, 1024 y 1440 px, sin desbordamiento del documento ni superposición de barra lateral.

Se inspeccionaron las [capturas reales](evidencias/README.md) de escritorio, móvil y formulario. Esto no equivale a una auditoría exhaustiva de accesibilidad o de todos los navegadores.

## Publicación y límites

La implementación pendiente del prompt definitivo se registró primero en `1828a76` (`feat: integrar rutinas PostgreSQL y verificacion de MIRA V2`). El rediseño se registra en un commit posterior de la misma rama; ambos se suben al remoto autorizado. No se fusiona `main` ni se cambia el alcance académico bloqueado.

MIRA continúa funcionando localmente en `http://localhost:5000`. La actualización del repositorio no constituye hosting de Flask, ASP.NET Core o PostgreSQL. V2 académica completa mantiene seis criterios bloqueados por el modelo oficial ausente; [matriz de aceptación](criterios-v2.md).
