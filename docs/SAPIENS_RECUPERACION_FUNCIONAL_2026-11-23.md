# Recuperación funcional de SAPIENS — 23 de noviembre de 2026

## A. Diagnóstico forense

Repositorio `francis872/Mira`, rama `feature/mira-v3`, referencia documental `0b15706`. La auditoría comenzó comprobando que **ambos servicios ejecutaban `250b46129ec46c9fa430e955441f33bfac9a07e9`**, anterior a ese commit documental. No se trabajó en BACKSTAGE. El PR #7 estaba abierto, Draft y sin revisiones formales.

No se comprobó una eliminación general del CRUD por Institutional Edition. La comparación `git diff da90f3a..250b461 -- frontend/investigacion.py frontend/reportes.py frontend/templates/investigacion MIRA.Api/Configuracion/SeguridadConfiguration.cs MIRA.Api/Servicios/InvestigacionService.cs database/migrations` no contiene diferencias. La cuenta institucional conserva Investigador, un rol de consulta. Los controles de escritura existían antes de esa edición.

Sí se reprodujeron defectos funcionales reales: tres filas de Sedes con tres celdas de acciones vacías para Investigador, ausencia de páginas independientes de detalle, ficha de grupo accesible solo desde edición, referencias descriptivas resueltas con opciones activas y navegación contextual incompleta. Una copia restaurada permitió además demostrar un retiro indebido: Coordinador omitió un semillero al guardar una ficha y la API respondió 200, dejando al hijo inactivo.

La sincronización de disciplinas/ODS de una línea presentaba la misma vía de retiro mediante PUT: la rutina de la migración 012 elimina los vínculos omitidos. La corrección comprueba esas omisiones dentro de la misma transacción de guardado. No se quitaron controles para recuperar botones.

## B. Diferencias entre versiones

| Referencia | Evidencia y alcance |
|---|---|
| `d41099a`, 2026-10-23 | Integración del modelo oficial y formularios de investigación; origen histórico del módulo. |
| `4265eba` / `f9f29f0`, noviembre | Controles de roles anteriores a Institutional Edition. |
| `de8dfc0`, 2026-11-12 | Ampliación de flujos y reportes. |
| `da90f3a` → `250b461` | Sin cambios en las rutas, plantillas, servicio, reglas SQL y política de investigación comparados. |
| `250b461` | Código realmente desplegado al iniciar la auditoría. |
| `0b15706` | Referencia documental, no versión de aplicación distinta en Render. |
| `bf846ca` | Lectura de grafo académico y protección transaccional de retiros indirectos. |
| `ce5a293` | Recuperación de consultas/navegación y verificador reproducible; commit candidato de publicación controlada. |

No hubo revert general, reconstrucción, migración nueva, cascada nueva ni cambio de autenticación. Parte del diff de Git corresponde a finales de línea; `git diff --ignore-cr-at-eol --stat 0b15706..ce5a293` muestra los cambios lógicos acotados.

## C. Recuperación CRUD

Se conservaron y comprobaron los seis catálogos V1: áreas de conocimiento, ODS, áreas de aplicación, términos clave, universidades y líneas del catálogo general. En investigación se comprobaron los once recursos: sedes, docentes, grupos, semilleros, líneas académicas, docentes por grupo, tutores, disciplinas, ODS de líneas, aplicaciones de líneas y palabras del modelo académico. Se revisaron también las cinco asociaciones: líneas de grupos, líneas de semilleros, disciplinas por línea, ODS por línea y aplicaciones por línea.

Todos disponen de consulta explícita **Ver detalles**, incluso para Investigador. Las acciones de escritura tienen etiquetas y contexto; editar conserva el retorno a la ficha o listado permitido. Se comprobaron creación y edición con Coordinador, consulta con Investigador y retiro de los registros propios de prueba con Administrador, desde Edge hasta API/PostgreSQL y de regreso a la interfaz. Las confirmaciones identifican el registro y conservan las reglas de dependencias existentes.

No se inventó PUT para puentes sin atributos editables ni CRUD de proyectos: no existen como entidad/controlador/ruta en el inventario actual. Cambiar un puente implica asignar el correcto y retirar el anterior con el rol autorizado. Las personas académicas de `usuarios` y las cuentas de acceso de `usuario` permanecen separadas; crear una cuenta no crea un docente automáticamente.

La [matriz de 205 funciones](SAPIENS_REGRESION_FUNCIONAL.md) detalla el estado previo, rol, causa, archivo, corrección, prueba y evidencia por operación. Distingue autorización correcta, recuperación, conservación y funciones no aplicables.

## D. Relaciones maestro-detalle

