# Transición propuesta de MIRA a SAPIENS

SAPIENS es el nuevo nombre conceptual/oficial del producto. **El PR #7 conserva MIRA y su trazabilidad académica. Este documento es un plan: no se renombraron interfaz, proyectos, namespaces, servicios, repositorio o datos.**

Rama futura propuesta: `feature/sapiens-rebranding`, desde el commit V3 que haya sido aprobado. Si se integra a main, hacerlo después de esa integración autorizada por el responsable; esta revisión no crea la rama ni fusiona el PR. No mezclar el rebranding con las correcciones de cierre del PR #7.

## Etapas y dependencias

| Etapa | Cambio propuesto | Dependencias y prueba de salida |
|---|---|---|
| 0 Cierre MIRA | Aprobar alcance V1/V2/V3, C06 y evidencias; conservar SHA/documentos históricos | Acta académica y versión V3 identificada. Sin sustituir los nombres de fuentes del profesor. |
| 1 Identidad visible | Nombre SAPIENS, logotipo/favicon aprobado, títulos/metadatos, login/dashboard, mensajes y documentación vigente | Inventariar Jinja/base/login, SVG y textos; conservar iconografía funcional Tabler/licencia y accesibilidad. Repetir contraste, teclado, zoom, viewport y mensajes. No generar una identidad visual nueva sin decisión del responsable. |
| 2 README/documentación | Presentar «SAPIENS, anteriormente MIRA» y explicar la continuidad | Mantener MIRA V1/V2/V3, prompts, fuentes, actas, PR #7 y resultados históricos fechados. Añadir glosario/equivalencias; no reescribir evidencias como si siempre hubieran sido SAPIENS. |
| 3 Despliegue/configuración | Etiquetas visibles o nuevos nombres de servicios solo si aportan valor | Inventario de URLs, variables, secretos, directorios, puertos, volúmenes, callbacks, logs, scripts y CI; pruebas de compatibilidad y plan de reversión. Puede conservarse el nombre técnico MIRA. |
| 4 Identificadores internos opcionales | Evaluar solución/proyecto/namespace .NET, módulos Flask y scripts | Solo con beneficio demostrado; no es necesario para cambiar la marca. Recompilar todo, actualizar referencias, comandos, filtros de tests, Docker COPY/ENTRYPOINT y documentación en el mismo cambio revisado. |
| 5 Repositorio opcional | Evaluar Mira → Sapiens en GitHub | Autorización específica del propietario, inventario de remotos, enlaces, integraciones Render/Actions/webhooks y reglas. Probar enlaces/redirecciones reales; no suponer que toda integración seguirá funcionando. No renombrarlo desde este PR. |

## Identificadores que deben conservarse inicialmente

| Elemento real | Riesgo de cambiarlo | Decisión propuesta |
|---|---|---|
| MIRA.sln, MIRA.Api.csproj, namespaces MIRA.Api y ensamblado MIRA.Api.dll | Rompe ProjectReference, tests/filtros, bootstrap/start-local, Docker ENTRYPOINT, publish y rutas | Mantener en la primera entrega con nueva marca. Si se autoriza cambio posterior, migración atómica del grafo de referencias y regresión completa. |
| Jwt issuer MIRA y audience MIRA.Frontend | Tokens vigentes pasarían a 401 por validación aunque la firma sea correcta | Mantener. Una transición posterior requiere contrato/versionado y ventana de revocación/compatibilidad aprobada; no desactivar validación. |
| Variables MIRA_ENV, MIRA_TEST_DB, MIRA_E2E_* y MIRA_ADMIN_* | Scripts, tests, operadores y CI dejarían de configurar la aplicación correctamente | Mantener. Si se ofrecen aliases SAPIENS_*, documentar precedencia, rechazar conflictos y probar ambos antes de retirar alguno. |
| Compose name mira-v2 y volúmenes mira_postgres_data/mira_frontend_sessions | Otro project name puede crear volúmenes nuevos y aparentar pérdida de datos/sesiones | Mantener identificadores persistentes. Cualquier cambio exige mapear volúmenes explícitamente, backup/restauración y reversión aprobados; no borrar ni recrear datos por branding. |
| SESSION_DIRECTORY, SECRET_KEY y cookie opaca | Perder archivos/clave cierra sesiones; cambiar permisos expone serialización privada | Mantener almacenamiento y política; probar continuidad y fallo cerrado en despliegue aislado. No migrar secretos al repositorio. |
| Rutas /api, /auth, catálogos/investigación y nombres de blueprint | Rompe formularios/url_for, cliente Flask, tests y consumidores | Mantener contratos. Un cambio de marca no requiere cambiar rutas de negocio. |
| Tablas usuario/usuarios, roles, rutinas, triggers y migraciones 001–008 | Cambia dominios distintos, rompe FK y schema_migrations, puede volver a ejecutar SQL histórico | No renombrar. bcrypt y datos tampoco requieren transformación por el nombre del producto. |
| Nombre de repositorio/servicios Neon/Render | Enlaces, remotos, permisos e integraciones pueden depender de ellos | Cambio opcional y separado; nunca automático ni requisito para llamar SAPIENS a la interfaz. |

## Inventario y criterios de aceptación futuros

Antes de editar, usar rg para inventariar MIRA/Mira/mira y clasificar cada coincidencia como marca visible, contrato técnico, dato persistente o referencia histórica. No realizar reemplazo global. Ejemplos que exigen tratamiento distinto: un texto Jinja de bienvenida, MIRA.Api.dll en ENTRYPOINT, el issuer JWT, name:mira-v2 en Compose y el título de un prompt del profesor.

Actualizar las aserciones de marca visible (por ejemplo Bienvenido a MIRA en test_app) sin debilitar las pruebas de permisos, CSRF, expiración, SQL y revocación. Conservar el sprite/licencia y la arquitectura .NET → servicio → repositorio → PostgreSQL con Flask/Jinja; no implica migración tecnológica.

Criterios de salida: identidad aprobada, equivalencias históricas documentadas, seis casos de seguridad y regresiones completas, accesibilidad y zoom repetidos, misma base/datos/sesiones preservados según el contrato y rollback a una versión segura compatible. No se acepta «arranca con una base vacía» como prueba de migración de identidad correcta.

Decisiones pendientes: aprobación de la identidad visual y rama futura; necesidad real de cambiar identificadores internos; eventual nombre del repositorio y recursos; presupuesto y ventana si se trasladan servicios. Ninguna está ejecutada por este plan.
