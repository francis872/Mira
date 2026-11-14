# SAPIENS: despliegue completo en Render y Neon

Verificación inicial del **13 de noviembre de 2026** y republicación comprobada el **14 de noviembre**, zona America/Bogota. Repositorio `francis872/Mira`, rama `feature/mira-v3`. La interfaz Flask y la API se publican en Render y reutilizan la base Neon existente. Se mantienen V1–V3 y el despliegue anterior de Vercel.

## Recursos y versión

| Componente | Recurso | Estado comprobado |
|---|---|---|
| Interfaz Flask / Gunicorn | [sapiens-web-v1-v3-20261113.onrender.com](https://sapiens-web-v1-v3-20261113.onrender.com) · `srv-dbrijl0ae00c73fpf0mg` | Docker, Render Free, Virginia, una instancia |
| API .NET | [sapiens-api-v1-v3-20261112.onrender.com](https://sapiens-api-v1-v3-20261112.onrender.com) · `srv-dbr6i3dckfvc73dc3hr0` | Docker, Render Free, Virginia, una instancia |
| PostgreSQL 16 | Neon, proyecto `snowy-bonus-50718377`, base `sapiens` | Organización Free; conexión TLS `verify-full` y `channel_binding=require` |

### Versión activa: republicación solicitada

Ambos servicios sirven el commit [`caf4c8cd0c8debb641f6605b478a2f043e11f22f`](https://github.com/francis872/Mira/commit/caf4c8cd0c8debb641f6605b478a2f043e11f22f). Se solicitó su despliegue el 13 de noviembre, a las 15:47 UTC, y se confirmó nuevamente `live` el 14 de noviembre:

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

## Límites y pendientes

- Solo se seleccionaron planes **Free**. Render comparte **750 horas gratuitas por workspace/mes**, se suspende por inactividad y puede tardar aproximadamente un minuto en despertar. Dos servicios activos consumen horas por separado; no se afirma disponibilidad continua. No se modificaron pagos ni límites de facturación. [Condiciones y límites actuales](https://render.com/docs/free).
- Esta validación se hizo con servicios activos. Queda pendiente la prueba conjunta tras al menos 15 minutos de inactividad de ambos servicios, además de carga sostenida y seguimiento de cuotas. Una API dormida puede causar un 503 temporal en Flask hasta completar el arranque. No se habilitaron peticiones artificiales para impedir la suspensión.
- **GitHub Actions bloqueado por la cuenta**: [ejecución 43452629583](https://github.com/francis872/Mira/actions/runs/43452629583), SHA `0891fe1`, jobs `quality` y `containers` fallidos con cero pasos; anotación: `The job was not started because your account is locked due to a billing issue.` No es una aprobación de CI. El PR #7 sigue Draft y sin merge; el titular/soporte debe resolver el bloqueo sin necesidad de contratar planes por esta operación.
- Dispositivos físicos, lector de pantalla, validación institucional de carga/recuperación, respaldo periódico y aceptación académica de C06/otros pendientes de V2 siguen abiertos. El muestreo axe no certifica accesibilidad completa. El dossier de sustentación anterior sigue siendo una instantánea independiente; no se alteró.

**Resultado:** plataforma desplegada y verificada para demostración en Render + Neon con los alcances anteriores. No se certifica producción institucional ni cierre académico integral.