Las páginas de detalle muestran referencias y dependientes del esquema existente: sede → grupos; grupo → semilleros/integrantes/líneas; semillero → tutores/líneas; docente → participaciones; línea → disciplinas/ODS/aplicaciones y grupos/semilleros vinculados. El inventario de **22 FK de `public` incluye también las de autenticación y roles heredados**; no son 22 relaciones académicas nuevas. Las relaciones sin datos muestran un estado vacío explícito. Las referencias históricas conservan su nombre y estado; una referencia no disponible no se declara retirada sin prueba.

La consulta de detalle usa una única sentencia SQL para obtener una fotografía consistente. Los nombres de vistas/tablas/FK proceden de metadatos cerrados; los identificadores son parámetros. Las asociaciones emplean sus tablas reales, no vistas inexistentes. No se devuelven contraseñas, hashes ni correos de personas en los nuevos catálogos descriptivos.

`InvestigacionRepository.GuardarFicha` y `Actualizar` bloquean el maestro, comprueban los hijos/vínculos conservados y ejecutan el guardado sobre la misma conexión y transacción. Con Coordinador, una omisión de retiro responde **403 y no guarda cambios parciales**. Se verificaron edición permitida, retiro por Administrador, pertenencia de hijos y reversión ante un hijo de otro grupo. No cambió el contrato SQL de mínimos ni se añadió cascada.

## E. Datos técnicos visibles

La auditoría encontró **450 coincidencias de campos en 301 registros**, muchos inactivos. No significa que existan 301 registros cuya invalidez esté demostrada. No aparecieron coincidencias literales actuales para `browser133`, `E2S`, `bridging` ni `chip` en los campos examinados.

| Caso concreto | Procedencia comprobable | Tratamiento |
|---|---|---|
| `sedes.id=11`, nombre/código `BROWSER-1333e771`, ciudad `Prueba` | `prepare-browser.py` histórico genera `BROWSER-` + UUID de ocho caracteres y esos campos; el valor exacto está en el snapshot de importación privado `neon-populated-private.sql`. No se atribuye un operador sin registro de actor. | Conservado; advertencia de coincidencia documentada pendiente de validación. |
| `grupos_investigacion.id=9`, sede 11, URL `https://example.invalid/BROWSER-1333e771` | Misma preparación histórica crea grupo con sede recién generada y URL del marcador. FK y snapshot preservan su asociación. | Conservado y señalado; no sustituido por un nombre ficticio. |
| `lineas_investigacion.id=11`, código/nombre del mismo marcador, descripción `Descripción de prueba` | La preparación histórica utiliza exactamente ese conjunto de atributos. | Conservado y señalado. |
| `sedes.id=13`, `Sede actualización de navegador`, ciudad `Prueba`, código `BROWSER-1794534642830` | `scripts/verify-browser-v3.mjs` crea el nombre/ciudad y código `BROWSER-` + fecha numérica; no retira esa sede al terminar. | Conservado y señalado. |
| `objetivo_desarrollo_sostenible.id=9`, `e2e-f5-ods-a86c31e0f4` | Fixture de `frontend/tests/e2e/test_v1_requisitos.py`: `e2e-f5-ods-` + UUID de diez caracteres. Coincidencia de patrón, sin atribuir actor. | Conservado, con aviso de validación pendiente. |
| Personas académicas 1 y 2 | Sus nombres originales ya indican prueba automatizada/QA remota. No existe en este examen un registro de actor suficiente para certificar quién las creó. | Sin modificar; correos/hashes privados omitidos del informe publicable. |

`sedes.created_at` es `timestamp without time zone`: los valores de las sedes 11 y 13 son `2026-11-13T01:49:10.018081` y `2026-11-13T01:50:43.076637`. No se les asigna una zona horaria ni se confunden con la fecha de ejecución del script original. En tablas sin columna de creación no se inventa una fecha.

El [inventario saneado](evidencias/functional-20261123/procedencia-datos.json) identifica cada coincidencia por tabla, clave y columna, fecha disponible, estado, clasificación y dependencias entrantes activas/totales. Incluye la definición de las FK. Las pruebas de similitud se distinguen de la evidencia exacta; no se afirma que el artefacto de diagnóstico del navegador contenga el marcador cuando no lo contiene.

**No se ejecutó depuración.** Procedimiento propuesto: aprobación institucional de IDs concretos y motivo; respaldo compatible verificado; revisión de referencias en ambos sentidos y retención académica; comparación de estado/fingerprint inmediatamente antes de operar; transacción con bloqueos; uso de los retiros existentes con Administrador; comprobación posterior y evidencia. Ningún borrado general por palabras, cambio inventado de nombres ni eliminación nueva en cascada. La sede 11 no debe tratarse de forma aislada de su grupo 9 y sus vínculos; el inventario permite revisar esas dependencias antes de decidir.

