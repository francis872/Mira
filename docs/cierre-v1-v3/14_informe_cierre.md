# SAPIENS — informe de cierre V1–V3 y preparación de publicación

Actualización de Fase 6, 12 de noviembre de 2026. El trabajo se realizó exclusivamente en MIRA. [Estado de despliegue](../FASE6_DESPLIEGUE.md), [resultados actuales](evidencias/fase6-20261112/resultados.json), [matriz de 104 registros](02_matriz_trazabilidad.md).

| Versión | Dictamen técnico y académico |
|---|---|
| V1 | **CERRADA TÉCNICAMENTE en el alcance local probado.** Inglés obligatorio de contenido y tres categorías ODS aplicados en interfaz/API/SQL; seis catálogos y regresión aprobados. No implica aceptación docente ni producción validada. |
| V2 | **NO CERRADA ACADÉMICAMENTE.** Se corrigieron 16 de los 22 criterios originales incompletos del inventario y se probaron códigos, inglés, descripción, mínimos, estados, articulaciones y reportes. Siguen pendientes perfil propio cuenta–docente, exactamente un líder por periodo, periodo académico y Facultad/filtros completos; C06 requiere decisión docente. |
| V3 | **Seguridad validada localmente y adaptación serverless probada; publicación/operación remota por comprobar.** bcrypt, JWT, roles, revocación, CSRF y D01 preservados. Sesiones compartidas, ingreso Vercel y readiness añadidos sin JWT en cookies. No se afirma Actions verde ni producción validada antes de resultados remotos. |

La Fase 5 dejó implementaciones y pruebas nuevas, pero este informe aún describía la auditoría previa. Se conserva el [informe anterior](evidencias/fase5-20261107/informe-pre-fase5.md), su matriz y hashes. La consolidación posterior distingue las pruebas del 7 de noviembre de las nuevas del 12; no se reciclan resultados.

## A. Identidad e integridad

Repositorio francis872/Mira, rama feature/mira-v3, HEAD inicial 3105273af84e55a1e813e76f2acb92acc31caa94; 165 candidatos iniciales de esta fase y 355 archivos de origen/configuración respaldados. ZIP independiente 1009d5415cb79deac5ecceaadd674001ff5c2351ed274a5c88c6b8ca36c454de y bundle Git 5222fb63a878771549c845f7a6b2209d883cc1db5c03a59b969e0360eba90120 verificados. Las migraciones históricas 001–008 y la corrección forwarding del workflow conservan hashes. [Preservación](evidencias/fase6-20261112/preservacion.json).

## B. Requisitos V1

HU02-CA03 exige español/inglés en palabras clave, sin ordenar traducir toda la interfaz. Modelo, servicios, formularios y 009 rechazan inglés NULL/vacío; V2 plural también incorpora ese campo. HU02-CA02 fija Social, Económica y Ambiental: selector, servicios y CHECK SQL lo hacen cumplir. 009 aborta antes de cambiar datos incompatibles; solo normaliza el rótulo histórico Económico→Económica.

## C. Requisitos V2

