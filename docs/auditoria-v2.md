# Auditoría de MIRA V1 + V2 parcial

## Revisión por el prompt definitivo

El nuevo adjunto exige que Repository solo invoque rutinas. La revisión anterior aceptaba SQL directo parametrizado; ese criterio quedó sustituido explícitamente y se corrigieron los seis repositorios. Se añadieron 30 funciones (004), un trigger de normalización (005), traducción segura de errores y mejoras de interfaz. El detalle de C01–C12 está en [criterios-v2.md](criterios-v2.md), con reglas en [reglas-negocio-v2.md](reglas-negocio-v2.md).

La regresión inicial volvió a ejecutar 31 .NET, 40 Flask y 30 HTTP antes de cambiar código. La verificación posterior y los controles de navegador se registran en [pruebas-v2.md](pruebas-v2.md). Las evidencias anteriores de este documento describen la primera auditoría y no sustituyen los resultados actuales.

Hallazgos nuevos resueltos: SQL CRUD directo en seis repositorios; inexistencia de trigger demostrable para una regla disponible; errores internos sin traducción uniforme; dependencia del Event Log de Windows al registrar advertencias; ausencia de búsqueda local y estados de envío. La nueva normalización en trigger conserva la regla de recorte de espacios que ya aplicaba la aplicación.

La búsqueda de documentos oficiales no encontró metodología, VERSION_2, mapa de versiones ni esquema académico. El usuario indicó que no recuerda su ubicación. Se conserva el modelo real y no se crean tablas de grupos, semilleros o docentes a partir de nombres de referencia.

Nuevos archivos de implementación: `database/migrations/004_v2_catalogos_rutinas.sql`, `005_v2_area_aplicacion_trigger.sql`, `database/tests/v2_catalogos_rutinas.sql`, `MIRA.Api/Excepciones/ApiExceptionMiddleware.cs`, `tests/MIRA.Api.Tests/ErroresPersistenciaTests.cs`, `frontend/tests/test_architecture.py`, recursos `frontend/static/mira.*`, `frontend/templates/includes/catalog_toolbar.html`, `scripts/verify-browser.mjs`. Se actualizaron repositorios, servicio/formulario de área, base y seis listados, verificación local y Spec Kit existente. Prompts auténticos y capturas se conservan en `docs/prompts/` y `docs/evidencias/`.

## Primera auditoría (referencia histórica)

Fecha: 23 de octubre de 2026 (Colombia). Repositorio: `francis872/Mira`, rama `feature/mira-v1-v2-integracion`. Base de revisión: commit `00dc9da` y cambios de esta auditoría. Los resultados de ejecución están en [pruebas-v2.md](pruebas-v2.md).

## Alcance y método

El prompt recibido se refería a Backstage. El usuario confirmó adaptarlo a MIRA. No existen Backstage, SOI, CleanCom, organizaciones ni algoritmos geoespaciales en este repositorio. No se añadieron esas funcionalidades ni se cambió la identidad visual de MIRA. Autenticación y autorización permanecen fuera de V1/V2 según la constitución y el Spec Kit.

Se inspeccionaron la solución, modelos, controladores, servicios, repositorios, rutas Flask, plantillas, migraciones, pruebas, scripts de operación y especificaciones. Se ejecutaron compilación, pruebas con PostgreSQL real, pruebas HTTP completas y un reinicio con comprobación de persistencia. No se realizó una prueba visual con navegador ni se ejecutaron imágenes Docker.

## Inventario real

| Componente | Evidencia | Estado final |
|---|---|---|
| API .NET 10, DI, Controller → Service → Repository | `MIRA.Api/Program.cs`, `Peticiones/`, `Servicios/`, `Repositorios/` | Operativa y verificada localmente |
| Área de conocimiento | `AreaConocimientoController.cs`, `area_conocimiento_*` en `frontend/app.py` | Operativa y verificada |
| ODS | `ObjetivoDesarrolloSostenibleController.cs`, `ods_*` | Operativa y verificada |
| Área de aplicación | `AreaAplicacionController.cs`, `area_aplicacion_*` | Operativa y verificada |
| Término clave | `TerminoClaveController.cs`, `termino_clave_*` | Operativa en los casos probados; conflicto de clave inactiva corregido |
| Universidad | `UniversidadController.cs`, `universidad_*` | Operativa y verificada |
| Línea de investigación | `LineaInvestigacionController.cs`, `linea_investigacion_*` | Operativa y verificada |
| Dashboard | `index`, `_count`, `CATALOGS`, `frontend/templates/index.html` | Conteos reales; manejo de respuesta inválida corregido |
| Mensajes de validación y conflictos | `error_message`, rutas crear/editar en `frontend/app.py` | Implementados pero defectuosos al inicio; corregidos y verificados |
| Estado de error en listados | `list_items` y seis rutas de listado | Fallos HTTP antes silenciosos; ahora muestran aviso |
| Formularios, edición e inactivación | `frontend/templates/modulos/*`, pruebas HTTP | Conectados a API y PostgreSQL; modales/JavaScript sin prueba visual |
| SQL parametrizado y borrado lógico | seis repositorios; migración 001 | Operativos y verificados con datos reales |
| Rutinas usuario→roles, vista y JSONB | migración 003, `database/tests/v2_usuarios_roles.sql` | Operativas solo en PostgreSQL; sin integración HTTP/UI por alcance |
| Maestro-detalle académico, FK seleccionables y triggers | `versiones/v2_profesor/2_spec.md` | Bloqueados por ausencia del modelo oficial de Entrega 2 |
| Login y administración de usuarios | constitución, especificación V2 | Fuera de alcance; rutas ausentes verificadas |
| Arranque/parada local | `scripts/start-local.ps1`, `stop-local.ps1` | Operativos; datos conservados tras reinicio |
| Docker | `Dockerfile`, `frontend/Dockerfile`, `docker-compose.yml` | Configuración existente; ejecución bloqueada: CLI Docker no disponible |
| Spec Kit | `versiones/v0.1_api_inicial/`, `versiones/v2_profesor/` | Documentos SDD manuales; no se acredita instalación de GitHub Spec Kit |
| Documentos oficiales | `docs/fuentes/README.md` | Dependencia externa ausente |

