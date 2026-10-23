# Pendientes de MIRA

23 de octubre de 2026. El modelo SQL de `repair/v1-catalogos` fue confirmado por el usuario y ya está integrado. La plataforma local está operativa con los seis catálogos V1, once recursos académicos y cinco asociaciones. [Pruebas](pruebas-v2.md) · [Criterios](criterios-v2.md).

| Prioridad | Pendiente | Situación concreta |
|---|---|---|
| P1 | Aplicabilidad de C06 | Ninguna FK del SQL oficial admite null. No se cambia el modelo; falta confirmación de no aplicabilidad o un SQL oficial distinto |
| P1 | Documentación del curso | No se localizaron `VERSION_2.md`, rúbrica original, mapa ni los nombres de nueve documentos. Se conserva el kit existente y las fuentes auténticas |
| Datos | Personas institucionales para docentes | La plataforma habitual no contiene usuarios plurales. El selector explica el vacío. No hay CRUD de usuarios ni se inventan credenciales; cargar personas reales desde una fuente institucional es precondición de datos |
| P2 | Docker/Python 3.11 | PostgreSQL 16.15 con API/Flask sí pasó pruebas completas; no se ejecutaron imágenes Docker ni Python 3.11 |
| P2 | Accesibilidad y otros navegadores | Edge y 72 combinaciones de página/ancho comprobadas; faltan lector de pantalla, revisión completa por teclado y otros motores |
| P2 | Dependencias y secretos históricos | Sin escaneo CVE ni del historial completo. Configuración local y respaldo están ignorados por Git |
| P2 | Casos de frontera de término clave V1 | Falta cobertura exhaustiva de separadores URL, porcentajes y todo Unicode |
| P2 | Operación productiva | Hosting público, monitoreo/readiness, carga y recuperación productiva no están dentro de la actualización GitHub solicitada |
| P3 | Retención de bases de auditoría | Los verificadores conservan bases exclusivas para inspección; no eliminan datos automáticamente |

No siguen pendientes las pantallas V2, los desplegables FK, los puentes, el maestro-detalle ni PostgreSQL 16. No hay autenticación en V1/V2. Se conservaron las tablas singulares legadas de usuarios y sus datos, y las plurales del SQL confirmado; ninguna tiene CRUD de aplicación.
