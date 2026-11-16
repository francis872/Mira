# SAPIENS — Plataforma de gestión académica e investigación

**Base tecnológica y evolución académica: MIRA V1–V3**

Informe maestro del 7 de noviembre de 2026, America/Bogota. **La plataforma se puede demostrar localmente; el cierre integral de los requisitos originales todavía no está completo.** Este encargo añadió documentación y evidencias, conservó el código y no publicó cambios. No se inició V4.

## Dictámenes separados

| Versión | Dictamen actual | Fundamento y alcance |
|---|---|---|
| SAPIENS V1 | **NO CERRADA** | Los seis CRUD, persistencia, arquitectura y regresiones del Spec de trabajo aprueban. El original exige dominio cerrado de categoría ODS e inglés obligatorio; el código permite categoría libre e inglés NULL, verificado con contraejemplos. Falta resolver esas diferencias, no más documentación para ocultarlas. |
| SAPIENS V2 | **NO CERRADA** | Once recursos, cinco puentes, ficha atómica, triggers y SQL confirmado funcionan. Siguen pendientes condiciones originales: código sede, impedir nuevas asignaciones inactivas, líder exactamente uno por periodo, descripción/mínimos de línea y capacidades de UI/reportes/exportación. C06 además necesita aceptación docente. |
| SAPIENS V3 | **NO CERRADA** | Autenticación/autorización/revocación y D01 se verificaron, junto con regresión V1/V2. El cierre de publicación tiene un bloqueo técnico reproducido: workflow quality omite la clave forwarding requerida por Compose. Debe autorizarse y probarse esa corrección. R27 conserva revisión manual pendiente y la descripción histórica de creación de roles necesita decisión de alcance. No se afirma CI o producción validados. |

Estos dictámenes evalúan la entrega integral exigida, no niegan que los módulos existentes funcionen. V3 seguridad puede sustentarse técnicamente en el alcance local verificado; eso no transforma un gate de publicación fallido en una excepción ya aceptada. “CIERRE CONDICIONADO” solo corresponderá cuando la implementación y las pruebas obligatorias estén resueltas y quede una excepción/aprobación externa explícita. “CERRADA TÉCNICAMENTE” exige todos sus criterios verificados; la aceptación docente seguirá siendo distinta.

## Documentación consolidada y entregables

1. [Arquitectura y responsabilidades](01_arquitectura.md).
2. [Matriz de 104 registros con criterios originales](02_matriz_trazabilidad.md), [CSV](02_matriz_trazabilidad.csv), [JSON](02_matriz_trazabilidad.json).
3. [Endpoints, contratos, archivo/método](03_endpoints.md).
4. [Modelo de datos y ER](04_modelo_datos.md).
5. [Funciones/procedimientos/triggers/transacciones](05_rutinas_triggers.md).
6. [Autenticación/autorización y D01](06_autenticacion_autorizacion.md).
7. [Instalación y paquete de revisión](07_instalacion_local.md).
8. [Manual del usuario](08_manual_usuario.md).
9. [Manual técnico, configuración Render/Neon y propuesta CI](09_manual_tecnico.md).
10. [Pruebas actuales, comandos, entorno y pendientes manuales](10_pruebas.md).
11. [Sustentación diez minutos, escenarios y respuestas técnicas](11_guia_sustentacion.md).
12. [Defectos, riesgos y excepciones](12_riesgos_excepciones.md).
13. [Historial, preservación y commits propuestos](13_historial_publicacion.md).
14. Este informe y los [controles finales](evidencias/control-final.json).

## Requisitos verificados y pruebas nuevas de este corte

Los flujos del alcance implementado conviven en una sola plataforma: seis catálogos V1, once recursos y cinco asociaciones V2, ficha grupo→semilleros atómica, validaciones/errores, migraciones aditivas y dos triggers; V3 añade bcrypt12, JWT HS256 validado, cuentas activas/roles/jti persistentes, revocación, CSRF, sesión opaca, permisos y límites de login por origen/cuenta.

Nueva ejecución sobre HEAD 3105273 + árbol local identificado: **90 .NET, 98 Flask, 43 HTTP y cinco suites SQL aprobadas; los mismos 43 HTTP repetidos en imágenes nuevas Linux/Gunicorn**. 50 combinaciones responsive, 18 revisiones axe sin infracciones/incomplete,10 grupos de teclado y 27 comprobaciones de zoom aprobadas. No se suman las repeticiones como casos diferentes. Compilación sin errores ni advertencias. El incremento frente a Fase 2 proviene de 19 .NET y 18 Flask ya añadidos por D01, no de tests nuevos inventados en este encargo. [Resultados y hashes de reportes crudos](evidencias/resultados.json), [nombres de los casos](evidencias/casos.json).

D01 se inspeccionó contra su diff local y se reprodujo corregido: origen agotado429, origen distinto302 y falsificación sin evasión de cuota. Logout con API caída: sesión local borrada y advertencia; el mismo JWT siguió200 al volver API, luego401 tras revocación confirmada. Este comportamiento degradado se documenta como riesgo residual, no como revocación exitosa.

## Requisitos pendientes y evidencia de las diferencias