Los datos de la migración 002 son semillas persistidas, no respuestas simuladas. Los dobles `FakeResponse` solo se utilizan en pruebas del frontend. El dashboard consulta la API; no contiene indicadores aleatorios. Varios documentos V1 todavía conservan plantillas históricas, registrados en pendientes.

## Defectos corregidos

| Prioridad | Hallazgo inicial y evidencia | Corrección y verificación |
|---|---|---|
| P1 | `requests.Response` es falso para 400/409. Las rutas perdían `mensaje`; el doble de pruebas no reproducía ese comportamiento. | Comparación explícita con `None`, uso de `error_message` y doble corregido. Pruebas Flask y mensajes reales en E2E. |
| P1 | `TerminoClaveService` solo consultaba activos: recrear una clave inactiva alcanzaba la restricción UNIQUE sin manejo; también existía una carrera entre consultas e inserciones. | El repositorio convierte SQLSTATE 23505 en conflicto controlado. Prueba de cuatro solicitudes concurrentes y recreación tras inactivación. |
| P1 | Ningún modelo limitaba textos a los VARCHAR de PostgreSQL. | `StringLength` en seis modelos; ASP.NET devuelve 400; el frontend interpreta `errors`. Pruebas POST/PUT y formulario real. |
| P2 | `_count` y listados suponían JSON válido; fallos HTTP podían parecer listas vacías sin explicación. | Validación estructural de respuestas exitosas y aviso en listados. Pruebas con respuesta ausente, diccionario y texto. |
| P2 | `hmac.compare_digest` sobre un token no ASCII provocaba `TypeError`. | Comparación de bytes UTF-8; 400 sin escritura. Pruebas Flask y HTTP real. |
| P2 | Dependencias Python declaradas distintas del entorno probado. | Versiones directas y Werkzeug alineadas con las instaladas y verificadas. No equivale a una auditoría de vulnerabilidades. |
| P2 | Compose publicaba servicios en todas las interfaces pese al alcance local sin autenticación. | Publicaciones limitadas a `127.0.0.1`; cambio inspeccionado, pendiente validación con Docker instalado. |

Al reproducir los defectos con el doble corregido y los nuevos casos hubo 18 fallos y 22 aciertos de Flask; después de corregirlos, 40 aciertos. No se sustituyeron operaciones de producción por mocks.

## Archivos de esta revisión

- Código: seis modelos de `MIRA.Api/Modelos/`, `MIRA.Api/Repositorios/TerminoClaveRepository.cs` y `frontend/app.py`.
- Verificación: `frontend/tests/test_app.py`, `frontend/tests/e2e/test_local_platform.py`, `tests/MIRA.Api.Tests/CatalogCrudRegressionTests.cs`, `database/tests/v2_usuarios_roles.sql` y nuevo `scripts/verify-local.ps1`.
- Entorno: `frontend/requirements.txt`, `frontend/requirements-dev.txt`, `docker-compose.yml`.
- Documentación: seis informes `docs/*-v2.md`, `docs/PROMPT.md`, `README.md`, `scripts/README.md`, especificación V1 y especificación/plan/contratos/quickstart/tareas del kit V2.

## Resultado de la auditoría

MIRA queda operativa localmente para los seis catálogos y los casos verificados. V2 académica completa sigue bloqueada por su modelo oficial. Despliegue Docker, accesibilidad visual completa, auditoría de dependencias y producción no están acreditados. Consultar [pendientes-v2.md](pendientes-v2.md) antes de ampliar el alcance o preparar una entrega.
