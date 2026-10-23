# Pendientes y bloqueos de MIRA

Estado tras la auditoría del 23 de octubre de 2026. La plataforma local está activa y los casos documentados pasan; esta lista delimita lo que no se acredita.

Actualización por el prompt definitivo: los seis catálogos ya usan 30 funciones PostgreSQL y existe un trigger de normalización real. Navegación, búsqueda, alta/edición/retiro en modal y menú móvil se comprobaron en Edge. Siguen pendientes el modelo académico, las FK/puentes y maestro-detalle de dominio; ver [criterios-v2.md](criterios-v2.md): seis criterios verificados y seis bloqueados.

Última revisión: se recuperaron fuentes históricas auténticas desde Git y se verificó toda la plataforma actual sobre PostgreSQL 16.15 (41/42/31 pruebas, SQL y cierre de procesos). La anterior carencia del motor 16 está resuelta para pruebas locales. El usuario rechazó un modelo provisional y pidió usar solo el oficial; la migración histórica diferente no se adopta como sustituto.

| Prioridad | Pendiente | Motivo y siguiente paso |
|---|---|---|
| P1 | Modelo relacional oficial de la Entrega 2 | Se restauraron plan, historias y cronograma en `docs/fuentes/historicos/`, pero no el esquema vigente. Faltan entidades, PK/FK, cardinalidades, nulabilidad y reglas del profesor. |
| P1 | Maestro-detalle académico, repositorios de rutinas, selectores FK, vistas y triggers justificados | Dependen del modelo anterior. No sustituirlos por relaciones ficticias entre catálogos independientes. |
| P2 | Auditoría exhaustiva de accesibilidad y navegadores | Edge real ya verificó siete controles, incluidos modales y viewport 390px. Falta revisión completa por teclado, lector de pantalla y otros navegadores. Bootstrap/iconos dependen de CDN. |
| P2 | Docker y Python 3.11 | PostgreSQL 16.15 ya fue probado con la API y frontend reales, de forma independiente. CLI Docker ausente y Python 3.11 sin probar; falta validar esa combinación de imágenes. |
| P2 | Auditoría actual de dependencias y secretos históricos | Se verificaron versiones instaladas y que solo `.env.example` está versionado como archivo de entorno. No se ejecutó escaneo CVE ni del historial completo. No afirmar ausencia de vulnerabilidades. |
| P2 | Casos de frontera de claves textuales | Verificar términos con separadores de ruta, porcentajes y otros caracteres reservados. Las rutas actuales usan la clave en el segmento URL; los casos aprobados no cubren todo Unicode ni todo carácter reservado. |
| P2 | Completitud de documentación histórica V1 | La especificación V1 fue actualizada retrospectivamente; los demás archivos del kit inicial aún pueden contener plantillas. No presentarlos como requisitos oficiales aprobados. |
| P2 | Observabilidad para despliegue | Traducción uniforme de errores y logs seguros a consola implementados. `/health` sigue siendo liveness; faltan readiness y monitoreo para producción. |
| P3 | Pruebas de carga y uso de conteos agregados | Dashboard obtiene seis listados completos secuencialmente; medir volumen real antes de introducir optimizaciones. |
| P3 | Gestión de bases exclusivas de auditoría | `verify-local.ps1` deja una base por ejecución para inspección. Definir retención antes de acumular ejecuciones; no elimina bases automáticamente. |
| P1 | Estructura oficial de nueve documentos y rúbrica | Faltan `0_METODOLOGIA.md`, `VERSION_2.md`, mapa, módulos y nombres oficiales del kit. Se actualizan los siete archivos V2 existentes; no se inventan nombres oficiales ni prompts de integrantes. |

## Fuera del alcance vigente

Login, JWT, contraseñas, roles de aplicación, multiempresa y aislamiento organizacional requieren una futura versión y definición explícita. Las tablas y rutinas legadas se conservan; no son una pantalla de usuarios incompleta que deba activarse en V2. Geoespacial, Backstage, SOI y CleanCom no corresponden a este repositorio.

## Despliegue y producción

No se realizó despliegue ni se certifica producción. Esa etapa requiere definir control de acceso, HTTPS, secretos, copias y recuperación, monitoreo, servidor web de producción, exposición de red y validación de infraestructura. Los scripts actuales dejan servicios locales en loopback; no convierten el equipo en un servicio productivo.

## Estado de aceptación

Se alcanzaron compilación local, inicio reproducible en el entorno disponible, seis flujos CRUD con persistencia real, controles de servidor/formulario probados, trazabilidad y documentación de sustentación. La aceptación completa de V2 está bloqueada por el modelo oficial, y el despliegue Docker está sin verificar. Ningún bloqueo se marca como funcionalidad terminada.
