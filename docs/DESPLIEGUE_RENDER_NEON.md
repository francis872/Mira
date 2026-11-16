# SAPIENS: despliegue completo en Render y Neon

## Acceso vigente: registro propio y diseño oscuro — 16 de noviembre de 2026

**[Abrir SAPIENS: sapiens-usb.onrender.com](https://sapiens-usb.onrender.com)**. Es un subdominio gratuito del proveedor; no se compró ni se afirma propiedad de un dominio raíz. Se creó el frontend `srv-dbtp70tckfvc7393jn5g` en el mismo espacio de Render, plan **Free**, región Virginia, Docker y una instancia. El cambio de nombre del frontend anterior no alteró su URL: se restauró su nombre y se conservó actualizado como respaldo.

| Componente | Despliegue confirmado live | Commit de código |
|---|---|---|
| Interfaz nueva `sapiens-usb` | `dep-dbtp90d6e07s73apshqg` | `541a4591aeb2837cdbdc4b3ec277876ca41d18ee` |
| API existente | `dep-dbtp6grl550s738uec6g` | `5b91bf559012338cd2224ebe93dbcea706fbe179` |
| Interfaz anterior, conservada | `dep-dbtp90h42hec73ajn8hg` | `541a4591aeb2837cdbdc4b3ec277876ca41d18ee` |

`541a459` modifica únicamente el contraste CSS de hover; el backend coincide con `5b91bf5`. Los tres servicios tienen autodeploy desactivado y plan Free; Neon confirmó Free. No hubo nuevas migraciones, bases ni cambios de contraseña. Tras una denegación inicial de revisión automática, el usuario autorizó expresamente configurar en el nuevo frontend los tres valores privados necesarios: SECRET_KEY, firma Flask–API y conexión de sesiones del usuario `mira_web`. Se validaron el mismo propietario, destino Neon y una lista cerrada de diez variables de runtime; no se trasladaron tokens de proveedores ni conexiones administrativas. La configuración de los servicios anteriores se conserva.

El nuevo `/registro` guarda la cuenta en `usuario` usando `sp_usuario_crear`, bcrypt coste 12 y solo el rol activo Investigador, resuelto por nombre en servidor. No se crean personas académicas ni se infiere afiliación. La web confirma la contraseña y crea una sesión de servidor después del registro. La API rechaza roles/estado/identificadores adicionales; comparte con login las cuotas por origen y cuenta, límite de cuerpo, firma de origen y concurrencia bcrypt. No reintenta POST con credenciales. El correo duplicado no reemplaza hashes ni reactiva cuentas.

Se comprobaron en la URL pública: registro y reingreso mediante navegador, persistencia y rol en Neon, duplicados 409, datos de privilegios 400, contraseña incorrecta/sin token/revocado 401, administración 403, salud/readiness y HEAD sin autenticación. La cuenta QA creada por estas pruebas quedó inactiva y sus sesiones se cerraron. La pasada final verificó 17 muestras responsive y contraste de hover 4,54:1; los dispositivos fueron emulados. [Resultados, capturas y límites](evidencias/acceso-20261116/verificacion.json), [pruebas reproducibles y pendientes manuales](V3_PRUEBAS.md).

Los servicios comparten las cuotas gratuitas del espacio de Render; no hay garantía de disponibilidad continua y pueden suspenderse por inactividad o límites del plan. No se añadió correo de verificación, recuperación por email ni controles para acreditar pertenencia institucional. Los ensayos anteriores de suspensión y recuperación de respaldo no se repitieron en esta actualización. C06 y la revisión formal del PR #7 permanecen pendientes; no se declara validada producción institucional.

## Antecedentes de despliegue

Verificación inicial del **13 de noviembre de 2026** y republicación comprobada el **14 de noviembre**, zona America/Bogota. Repositorio `francis872/Mira`, rama `feature/mira-v3`. La interfaz Flask y la API se publican en Render y reutilizan la base Neon existente. Se mantienen V1–V3 y el despliegue anterior de Vercel.

> El ensayo del 14 de noviembre falló y queda conservado. La repetición aprobada del **16 de noviembre**, con las correcciones siguientes, recuperó el inicio desde el navegador en **51,571 segundos**, con suspensión real confirmada y sin calentamiento manual de API. Es un muestreo con JavaScript, no una certificación de disponibilidad continua. La restauración nativa del respaldo sigue verificada en su copia Neon. [Resultados y condiciones de entrega](REVISION_ENTREGA_2026-11-14.md).

## Recursos y versión

| Componente | Recurso | Estado comprobado |
|---|---|---|
| Interfaz Flask / Gunicorn | [sapiens-web-v1-v3-20261113.onrender.com](https://sapiens-web-v1-v3-20261113.onrender.com) · `srv-dbrijl0ae00c73fpf0mg` | Docker, Render Free, Virginia, una instancia |
| API .NET | [sapiens-api-v1-v3-20261112.onrender.com](https://sapiens-api-v1-v3-20261112.onrender.com) · `srv-dbr6i3dckfvc73dc3hr0` | Docker, Render Free, Virginia, una instancia |
| PostgreSQL 16 | Neon, proyecto `snowy-bonus-50718377`, base `sapiens` | Organización Free; conexión TLS `verify-full` y `channel_binding=require` |

### Antecedente: republicación anterior del 16 de noviembre

Por solicitud de despliegue se republicaron ambos servicios desde `4a9c0463cf6791b0aff83315b12dfd7d61adbb0a`, con estado `live` comprobado en Render:

| Servicio | Despliegue activo | Fuente |
|---|---|---|
| API | `dep-dbtoe4t6e07s73anhhmg` | `4a9c046` |
| Interfaz | `dep-dbtoeb8n74is73eck580` | `4a9c046` |

El código de API/SQL/Dockerfile coincide con `caf4c8c`, y el frontend con `68d2261`. Las diferencias de esta revisión eran documentales. No se modificaron variables privadas, credenciales, esquema ni planes; no se crearon servicios o bases. Ambos autodeploys siguen deshabilitados. Se comprobaron **22 solicitudes HTTP antes y después**: salud/readiness, HEAD de acceso, autenticación, mismo JWT válido tras reemplazar la API, misma sesión/SID e inicio protegido 200 tras reemplazar Flask, cierre de sesión y reutilización del JWT revocado 401. Neon confirmó Free. [Evidencia de republicación](evidencias/republicacion-render-20261116/verificacion.json).

Este paso comprueba el redespliegue y la continuidad de sesiones; no repite las suites completas ni el ensayo de suspensión siguiente. La sesión web se conservó durante ambos reemplazos. Esto es una comprobación nueva y distinta de la conservación del SID anónimo durante el ensayo de arranque, que quedó sin medir por reinicio del instrumento.

### Validación anterior del 16 de noviembre: recuperación verificada y límites

El ensayo descrito en esta sección utilizó frontend `68d22614593b7efe2bc3aee57ee4646fb7cdf1e7`, despliegue `dep-dbtkgnbl550s73di9g5g`, y API `caf4c8c`, `dep-dbrj24gn74is73fo7a4g`. En ese ensayo no se redesplegó la API. Ambos servicios conservaban Free, autodeploy deshabilitado, sus variables privadas y health checks anteriores. La republicación posterior figura arriba y mantiene el mismo código de aplicación. No se aplicaron migraciones ni se cambiaron credenciales.

La espera fija de ocho segundos era inferior a los 16,283 segundos observados al despertar la API desde un cliente externo. `frontend/app.py::_wake_render_api` consulta `/health` sin credenciales antes del login de Render y de `/ready`. Solo repite GET de salud; el POST con credenciales se envía una vez. Se conserva la comprobación del origen Cloudflare antes de cualquier consulta de arranque, la firma HMAC, TLS, CSRF y RBAC. `/health` de Flask sigue sin despertar dependencias; no hay un proceso de keep-alive.

El presupuesto previsto es de 45 segundos para preparar/enviar el login, reservando ocho para su envío después de la espera; `/ready` permite 35 segundos para activar la API antes de comprobar dependencias. Las demás llamadas conservan ocho segundos. Los límites connect/read de Requests no garantizan un máximo estricto de tiempo total, especialmente durante DNS. [Semántica oficial de los timeouts](https://requests.readthedocs.io/en/latest/user/advanced/#timeouts).

El primer candidato, `f522f8b`, recibió 17 respuestas 502 al consultar salud con `Accept: application/json`; después de despertar manualmente la API, `/ready` web respondió 200. El segundo, `8bc49a0`, cambió ese encabezado a `*/*` y volvió a fallar tras suspensión real: primer login 503 en 36,294 segundos y readiness sin recuperar. **El encabezado no resolvió el hallazgo; la causa interna del proveedor no está demostrada.** Se preservan ambos fallos y los dos errores de conexión del cliente HTTP anteriores a cargar el formulario.

`frontend/static/render-startup.js` hace un GET público de salud desde el navegador al cargar el acceso en el perfil Render. Omite cookies, autorización y referente, no lee el formulario ni maneja JWT, y limita su espera a 45 segundos mediante AbortController. El navegador solo activa el servicio: su respuesta `no-cors` es opaca y no demuestra autenticación. La disponibilidad y las credenciales se comprueban después en Flask/API. La recuperación automática de esta topología requiere JavaScript y acceso del navegador a esa URL pública; `/ready` por sí solo continúa reflejando dependencias no disponibles cuando la API está dormida.

Edge detectó un error del primer script (`32bed57`): combinar `no-cors` con `redirect: error` impide que salga la consulta. `3018573` lo corrige a `follow`, exigido por el [estándar Fetch](https://fetch.spec.whatwg.org/#main-fetch). Esa consulta no contiene credenciales, tampoco en redirecciones; el POST de login y los GET de salud del servidor mantienen `allow_redirects=False`. No se habilitó CORS sobre la API ni se modificó su autorización.

El ensayo completo de Edge sobre `3018573` volvió a confirmar ambas instancias ausentes y Neon idle, pero no obtuvo el formulario: la página de espera hizo HEAD repetidos y aparecieron 503/502. Los logs de Flask identificaron **siete HEAD `/login` con duración aproximada de 36–37 segundos**. `seguridad.register_security.login` solo consideraba GET como consulta; HEAD entraba por error en la rama que enviaba un POST de autenticación. La nueva espera agravaba esa ocupación de los cuatro threads. La URL de salud del navegador ni siquiera llegó a ejecutarse en ese intento.

`68d2261` corrige el despacho: GET y HEAD renderizan la página de acceso; HEAD no autentica ni consulta dependencias. Dos pruebas envían HEAD con y sin datos de credenciales, sin CSRF, y comprueban 200 sin cuerpo, cero llamadas a API y ausencia de identidad autenticada. La prueba aislada anterior, con web activa y API dormida, sí alcanzó login 302 e inicio 200; una aserción de instrumentación sobre headers provisionales impidió completar sus demás controles. No se presenta como aprobación integral.

Hay **170 pruebas Flask aprobadas y cinco omitidas** por requerir PostgreSQL exclusivo de pruebas. El [verificador nuevo de Edge](../scripts/verify-render-startup.mjs) aprobó la consulta automática: un GET, HTTP 200 por HTTPS —QUIC en esta ejecución— y sin cuerpo/cookie/autorización/referente/assertion, comprobados también en los headers efectivos de CDP ExtraInfo.

La repetición completa aprobó después de **964,694 segundos sin tráfico propio**, con **cero instancias en ambos servicios y Neon idle** antes de abrir el acceso. El navegador recién conectado esperó el documento real de SAPIENS y ejecutó el flujo sin consultas manuales a la API. El instrumento espera hasta 150 segundos por comandos de navegación/DOM; los presupuestos de la aplicación siguen iguales. Los intentos previos interrumpidos por CDP y el intervalo extendido de 43 minutos se conservan como resultados instrumentales, no como aprobaciones de la aplicación. Una prueba local de transición 503→200 verificó el nuevo mecanismo del instrumento sin contactar la instalación.

| Comprobación del ensayo aprobado | Resultado |
|---|---|
| Navegación inicial hasta inicio autenticado | **51,571 s**; 28,007 s desde enviar el formulario hasta el inicio |
| Primer login | **Un POST, 302**; ningún reenvío de credenciales |
| Inicio / readiness / logout | Inicio **200**, `/ready` web **200**, cierre regresa al acceso |
| Activación de API desde navegador | Un GET de salud, sin cuerpo, Cookie, Authorization, Referer ni assertion; sin calentamiento manual |
| HEAD en los logs de este arranque | **200 en 0,788 s**; GET de acceso 200 en 0,040 s. Antes había HEAD de 36–37 s |
| Instancias y sesión | Ambas instancias cambiaron; SID rotado al autenticar. La conservación del SID anónimo anterior no se pudo medir porque se reinició el navegador del instrumento |
| JWT emitido realmente | Vigencia de **1200 s**, `/auth/me` 200 al emitir; el mismo token devolvió **401** a los 5499 s desde su emisión, después de expirar, sin cambiar duración, claves ni reloj. Esta comprobación es independiente de los 16 minutos del último intervalo de arranque |

Después aprobaron **16 comprobaciones remotas de seguridad/operación**: las 14 del programa base, contraseña incorrecta desde la web 401 y denegación nativa de `/api/auth/usuarios` 403 para Investigador. La expiración por reloj real es una comprobación separada del ensayo. Las **seis muestras responsive** cubren acceso e inicio a 1280×800, 768×1024 y 390×844 en Edge con viewport/touch emulados: sin overflow, botones sin nombre ni excepciones JavaScript observadas. No son pruebas en dispositivos físicos ni una repetición completa de los E2E históricos. [Evidencia, fuentes y fallos conservados](evidencias/arranque-render-20261116/verificacion.json).

### Antecedente: republicación del 13–14 de noviembre

Ambos servicios servían el commit [`caf4c8cd0c8debb641f6605b478a2f043e11f22f`](https://github.com/francis872/Mira/commit/caf4c8cd0c8debb641f6605b478a2f043e11f22f). Se solicitó su despliegue el 13 de noviembre, a las 15:47 UTC, y se confirmó nuevamente `live` el 14 de noviembre:

| Servicio | Despliegue activo |
|---|---|
| API | `dep-dbrj24gn74is73fo7a4g` |
| Interfaz | `dep-dbrj256tlv9c738maehg` |

[Evidencia de republicación y comprobaciones posteriores](evidencias/render-neon-20261114/redespliegue.json). Se conservaron variables privadas, planes Free, Neon, el mismo JWT y el mismo SID durante el redespliegue; el logout posterior invalidó ambos accesos. El 14 de noviembre se aprobaron las 14 comprobaciones de seguridad/operación y, adicionalmente, el rechazo 401 de contraseña incorrecta desde la interfaz. Ambos `/health` y `/ready` respondieron 200 con correlación de la aplicación. Render y Neon confirmaron sus planes Free.

La verificación requirió repetición: el primer intento dejó ocho comprobaciones aprobadas y un timeout de lectura de 40 segundos al consultar `/login`; el segundo dejó una aprobada y falló al exigir 200 de `/ready` web, sin capturar su código real. La tercera corrida completó las 14. Se preservan los intentos incompletos; no se afirma una primera corrida limpia ni una causa de red demostrada. La comprobación posterior de `/health` API tardó 16,334 segundos y los restantes endpoints alrededor de un segundo. Estos datos no garantizan disponibilidad continua ni sustituyen un ensayo controlado de arranque frío.

`git diff` confirma que el código de ejecución no cambió respecto de la validación funcional anterior. No se repitieron los 50 E2E completos ni el navegador en esta republicación. Los resultados de la sección de pruebas siguiente pertenecen a la verificación inicial. La documentación posterior no exige reconstruir las imágenes.

### Versiones de la verificación inicial

Frontend inicial: commit [`0891fe160bcaa898b73bb748fe596aed3ae9acce`](https://github.com/francis872/Mira/commit/0891fe160bcaa898b73bb748fe596aed3ae9acce), despliegue `dep-dbriod0n74is73fngprg`, entonces `live`. La API utilizaba `dep-dbr6nft6e07s73an9bd0`, commit `630721a118f7cfa38093d1da8e885d502c889ddc`: `git diff` confirma que `MIRA.Api`, `Dockerfile` y `database` no cambian entre ese commit y el frontend aceptado.

El primer despliegue web `dep-dbrijlgae00c73fpf1cg`, basado en `b40a4ca`, se usó para observar el ingreso de solicitudes. El despliegue aceptado lo reemplazó: se retiró el comando temporal de logs y se restauró el CMD de la imagen. No se ejecutaron migraciones, restablecimientos de cuentas ni cambios de credenciales existentes. Las pruebas funcionales crean registros QA identificados e inactivan sus propios registros; la base sigue siendo de demostración.

## Configuración y seguridad

El [ejemplo de los dos servicios](../deploy/render-neon-free.yaml.example) fue validado por Render CLI: `valid=true`, dos acciones propuestas. **Validar no aplica el Blueprint.** Los recursos reales se configuraron por la API del proveedor. Revisar nombres, URL y secretos antes de importar el ejemplo; no volver a crear la base ni usar el ejemplo histórico con planes starter/disco.

| Servicio | Variables o configuración |
|---|---|
| API | `ASPNETCORE_ENVIRONMENT=Production`, `PORT=10000`, `Jwt__Issuer=MIRA`, `Jwt__Audience=MIRA.Frontend`, `Jwt__Minutes=20`; `ConnectionStrings__PostgreSql`, `Jwt__Key`, `LoginProtection__ForwardingKey` privados |
| Flask | Root Directory `frontend`; Dockerfile `./Dockerfile`, contexto `.` relativos a esa raíz; CMD de imagen, Gunicorn, `PORT=10000`, `WEB_CONCURRENCY=1` |
| Flask | `MIRA_ENV=production`, `API_URL=https://sapiens-api-v1-v3-20261112.onrender.com/api`, `SESSION_COOKIE_SECURE=true`, `MIRA_PLATFORM=render` |
| Sesiones Flask | `MIRA_SESSION_BACKEND=postgres`, `MIRA_SESSION_DATABASE_URL` privada con rol `mira_web`, `sslmode=verify-full`, `channel_binding=require` |
| Secretos Flask | `SECRET_KEY` nueva para esta instalación y `MIRA_LOGIN_FORWARDING_KEY` existente, iguales solo las dos claves de forwarding entre API y Flask; JWT, sesión y forwarding usan tres claves distintas |

Los secretos se almacenan como variables privadas del proveedor y en archivos locales ignorados para operación. No aparecen en este ejemplo, evidencias ni Git. El JWT permanece en el servidor; el navegador recibe un SID opaco `Secure`, `HttpOnly`, `SameSite=Strict`. No se depende del disco efímero de Render para sesiones.

Consultas de solo lectura verificaron cinco condiciones: API y web no pueden crear objetos en `public`; web no puede leer tablas del dominio y sí mantener `mira_web.sesion`; API no puede insertar, actualizar ni eliminar el registro de migraciones. Las 15 entradas del registro coinciden con los nombres de migración del repositorio y sus archivos conservan los hashes de la evidencia de aplicación anterior. No se presenta esto como recalcular un checksum almacenado en la base: el registro guarda nombres y la comparación de hashes usa la evidencia local de Fase 6.

### Origen de acceso y D01

`frontend/login_protection.py::configure_login_protection` admite el perfil explícito `render` únicamente con las variables de servidor oficiales `RENDER=true` y `RENDER_SERVICE_TYPE=web`. `client_origin` toma una sola IP canónica de `CF-Connecting-IP`. Un valor ausente, inválido, con múltiples IP o zona IPv6 produce fallo cerrado: `frontend/app.py::api_call` no envía el login a la API. Se conservan el perfil Vercel y el modo de proxies con CIDR explícitos.

La frontera de confianza de este perfil es **Render Free con ingreso público del proveedor**, sin un proxy propio adicional. Render documenta que sus servicios web Free no reciben tráfico por la red privada y que el ingreso público pasa por Cloudflare. Cambiar a una modalidad que permita ingreso privado o añadir otro intermediario requiere revisar esta frontera. No se confía en cualquier peer ni en el primer elemento de `X-Forwarded-For`. Fuentes: [red privada Render](https://render.com/docs/private-network), [ingreso y protección de borde](https://render.com/articles/how-render-handles-ddos-attacks), [variables oficiales](https://render.com/docs/environment-variables), [semántica de cabeceras Cloudflare](https://developers.cloudflare.com/fundamentals/reference/http-headers/).

Prueba sobre el servicio real: una solicitud base y otra con `X-Forwarded-For`, `X-Real-IP` y `X-Vercel-Forwarded-For` falsificados llegaron a Flask con el mismo origen Cloudflare. El XFF conservó el valor inyectado. Las solicitudes de prueba con `CF-Connecting-IP` falsificado recibieron 403 de Cloudflare, sin identificador de correlación de la aplicación. Los logs originales quedan privados; la evidencia pública contiene booleanos y no direcciones de clientes. La instrumentación temporal se retiró antes de aceptar la versión. Estas observaciones acreditan las solicitudes ensayadas, junto con el contrato del proveedor; no constituyen una prueba de todos los caminos futuros de red.

El origen canónico se firma con el protocolo HMAC existente (`LoginOriginAuth`); no cambiaron `LoginProtectionMiddleware`, bcrypt, JWT, roles ni cuotas en la API. Las cuotas siguen siendo por instancia, sin garantía distribuida.

## Pruebas de la verificación inicial (13 de noviembre)

La [evidencia saneada](evidencias/render-neon-20261113/verificacion.json) incluye casos, resultados, versiones y marcas de tiempo. Los resultados históricos permanecen separados.

| Verificación | Resultado |
|---|---|
| Flask, sin E2E y sin base local configurada | **146 aprobadas, cinco omitidas**; 16 casos nuevos del perfil Render. Las cinco omitidas requieren PostgreSQL local aislado; no se cuentan como aprobadas |
| E2E HTTP V1–V3 sobre la nueva URL Render y Neon | **50/50 aprobadas en una corrida**, cero omitidas; creación, edición, inactivación, asociaciones y reportes según los casos de `frontend/tests/e2e` |
| Seguridad y operación remotas | **15 comprobaciones aprobadas**: health/readiness de ambos servicios, Swagger oculto, login correcto e incorrecto en API e interfaz, solicitud anónima, denegación por rol, SID/cookies, CSRF, logout y reutilización del mismo JWT revocado |
| Navegador Edge real, tamaños emulados | **11 combinaciones página/ancho**, login/inicio/universidades/reportes, 1440/768/390 px según el caso; sin desbordamiento, errores de consola ni botones sin nombre; **ocho axe**, cero infracciones y cero incomplete en este muestreo |
| Reinicio del frontend | Instancia anterior y posterior diferentes; misma cookie/SID recupera acceso sin segundo login; `/ready` 200; logout invalida su reutilización. No se afirma cero interrupción |
| Perímetro | Carga maliciosa del caso local de persistencia rechazada con 403 de Cloudflare; no es el criterio de almacenamiento y escape en la aplicación |
| Neon | TLS real y cinco controles de privilegios aprobados mediante consultas de solo lectura; 15 migraciones existentes |
| Configuración | Ambos servicios `plan=free`, autodeploy deshabilitado; claves independientes; comando de logs temporal retirado; Blueprint válido |
| Secretos | Escaneo heurístico y búsqueda en memoria de los secretos conocidos contra los archivos a publicar; sin coincidencias |

El caso `test_injection_text_is_stored_as_data_and_escaped` se excluyó de los 50 E2E remotos porque el borde rechaza la carga. Su aserción original permanece intacta; su resultado local pertenece a Fase 6, no a esta ejecución. El rechazo remoto se comprobó por separado. Los reintentos de transporte del cliente de regresión solo cubren establecimiento de conexión, nunca lecturas ni respuestas HTTP, y no modifican el transporte de la aplicación.

La duración JWT de 1200 segundos se verificó en los claims de un token auténtico. La espera de expiración real con token de 60 segundos corresponde a la [evidencia histórica de Fase 6](cierre-v1-v3/evidencias/fase6-20261112/seguridad-preview.json) sobre la misma API; en esta operación no se modificó la duración ni se esperaron 20 minutos. No se repitieron suites .NET/SQL ni auditorías de dependencias, dado que API, migraciones y dependencias no cambiaron. El reinicio real y las pruebas HTTP nuevas acreditan el uso de sesiones compartidas en este despliegue, sin sustituir los cinco tests locales omitidos.

## Operación reproducible

1. Abrir la interfaz HTTPS. Si hubo inactividad, esperar el arranque de ambos servicios y volver a intentar; no crear cuentas ni modificar datos para resolver un arranque frío.
2. Consultar `/health` y `/ready` en API y web. `/health` solo acredita vida del proceso. `/ready` comprueba dependencias. Render usa `/ready` para API y `/health` para web; así el chequeo periódico del frontend no consulta continuamente la API. Antes de demostrar, comprobar manualmente ambos `/ready`.
3. Usar las credenciales privadas existentes. No imprimirlas, ponerlas en URLs ni guardarlas en el repositorio. Para la regresión con datos QA exclusivamente, definir por un gestor privado `MIRA_E2E_URL`, `MIRA_E2E_API_URL`, `MIRA_E2E_EMAIL` y `MIRA_E2E_PASSWORD`; ejecutar `python -m pytest frontend/tests/e2e -k "not test_injection_text_is_stored_as_data_and_escaped" -q` en un entorno de pruebas preparado. Los logs completos pueden contener cookies: no publicarlos sin revisión.
4. Para actualizar, revisar y publicar un commit en `feature/mira-v3`, comprobar pruebas y secretos, desplegar ese SHA manualmente en Render y revisar estado `live`, `/ready`, login y roles. Los autodeploys permanecen apagados. Cambios de documentación no requieren reconstruir imágenes.
5. Conservar la misma `SECRET_KEY` de esta instalación y la misma conexión de sesiones al reiniciar o redesplegar. Rotarla invalida la firma de sesiones existentes. Un cambio de esquema requiere un procedimiento independiente con respaldo, revisión y rol migrador; no forma parte del arranque.

El [ejemplo de despliegue](../deploy/render-neon-free.yaml.example), `frontend/Dockerfile` y `frontend/gunicorn.conf.py` describen los parámetros reproducibles. No importar el ejemplo sobre recursos ajenos ni añadir claves literales.

Para repetir específicamente el ensayo de arranque, registrar primero readiness con ambos servicios activos y una página de login con sesión anónima. Dejar **23 minutos sin HTTP ni SQL propios** hacia la instalación. Antes del primer acceso, comprobar en los paneles/API de control del proveedor que ambos servicios no tienen instancias y Neon está idle; consultar `/health` en ese momento alteraría el ensayo. El tráfico externo no está bajo control: si siguen activos, registrar el ensayo como inconcluso respecto de suspensión real.

Después del intervalo, acceder a `/login` y enviar el formulario una sola vez desde la interfaz. Registrar código y duración del primer GET/POST, acceso al inicio y `/ready` web, y cierre de sesión. No despertar manualmente la API durante esa medición. Cualquier repetición explícita del login debe registrarse aparte del primer intento. Conservar solo códigos, tiempos y correlaciones saneadas, sin cookies ni credenciales. La comprobación rutinaria previa a una sustentación y este ensayo controlado son operaciones distintas.

## Límites y pendientes

- Solo se seleccionaron planes **Free**. Render comparte **750 horas gratuitas por workspace/mes**, se suspende por inactividad y puede tardar aproximadamente un minuto en despertar. Dos servicios activos consumen horas por separado; no se afirma disponibilidad continua. No se modificaron pagos ni límites de facturación. [Condiciones y límites actuales](https://render.com/docs/free).
- El ensayo del 14 de noviembre falló y se conserva. La repetición del 16 aprobó el flujo de navegador con JavaScript y suspensión confirmada, tras corregir HEAD y activar salud desde el cliente. No demuestra una disponibilidad continua ni que un cliente sin JavaScript despierte por sí solo ambos servicios. Se mantienen pendientes carga sostenida y validación institucional. No se instalaron keep-alives. [Cronología y recuperación de backup](REVISION_ENTREGA_2026-11-14.md).
- **Actions excluido como requisito por el usuario el 15 de noviembre**. Se conserva como antecedente el bloqueo de cuenta de la [ejecución 43452629583](https://github.com/francis872/Mira/actions/runs/43452629583), SHA `0891fe1`, sin pasos iniciados; no se lo presenta como aprobación de CI. Ya no se exige resolver facturación ni obtener Actions verde. El PR #7 conserva pendientes de revisión y aceptación académica; el [registro del 15 de noviembre](evidencias/decision-entrega-20261115.json) es anterior al ensayo aprobado del 16.
- Dispositivos físicos, lector de pantalla, validación institucional de carga/recuperación, respaldo periódico y aceptación académica de C06/otros pendientes de V2 siguen abiertos. El muestreo axe no certifica accesibilidad completa. El dossier de sustentación anterior sigue siendo una instantánea independiente; no se alteró.

**Resultado:** plataforma desplegada y verificada para demostración en Render + Neon con los alcances anteriores. No se certifica producción institucional ni cierre académico integral.