Se recuperaron 10 historias y 36 criterios del DOCX original, además del plan/cronograma, el SQL confirmado y los Spec de trabajo. Los tres binarios originales coinciden con sus blobs Git; no se crearon documentos académicos faltantes ni se asumió que el cronograma acredita trabajo terminado. La matriz de 104 registros relaciona requisito→versión→archivo→implementación→prueba→evidencia→estado, con siete estados permitidos y límites por fila. Incluye fuentes solapadas y no pretende 104 requisitos independientes.

Los contraejemplos en una base aislada con ROLLBACK confirman diferencias reales. También se inspeccionaron todos los controladores, el catálogo cerrado de recursos, rutas Flask, plantillas y esquema para determinar funciones ausentes. No se buscaron únicamente nombres de marca o archivos. Inglés/ODS, código sede, autoservicio docente, temporalidad de líder, presentación de asociaciones, mínimos de línea y reportes/exportaciones necesitan una decisión de alcance o implementación posterior autorizada. La frase de tutores sobre mismo intervalo sí se satisface para duplicado exacto; intervalos distintos superpuestos quedan como aclaración, sin inventar un requisito de exclusión temporal.

C06: 22 columnas FK públicas y 0 opcionales otra vez. No se encontró nueva decisión docente en las fuentes/repositorio/adjuntos revisados. La condición del criterio no tiene relación opcional a la que aplicarse; académicamente sigue pendiente. No se cambió nulabilidad para simular cumplimiento.

## Seguridad, dependencias y despliegue

[Revisión final de candidatos y exclusiones](evidencias/seguridad.json). No se detectaron secretos reales por los controles descritos; fixtures son de prueba y los archivos privados siguen fuera. El alcance es heurístico sobre el árbol actual y contenido /app de las imágenes, no una certificación de todo el historial, todos los metadatos o cuentas cloud.

OSV consultó 18 distribuciones de la nueva imagen Python sin avisos reportados; NuGet ambos proyectos y transitivos sin paquetes vulnerables reportados. Conectividad disponible en este corte; B01 DNS anterior no se reprodujo. El build inicial reutilizó caché; la verificación final reconstruyó ambas imágenes con --no-cache --pull y repitió integración/sesiones/smoke. Se verificó descarga de dependencias a esta fecha; no se acredita reconstrucción offline o disponibilidad futura de registries. No se modificó la red global ni se borraron recursos ajenos. CVE del SO/base permanece fuera de ese resultado.

Producción **no validada**. Se inspeccionaron Render/Neon, secrets externos, PORT, TLS exigido, disco y proxy. El smoke Production fue local con example.invalid, sin contactar destinos reales. Falta probar TLS/certificados/proxies/Neon/permisos/pooler/redeploy/restauración/carga. No se modificaron credenciales ni bases remotas, no se creó infraestructura y no hubo despliegue. `/health` 200 no demuestra disponibilidad de BD.

## Integridad y publicación

Raíz/remote/rama correctos: MIRA, https://github.com/francis872/Mira.git, feature/mira-v3; HEAD 3105273af84e55a1e813e76f2acb92acc31caa94 sin cambiar. Los 64 pendientes iniciales y todos los archivos previamente versionados conservan sus hashes. Solo se agregaron archivos nuevos dentro de docs/cierre-v1-v3 y auditoría privada ignorada. Respaldos Fase 2/Fase 3/pre-documentos verificados por SHA256 y CRC. Históricos de evidencias intactos. [Inventario y hashes](evidencias/inventario.json), [plan por responsabilidad](evidencias/plan_commits.json), [control final](evidencias/control-final.json).

No se escribió en BACKSTAGE. Su HEAD ce18b6d7b79867d940e1689d61755446acbf974c y SHA256 del READINESS_REPORT conocido permanecen coincidentes con la auditoría anterior; es un cotejo limitado de huellas, no una nueva garantía exhaustiva de todo su árbol. Todas las pruebas usaron instancias/bases/usuarios ficticios exclusivos. No se alteraron datos institucionales. Los ROLLBACK diagnósticos pueden avanzar secuencias solo en esas bases descartables.

PR #7 continúa abierto/Draft/sin merge; head remoto 3105273 y cero runs Actions para ese SHA. No se hicieron commits, staging, push, merge, cambio de rama ni cambios al PR. El ZIP local es material de revisión, **no publicación en GitHub**. Los cuatro grupos de publicación previos se preservan y se propone separar D01 y la consolidación nueva; no se ejecuta el plan. El workflow actual tiene CI01 pendiente, y la propuesta de una línea permanece sin aplicar.

## Recomendación y autorización necesaria

**Preparada para una sustentación local honesta con evidencias; no preparada para declararse entrega integral cerrada ni producción validada.** Resolver por escrito las diferencias entre historias originales y SQL/Spec confirmado; definir si se aceptan excepciones o se autorizan correcciones. Obtener decisión C06, autorizar el arreglo CI01 y repetir su gate; completar o aceptar expresamente los límites manuales. Solo después revisar commits/publicación y aceptación docente.

No iniciar SAPIENS V4 todavía ni asignarle automáticamente pendientes originales. Solicitar al propietario autorización separada para: (1) resolver alcance/correcciones históricas, (2) aplicar la fixture CI y verificarla, (3) crear/publicar los grupos de commits que apruebe. Merge, Ready for Review y despliegue requieren su decisión específica. Este informe no ejecuta ninguna de esas acciones.