## F. Reportes

Se conservaron los dos reportes reales: articulación académica e integrantes por grupo. Sus joins ya mostraban sede, grupo, semillero, docente, línea y ODS antes de Institutional Edition; no se atribuye al rediseño una desaparición de esos joins. Se integraron enlaces a fichas y búsqueda/paginación coherentes.

Las exportaciones usan nombres/encabezados, no claves como sustituto del nombre. Excel de articulación respeta sede/línea y la búsqueda visible; Excel/PDF de integrantes respetan la búsqueda del grupo. Se exportan todos los resultados filtrados, no solo la página visible. Se verifican celdas, encabezados, nombres y firma/estructura de PDF; se conserva la protección de fórmulas de Excel. Los valores originales de prueba permanecen identificados como tales cuando la procedencia se documenta; una presentación legible no los convierte en registros académicos validados.

## G. Roles y seguridad

| Rol | Consultar detalles/relaciones/reportes | Crear y editar | Retirar |
|---|---|---|---|
| Investigador | Sí | No | No |
| Coordinador | Sí | Sí, conservando vínculos cuyo retiro no tiene autorizado | No |
| Administrador | Sí | Sí | Sí, sujeto a integridad |

La cuenta `sapiens@gmail.com` sigue activa y con **Investigador**. Si el profesor debe gestionar datos, un Administrador puede asignarle Coordinador desde Cuentas de acceso conforme a la decisión institucional. No se realizó esa elevación. Los cambios de correo/roles y los restablecimientos revocan sesiones según el contrato existente.

Se mantuvieron bcrypt, JWT, consulta de roles, validación de sesión compartida, CSRF y registro público cerrado. La autorización Flask se vincula al endpoint y al verbo: una clave legítima que contenga `editar` no bloquea una consulta; HEAD es lectura. Las decisiones de retiro proceden del usuario autenticado en ASP.NET, nunca de un booleano enviado por el formulario.

La revisión de secretos distingue credenciales reales de un marcador académico explícito que no pasa la validación bcrypt. No se versionan respaldos, configuraciones privadas, cookies, JWT, DSN ni snapshots con datos personales. El escaneo no certifica todos los objetos históricos Git ni la seguridad completa del sistema operativo.

## H. Interfaz

Se conservaron la paleta blanco/negro/grises, tipografía, navegación oscura, login y diseño aprobado. Se inspeccionaron críticamente una ficha de grupo con relaciones y un detalle móvil antes de seleccionar capturas finales. Las tablas conservan desplazamiento propio; la página no desborda en las muestras verificadas.

Se comprobaron **35 combinaciones** de siete vistas × cinco anchos: 320, 390, 768, 1024 y 1440 px. Edge también se probó con **zoom real de 200 %**, ventana externa de 1440 px, viewport CSS de 708 px y DPR 2, sin emular zoom mediante un viewport más pequeño. Cuatro rutas pasaron axe sin incidencias. Se corrigió el contraste transitorio del enlace activo eliminando su transición de color.

Las [capturas de la copia aislada](evidencias/functional-20261123/) contienen datos `RECUPERACION-…` y cuentas `@invalid.test`; no representan altas institucionales ni datos añadidos a producción. No se afirma validación de dispositivos físicos, Safari/iOS o lector de pantalla.

## I. Pruebas y reproducción

| Comprobación | Resultado real |
|---|---|
| .NET con PostgreSQL restaurado | **157 aprobadas, cero omitidas**. |
| Flask con PostgreSQL de sesiones | **214 aprobadas, cero omitidas**. |
| Regresión HTTP V1/V2/V3 | **51 aprobadas, cero omitidas**. |
| Edge → Flask → API → PostgreSQL → UI | **69 recorridos aprobados**, repetidos con el verificador entregado. |
| Visual | 35 combinaciones; zoom real 200 %; cuatro rutas axe sin incidencias. |
| Dependencias Python | Consulta OSV de los 25 paquetes fijados; consultar fecha y resultado en la evidencia. |

Los primeros intentos fallidos se conservaron privadamente: retorno incorrecto después de editar catálogo, consulta a una vista de puente inexistente, selector de prueba que suponía nombres únicos de fixtures, espera prematura del foco del diálogo y contraste de la transición. Se corrigió el código o el instrumento según la causa y se repitió el recorrido completo. Una pasada Flask con configuración PostgreSQL encontró permisos insuficientes en un temporal anterior de Windows; usar un directorio temporal nuevo y propio permitió completar las 214 pruebas. No se presentan esos intentos como aprobados.

