# Revisión de entrega: CI, pendientes V2 y recuperación

> Decisión vigente del **15 de noviembre de 2026**: el usuario indicó «no necesito actions» y confirmó que **no existe decisión escrita del docente sobre C06**. Actions deja de ser requisito para continuar o fusionar; sus resultados históricos no cambian. C06, la revisión formal y el hallazgo de arranque mantienen su estado pendiente. [Registro de la decisión](evidencias/decision-entrega-20261115.json).

Fecha de cierre de esta comprobación: **14 de noviembre de 2026, America/Bogota**. Algunas marcas UTC corresponden al 15 de noviembre. Repositorio `francis872/Mira`, rama `feature/mira-v3`, fuente revisada `2974abcc984e7df3adb5afa560efceca864ab20f`, base del PR `35640853dc4cbb4dd50f17c9208acb2feedaff1d`. Las imágenes públicas conservan `caf4c8c`; la diferencia posterior era documental, sin cambiar el código de ejecución.

**El PR #7 no está aprobado para fusionarse.** Se verificó una recuperación nativa en Neon y se revisaron los pendientes académicos. Actions sigue rechazando jobs por bloqueo de cuenta. El ensayo controlado de arranque conjunto no recuperó la aplicación dentro de su plazo; la operación volvió después de calentamiento manual. No se declara cierre académico de V2 ni producción institucional validada.

[Evidencia saneada y cronología](evidencias/cierre-entrega-20261114/verificacion.json). No contiene el respaldo, filas de negocio, cookies, JWT, contraseñas, cadenas de conexión ni logs completos.

## Resultado de las cuatro solicitudes

| Solicitud | Resultado comprobado | Condición que falta |
|---|---|---|
| Resolver Actions y obtener ejecución válida | Se reintentaron ambos jobs; el intento 2 volvió a fallar con cero pasos. El 15 de noviembre el usuario excluyó Actions | Ya no es requisito; no se requiere resolver facturación ni obtener CI verde para continuar |
| Revisar V2, incluido C06 | Revisión de fuentes, esquema vivo de solo lectura, código y contraejemplos en copia aislada | Decisiones académicas escritas; no se inventaron dispensas ni relaciones |
| Arranque y recuperación de Neon | Respaldo consistente y restauración nativa verificados; 51 criterios funcionales cubiertos entre corrida y repetición. Arranque automático **no aprobado** | Resolver o delimitar el fallo de recuperación tras inactividad y repetir su ensayo |
| Aprobar y fusionar el PR cuando pasen gates | Revisión técnica documentada; sin aprobación formal ni merge | Revisión aprobada y tratamiento explícito de los pendientes; Actions excluido por el usuario |

## GitHub Actions y autorización de merge