010 añade código sede/inglés plural/descripción obligatorios, protege nuevas asignaciones a sedes/líneas inactivas, lote múltiple atómico, resúmenes históricos y filtros semillero por grupo/tutor. 011 implementa correspondencias Grupo/Semillero–Docente–Línea–ODS, filtros Sede/Línea, Excel y miembros HTML/Excel/PDF. 012 guarda línea/disciplinas/ODS de una vez y comprueba mínimos al commit; bloqueos del padre evitan una doble retirada concurrente inválida. 013 permite reactivación Administrador sin perder puentes; 014 completa los 17 ODS plurales exigidos expresamente por HU09-CA02. Fuente nominal: [PNUD](https://www.undp.org/es/sustainable-development-goals).

La interfaz actualiza sedes abiertas, confirma retiros con ciclo Tab/Escape, conserva selección/error y ofrece resúmenes legibles. Las exportaciones comparan datos/filtros reales, rechazan texto XML no admisible, preservan fórmulas aparentes como texto y manejan vacíos/errores. No existe filtro Facultad: se rechaza su parámetro en vez de ignorarlo.

## D. C06

22 columnas FK públicas y ninguna opcional; no se alteró nulabilidad. Consulta pendiente: «¿C06 no aplica al modelo confirmado o qué relación concreta debe ser opcional? Indicar tabla/columna, cardinalidad, fuente y aceptación escrita». No hay excepción docente aceptada.

## E. Persistencia y recuperación

001–008 intactas; 009–015 nuevas. La actualización representativa desde V3 y restauración independiente aprobaron. Los CHECK NOT VALID de 010 conservan datos heredados faltantes; completar con valores aprobados y VALIDATE antes de aceptar una actualización institucional. No se inventan traducciones/códigos/vínculos. 015 separa sesiones web de cuentas/personas y no añade FK académicas.

Se creó un proyecto Neon gratuito nuevo; se verificó esquema vacío y TLS del cliente, se respaldó y restauró antes de aplicar 001–015 atómicamente. Se comprobaron los roles web/API sin privilegios de DDL ni lectura cruzada. No se modificaron bases existentes. [Evidencia remota de base](evidencias/fase6-20261112/neon-migraciones.json).

## F. Seguridad V3

AuthService/PasswordService siguen generando/verificando bcrypt12 y JWT HS256; CuentaRepository revalida jti, cuenta y roles. La clave forwarding efímera del workflow permanece intacta. El nuevo almacén compartido conserva SID opaco, firma del contenido y fallo cerrado ante caída, sin pickle ni JWT en cookies. D01 conserva el protocolo firmado; el ingreso Vercel solo se habilita por configuración del servidor y rechaza origen inválido.

## G. Calidad

138 .NET, 132 Flask, 51 HTTP, 11 SQL y repetición de 51 HTTP en Docker/Gunicorn con sesiones PostgreSQL: cero fallos/omisiones. 110 combinaciones responsive; 38 revisiones axe sin infracciones, con dos incomplete de contraste en textarea descripción; 10 grupos de teclado más cinco académicos; 51 comprobaciones de zoom real 100/200/400%. El verificador espera el cierre de su propio navegador, sin matar procesos ajenos. Se corrigió una carrera de health de PostgreSQL temporal; los intentos fallidos se conservan.

OSV/NuGet sin avisos reportados en nuevas consultas. No se certifica WCAG, lector de pantalla, dispositivos físicos ni todo el SO/base. [Casos y hashes](evidencias/fase6-20261112/casos.json), [comandos](evidencias/fase6-20261112/comandos.json).

## H. Documentación

Matriz de 104 registros/36 criterios originales con estado anterior/posterior, archivos, método, prueba, evidencia y límites. Se actualizaron manuales, recorrido de diez minutos y riesgos. Los originales y cortes históricos se conservan; no se reconstruyen documentos académicos faltantes.

## I. Control de cambios

La autorización de Fase 6 permite commits/push y despliegues no destructivos después de verificar pruebas y secretos. Se separan API, frontend, pruebas, CI, infraestructura y documentación; sin force push ni reescritura de historia. [Inventario final](evidencias/fase6-20261112/control-final.json). BACKSTAGE no se accedió ni modificó; no se certifica su árbol frente a acciones de terceros.

## J. Dictamen

V1 cumple el alcance técnico verificado. V2 tiene operaciones implementadas funcionales pero no cierre académico integral. V3 conserva seguridad local y adaptación para almacenamiento compartido; su operación remota se declarará exclusivamente con URL y pruebas reales. No se acepta C06 por inferencia ni se confunde una base remota migrada con toda la plataforma validada.

## K. Pendientes y riesgos

Definir Facultad, cuenta–docente y periodos/líder; resolver C06 y aceptación de los tres roles cerrados. Revisar dos incomplete axe, lector de pantalla y dispositivos físicos. Límites gratuitos/cold starts/cuotas, carga, patches del SO y recuperación operativa periódica siguen siendo riesgos reales. No se contrata pago para resolverlos sin autorización.

## L. Publicación y siguiente paso

Completar revisión de secretos y commits autorizados, publicar feature/mira-v3, inspeccionar Actions y actualizar PR #7. Desplegar API en Render Free y Flask en Vercel con Neon compartido; validar Preview antes de Production. Solo registrar resultados efectivos. No iniciar V4 ni declarar aceptación académica/producción por el mero hecho de publicar.