El [verificador reproducible](../scripts/functional-recovery/README.md) está versionado. Rechaza PostgreSQL remoto y nombres de base fuera de `mira_audit_functional_*`, crea una persona académica y fixtures solo en la copia, usa cuentas locales con los tres roles y comprueba cada guardado/retiro mediante una conexión independiente. Las credenciales se leen de un JSON privado ignorado por Git. No acepta una URL de producción ni despliega servicios. Los archivos de prueba locales se preservan para auditoría; no se trasladan a Neon.

## J. Base de datos

Se comprobó el respaldo compatible de PostgreSQL 16.16: SHA-256 `80ce3aa07d4576a1bfbb46a8cd67f8f5e7617dacae55cdf9611c8241cdd4bb18`. La restauración inicial coincidió con **25 tablas y 511 registros académicos**, antes de insertar los fixtures locales. No se editó el dump para forzar una restauración. La base usa las quince migraciones existentes; no hay migración 016 de esta intervención.

La comprobación remota compara cantidad y fingerprint del contenido completo de cada tabla académica, migraciones, identidad/hash/estado/roles institucionales y configuración de ambos servicios. Las autenticaciones crean/consultan sus sesiones normales; no se confunden esas operaciones con escrituras de prueba sobre datos académicos. Se revisaron los permisos SELECT ya existentes de `mira_api` para las nuevas consultas; no se concedieron permisos nuevos.

## K. Despliegue

La publicación controlada se realiza exclusivamente sobre la API `srv-dbr6i3dckfvc73dc3hr0` y el frontend oficial `srv-dbtp70tckfvc7393jn5g`. La URL permanece **https://sapiens-usb.onrender.com/login**. No se crean servicios, dominio o base, no se cambia ningún valor de entorno, y no se publica el frontend antiguo paralelo.

**Publicado y comprobado:** ambos servicios están `live` con **`ce5a293e762fa244999d7dee511e24cb93b8ef71`**. API: `dep-dc2b0td6e07s73c14qjg`; frontend: `dep-dc2b3brrjlhs73ad1hi0`. Se desplegó y verificó primero la API, antes de iniciar la web. El fallo transitorio de DNS durante una consulta de estado se resolvió retomando el mismo ID; no se duplicó el despliegue.

La pasada remota aprobó **13 comprobaciones de API** y después **41 comprobaciones finales**: listados/detalles de los once recursos, los seis catálogos y cinco asociaciones; reportes, permisos y registro cerrado. Las tablas sin filas se distinguen explícitamente de un detalle probado. La sesión web y el JWT institucional anteriores al reemplazo siguieron funcionando. Las 25 tablas/511 filas, fingerprints, migraciones, identidad/hash/roles y todos los valores de entorno coincidieron antes/después. No se realizó CRUD académico sobre producción ni se restableció la contraseña.

Edge comprobó también la navegación pública con la sesión institucional: Investigación → Sedes → Ver detalles → grupo asociado → relaciones y reportes. Las tres filas de Sedes presentan «Ver detalles», sin botones de escritura; el aviso de datos pendientes y las relaciones reales se muestran correctamente. Cinco anchos públicos adicionales no desbordan la página; cero excepciones JavaScript. Esto acredita consulta pública; el CRUD completo permanece probado únicamente en la copia aislada.

Recuperación preparada: desplegar el commit anterior **`250b46129ec46c9fa430e955441f33bfac9a07e9`** sobre el mismo servicio si falla una comprobación crítica. Mantener registro cerrado, configuración y Neon. No restaurar la base de producción para revertir una corrección de interfaz. El respaldo restaurado y los despliegues anteriores se conservan; no se ejecutó un rollback innecesario.

## L. Pendientes y recomendación

- Validación institucional y aprobación de cualquier depuración; no se ha autorizado ni ejecutado ninguna.
- Decisión docente escrita de C06 y demás decisiones V2 pendientes: esta corrección no cambia su aplicabilidad.
- Revisión formal de PR #7: permanece Draft y sin merge. Actions sigue excluido por instrucción del usuario; no se etiqueta un bloqueo histórico de facturación como ejecución aprobada.
- Revisión humana con lector de pantalla, dispositivos físicos, Safari/iOS, teclado virtual y prueba institucional de carga/disponibilidad. No se repitió aquí el ensayo completo de suspensión de quince minutos de Render Free.
- El CRUD completo se verificó en la copia aislada; en producción se verificó consulta/navegación y preservación de datos, sin reproducir escrituras académicas de prueba.

**Recomendación:** la recuperación funcional está publicada y verificada en su alcance: CRUD completo en copia aislada y consultas/continuidad/integridad en producción. Está preparada para revisión funcional y sustentación de esta corrección. La aceptación académica integral, depuración institucional y certificación de disponibilidad conservan los pendientes indicados; no se declara un cierre académico sin esas decisiones.