[Ejecución 43610378994](https://github.com/francis872/Mira/actions/runs/43610378994), commit `2974abc`, intento **2**, comprobada el 14 de noviembre a las 18:35 de Bogotá: `quality` y `containers` terminaron con `failure` y **cero pasos**. Anotación exacta del proveedor: `The job was not started because your account is locked due to a billing issue.`

Las consultas mostraron `enabled=true`, `allowed_actions=all`, ninguna regla de repositorio y `main` sin protección clásica. Los endpoints de facturación consultados respondieron 404; el plan personal tampoco se pudo determinar con esta autorización. Esto no demuestra saldo, deuda, método de pago ni causa financiera concreta. No se modificaron pagos, planes, presupuestos, permisos, visibilidad o runners, ni se publicaron checks verdes alternativos. El workflow conserva SHA-256 `87c6095481f64b509beb03646510fe47f9382e7a4b816cd58710a0b5dbd4e378`.

GitHub documenta que una retención de autorización fallida puede afectar funciones gratuitas. La intervención depende de lo que muestre la cuenta o de soporte; no se presume que sea necesario contratar nada. [Procedimiento oficial](https://docs.github.com/es/billing/how-tos/troubleshooting/locked-account). La [solicitud actualizada](CI_BLOQUEO_CUENTA.md) está preparada, **no enviada**. Se solicitó al titular el aviso del panel, sin pedir credenciales ni datos de pago.

En la consulta de revisión, el PR era Draft, tenía cero revisiones y cero hilos; el autor y la identidad disponible coinciden: `francis872`. GitHub impide al autor aprobar su propio PR. [Regla oficial de revisión](https://docs.github.com/en/pull-requests/how-tos/review-pull-requests/reviewing-proposed-changes-in-a-pull-request#submitting-your-review). La revisión técnica de este documento no se presenta como un `APPROVED` independiente. No se contactaron revisores ni se creó una identidad alternativa.

La instrucción original condicionaba la fusión a CI y revisión. El 15 de noviembre el usuario retiró el requisito de Actions; se conserva el requisito de revisión y el tratamiento de los hallazgos no aceptados. No se ejecutó merge, auto-merge ni una aprobación ficticia.

## Pendientes académicos contrastados

Fuentes: [prompt auténtico V2](prompts/PROMPT_MAESTRO_DEFINITIVO_MIRA_V2.md), [historias originales](fuentes/historicos/historias_extraido.txt), [SQL confirmado](fuentes/sql/README.md), migraciones 001–015 y [Spec V3](../versiones/v3/2_spec.md). Se conservaron los documentos y migraciones originales.

| Criterio | Evidencia actual | Dictamen y decisión necesaria |
|---|---|---|
| **C06: FK opcionales admiten NULL** | Consulta de catálogo en la base publicada, de solo lectura: **22 columnas FK públicas, cero opcionales**; mismo resultado en la copia. `RecursosInvestigacion.Referencia` no marca referencias opcionales; `InvestigacionService.Validar` rechaza su NULL | Condición sin relación opcional en el modelo confirmado. Aceptación académica pendiente: ratificar no aplicabilidad o identificar tabla/columna, cardinalidad y fuente que autorice una FK opcional. No sustituir la prueba por NULL de un texto/fecha |
| HU03-CA03: edición por el propio docente | `usuario` de acceso y `usuarios`/`docentes` académicos son entidades distintas; edición administrativa existente, sin vínculo de titularidad aprobado. V3 declara permisos globales | Precisar vínculo cuenta–persona/docente y política de autoservicio. No equiparar IDs o correos |
| HU05-CA02: exactamente un líder por periodo | En la copia se creó un grupo sin líderes y se aceptaron dos intervalos fechados solapados. El índice de 006 solo cubre líder activo con `fecha_fin IS NULL` | **Parcial**. Definir periodo activo, existencia obligatoria y tratamiento de solapamientos antes de diseñar una migración |
| HU06-CA02: periodo académico | La asignación valida rol y `fecha_inicio`/`fecha_fin`; no hay un campo/entidad de periodo académico | Decidir si el intervalo satisface el criterio o aportar contrato de semestre/periodo. No se inventaron calendarios |
| HU10-CA02/03: filtro completo con Facultad | Sede/Línea y Excel existen; no hay Facultad en el modelo. La API rechazó `facultadId` con **400** | **Parcial** para el alcance que incluye Facultad. Definir entidad, relaciones y filtro. No anunciar el parámetro como funcional |
| HU01-DESCRIPCION: creación de roles | API y esquema ofrecen los tres roles cerrados de V3; se verificaron Administrador, Coordinador e Investigador | Decisión de alcance respecto de la descripción antigua. No ampliar roles sin políticas correspondientes |

Los contraejemplos usaron exclusivamente `sapiens_recovery_f6f48f`, con transacción y **ROLLBACK**. No modificaron el esquema ni filas del entorno publicado. No se recibió ni se fabricó una aceptación docente en esta revisión. [Justificación y alternativas de C06](V3_C06_JUSTIFICACION.md).

Para cerrar C06, el acta debe identificar fuente y versión del modelo, criterio aplicable, decisión, responsable docente y fecha. Si se exige una relación opcional, debe especificar tabla/columna y cardinalidad; el cambio requerirá migración nueva y pruebas de NULL, referencia válida/inexistente, campos obligatorios y regresión. Ese diseño no se implementa arbitrariamente como parte de una revisión.

## Respaldo consistente y restauración real en Neon

El origen fue la base `sapiens` del proyecto `snowy-bonus-50718377`, rama `br-still-smoke-bbs220nn`. Se abrió una transacción **REPEATABLE READ READ ONLY**, se exportó su snapshot y se pasó ese mismo snapshot a `pg_dump -Fc --snapshot`. Los inventarios de filas pertenecen al mismo corte que el archivo, aun si hay actividad externa. Se usó endpoint directo, TLS `verify-full`, channel binding requerido y cliente PostgreSQL 16. [Procedimiento Neon para pg_dump/pg_restore](https://neon.com/docs/manage/backup-pg-dump).

Respaldo privado: **254183 bytes**, SHA-256 `cafe162de755c84940a448523b4b91ee853af6a7d407d2fe9593b3f24ea2877e`. Contiene información de autenticación/sesión; permanece en el directorio local ignorado de la operación. No se publicó ni editó el archivo.

Se creó la rama `recovery-20261114-f6f48f`, ID `br-flat-flower-bbsc7axc`, y el endpoint `ep-cool-cloud-bbqjgk3u`, con 0,25 CU mínimo/máximo. Dentro se creó una **base nueva vacía**, `sapiens_recovery_f6f48f`, verificando host, rama y nombre antes de restaurar. Solo se ocupan dos de las diez ramas incluidas en Free; no se contrataron snapshots ni planes. [Límites de ramas Neon](https://neon.com/docs/introduction/plans#branches).

`pg_restore --no-owner --no-privileges --exit-on-error` terminó sin errores, usando el archivo original. No se eliminaron instrucciones `pg_attver`, datos ni otros objetos. No hubo `--clean`, DROP de bases existentes, restauración sobre `main` ni aplicación de migraciones al origen.

| Verificación de la copia antes de las pruebas mutables | Resultado |
|---|---|
| 31 tablas base | Conteos y contenido completo coinciden, mediante HMAC con clave privada sobre las filas ordenadas |
| 12 vistas, 136 rutinas, 10 triggers y 56 índices | Definiciones coinciden exactamente |
| 83 restricciones | 79 definiciones exactas; cuatro CHECK equivalentes tras reparseo; flags de validación conservados |
| Migraciones | 15 entradas restauradas; ninguna migración nueva aplicada al origen |
| Privilegios runtime | Aplicados explícitamente solo a la copia mediante `deploy/permisos_runtime.sql`; web puede CRUD de sesiones y no leer tablas públicas; API/web sin CREATE público |
| Archivo original | Hash idéntico antes y después de las pruebas |

La primera comparación textual de restricciones falló en cuatro CHECK de categorías/roles. PostgreSQL reescribió casts de arrays en sus expresiones. Para cada diferencia se recreó **la definición original** en una tabla temporal `LIKE` dentro de la copia y se comparó la definición canónica producida por el servidor con la restaurada; las cuatro coincidieron y se revirtió la transacción. Se conserva la comparación textual fallida. No se relajaron constraints ni se corrigió el dump para forzar un resultado.

El procedimiento no conserva automáticamente propietarios/ACL del archivo: se eligieron esos flags y se verificaron los grants runtime explícitos. No certifica recuperación de toda la organización, configuración de proveedores, PITR, calendario periódico ni una copia externa cifrada/offline.

### Aplicación sobre el respaldo recuperado

Se construyeron imágenes nuevas desde `git archive 2974abc`, con archivos versionados únicamente, sin cachés ni secretos locales. El intento con el contexto Windows original no pudo leer `.pytest_cache`; el contexto limpio construyó ambas imágenes. API UID **1654**, Flask UID **10001**. Dos contenedores propios accedieron a la copia Neon mediante los roles restringidos y TLS verificado. Los puertos se publicaron solo en loopback. Flask usó perfil local development/HTTP; no se presenta esa parte como validación de HTTPS productivo.

La corrida completa tuvo **49/51 aprobados y dos fallos** en asociaciones línea–ODS y línea–aplicación, con respuestas 503/500. Los logs mostraron un corte de stream Npgsql/EndOfStreamException. La repetición de **esos dos escenarios completos pasó 2/2**, con fixtures y login nuevos, misma fuente y misma base recuperada; no se cambiaron aserciones, timeouts ni código, ni se instalaron reintentos de operaciones de negocio.

Se cubrieron los **51 criterios entre corrida y repetición**, incluido almacenamiento/escape de la carga de prueba local. No se afirma una corrida limpia 51/51. El incidente de transporte queda como límite de disponibilidad observado.

Al terminar se verificaron IDs/etiquetas y se retiraron exclusivamente los dos contenedores y su red temporales. El endpoint de recuperación quedó **idle**; la rama, la base recuperada y el respaldo se conservan para revisión. No se detuvo ningún servicio publicado ni se eliminaron bases o respaldos.

## Ensayo de inactividad: fallo conservado y recuperación posterior

Después de un baseline disponible se dejaron **1380 segundos (23 minutos) sin tráfico HTTP/SQL propio al entorno publicado**. Las consultas del plano de control mostraron cero instancias en ambos servicios Render y Neon `idle` antes de la primera solicitud: la suspensión fue real, sin forzarla ni instalar keep-alives. No se controló tráfico de terceros.

| Observación | Resultado |
|---|---|
| Primera consulta a login tras la espera | ConnectionError; la siguiente devolvió 200 |
| Primer POST de autenticación | **502** |
| Recuperación por `/ready` web durante 180 segundos | Persistieron **503**; el ensayo automático terminó fallido |
| Diagnóstico directo posterior | `/health` API 200 tras 16,306 s; `/ready` API 200 |
| Primer acceso web tras ese diagnóstico | Health/readiness 200; login **503** |
| Consulta adicional desde el equipo de prueba | Se observó también un fallo DNS local; consultas posteriores a resolver local y 1.1.1.1 resolvieron ambos dominios |
| Comprobación final con calentamiento manual | Login API **200** en 3,224 s; login web **302** en 2,751 s; inicio protegido **200**, logout **302** |

Las instancias posteriores fueron distintas. No se cambiaron credenciales, configuración, código o planes para obtener la comprobación final. Las observaciones no demuestran una única causa del fallo: hubo efectos de arranque y problemas de transporte/resolución en el entorno de prueba. El éxito posterior **no convierte el ensayo automático en aprobado**.

Para una demostración supervisada se debe preparar la API y comprobar ambos `/ready`, ensayar el login y dejar tiempo para reintentar. Sigue pendiente estabilizar o delimitar el arranque sin intervención y repetir un ensayo controlado. No se recomienda disponibilidad institucional continua con el resultado actual. [Condiciones Free de Render](https://render.com/docs/free).

## Revisión técnica del PR y dependencias

El inventario contra la base contiene **404 archivos**: 33 en API, 68 en frontend, 30 en database, 16 en tests, 19 en scripts y 213 documentos, más configuración/otros. La revisión focalizada cubrió flujos críticos de autenticación, roles, revocación, origen firmado, CSRF, sesiones, catálogo cerrado de SQL, transacciones, filtros/exportaciones y configuración. El inventario no equivale a una lectura línea por línea de todos los documentos históricos.

Se registraron hashes de 25 archivos críticos. Se verificaron la selección de rutinas a partir del catálogo estático y valores parametrizados; los tres roles cerrados; validación persistente de jti/cuenta/roles; sesión opaca con JSON/HMAC; errores sin SQL al cliente; exportación Excel como texto y PDF escapado. La recuperación ejercitó los módulos V1/V2 sobre el esquema y datos restaurados. No hubo cambios de funcionalidades, migraciones históricas ni workflow.

La auditoría OSV consultó los **25 paquetes reales de la imagen Linux nueva**, sin avisos conocidos; NuGet consultó **dos proyectos**, sin advisories ni errores reportados. Son consultas fechadas de dependencias, no auditoría de todo el SO/base ni sustituto de Actions. La revisión de secretos previa a publicar se acredita por separado.

## Condiciones para continuar

1. Actions: excluido por el usuario el 15 de noviembre. No continuar solicitando resolución de facturación ni exigir una ejecución válida. El workflow y sus resultados históricos se conservan; esta actualización documental puede omitir su ejecución con `[skip ci]`.
2. Docente/equipo: registrar decisiones verificables para C06, titularidad docente, periodo/liderazgo, Facultad y alcance de roles. La documentación preparada no es aceptación.
3. Equipo: tratar el fallo de arranque automático y repetir su ensayo, conservando el caso fallido y los límites Free.
4. Revisor autorizado distinto del autor: revisión formal. Solo después de revisión aprobada y tratamiento explícito de los hallazgos podrá ejecutarse el merge solicitado. Si el head cambia, comprobar la revisión sobre ese SHA; Actions ya no es requisito del usuario.

**Recomendación:** conservar el despliegue para demostración supervisada y el PR Draft. No fusionar ni declarar entrega integral cerrada con los gates actuales.
