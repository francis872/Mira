# D01 — límite de login con Flask intermediario

Corrección local de Fase 3, 2026-11-07. No publicada; sin cambios de marca, migraciones, roles o contratos de negocio. C06 y la aceptación del profesor permanecen pendientes.

## Causa y controles

La API atribuía todas las peticiones de Flask a su misma IP de conexión. Un cliente agotaba los diez intentos por minuto y otro recibía 429 en su primer login válido. No era un fallo de bcrypt ni una evasión de autorización.

La cuota original se conserva y se combina con tres controles:

| Control | Implementación y alcance |
|---|---|
| Origen | Diez solicitudes de login por minuto y origen autenticado. Sin afirmación de Flask se usa la IP de conexión real de la API. Cambiar correo o cookie no reinicia esa cuota. |
| Cuenta | Diez solicitudes por minuto y correo normalizado, entre orígenes distintos. La clave de partición es SHA-256; no se registran correos en ella. El cálculo ocurre antes de consultar si existe la cuenta. |
| Trabajo concurrente | Cuatro logins simultáneos en la instancia, sin cola, para limitar trabajo de contraseña entre cuentas/orígenes. No es una cuota temporal global ni una defensa completa contra DDoS. |

Los límites cuentan solicitudes, incluidas las exitosas y las rechazadas por validación. Un 429 no identifica la partición ni confirma que una cuenta exista. Credenciales incorrectas siguen dando 401 uniforme; solicitudes inválidas conservan 400. El cuerpo de login está acotado a 8192 bytes; un exceso devuelve 413 antes de trabajo de credenciales.

## Identidad de origen y confianza

`frontend/login_protection.py` obtiene la IP de `request.remote_addr`. Ignora X-Real-IP y no usa X-Forwarded-For desde un peer ajeno a las redes explícitas de `MIRA_TRUSTED_PROXY_NETWORKS`. Si el peer es confiable, recorre X-Forwarded-For de derecha a izquierda y se detiene en el primer salto no confiable; una cadena inválida o ausente conserva la IP del peer. Se rechazan redes /0. No se instala ProxyFix ni se confía automáticamente en una cantidad de saltos.

Flask construye una nueva cabecera `X-Mira-Login-Origin`: versión, timestamp UTC, IP canónica y HMAC-SHA256. Se firma el método, ruta y SHA-256 del cuerpo realmente preparado por requests, además del origen y timestamp. No se copian afirmaciones suministradas por el navegador. El login no sigue redirects de la API.

`LoginProtectionMiddleware` verifica la firma en tiempo constante, formato, cuerpo y ventana de 60 segundos antes del limitador y de comprobar contraseñas. Una afirmación inválida obtiene 401 y no alcanza bcrypt. Sin cabecera, el cliente directo no puede elegir su origen mediante X-Forwarded-For o X-Real-IP. No se registran cabeceras, contraseñas, firmas ni claves.

La firma autentica al intermediario que posee la clave; no certifica por sí sola la configuración del proxy situado antes de Flask. El operador debe conocer sus redes reales y garantizar que el proxy añade/sobrescribe los datos de origen correctamente. No usar redes genéricas para aparentar identificación confiable. En NAT, proxies desconocidos o cabecera ausente, clientes pueden seguir compartiendo la IP observada: es una limitación explícita, no una identidad inventada.

TLS y un API_URL controlado son necesarios fuera del ensayo local. La afirmación no sustituye JWT ni autoriza acceso a datos. Una repetición dentro de su ventana sigue consumiendo las mismas cuotas de origen/cuenta. Su clave no permite emitir JWT y se mantiene separada de ellos.

## Configuración requerida

- API: `LoginProtection__ForwardingKey`, externa y de al menos 32 bytes UTF-8, diferente de Jwt__Key.
- Flask: `MIRA_LOGIN_FORWARDING_KEY`, el mismo valor externo, diferente de SECRET_KEY.
- Producción rechaza una clave ausente, corta o placeholder. Desarrollo directo puede omitirla: no se aceptan entonces afirmaciones de origen. Para comprobar D01 con Flask, ambos servidores necesitan el mismo valor.
- `MIRA_TRUSTED_PROXY_NETWORKS`: CIDR separados por comas, vacío por defecto; solo redes de proxies realmente controlados. La lista vacía es correcta para conexión directa al frontend y no permite atribuir IP externas tras un proxy desconocido.
- Compose usa `LOGIN_FORWARDING_KEY` para ambos servicios. `.env.example` solo contiene un placeholder; no se modificaron archivos privados reales.
- `start-local.ps1` genera, si hace falta, una clave independiente en `.local/login-forwarding-key`, sin sobrescribir una existente. Reiniciar API y frontend de forma coordinada para cargar la configuración; no mezclar procesos antiguos y nuevos.
- Los verificadores SQL/Docker y el smoke generan una clave exclusiva de prueba. La API no ejecuta DDL ni cambia credenciales por esta corrección.

Los contadores son de una instancia y se reinician con el proceso. Una segunda réplica multiplica su presupuesto; no se declara protección distribuida compartida. El límite por cuenta reduce ataques desde diferentes IP sobre la misma cuenta, pero puede bloquear temporalmente al titular si su cuenta es atacada. Ataques entre muchas cuentas y muchos orígenes requieren controles de borde/capacidad adicionales. No se modificó configuración de proxies, claves reales o servicios cloud.

## Archivos y regresión

- API: `MIRA.Api/Configuracion/LoginProtection.cs`, `SeguridadConfiguration.cs`, `Program.cs`.
- Frontend: `frontend/login_protection.py`, `app.py`; Dockerfile incluye el módulo.
- Pruebas: `tests/MIRA.Api.Tests/LoginProtectionTests.cs` (19 casos nuevos) y `frontend/tests/test_login_protection.py` (18 casos nuevos). El doble HTTP existente acepta los argumentos de transporte nuevos; no cambia sus resultados simulados ni se eliminan aserciones previas.
- Regresión actual: 90 .NET, 98 Flask, 43 HTTP E2E, cinco SQL, 50 combinaciones responsive, 18 axe sin infracciones/incomplete, diez grupos de teclado y 27 zoom real. Los resultados nuevos están separados en `.local/fase3-d01-20261107/`; los artefactos históricos permanecen intactos.
- Reproducción integrada: API y PostgreSQL reales exclusivos, clientes Flask con REMOTE_ADDR distintos. Después de diez 401, el atacante obtiene 429; otro origen entra con 302 del frontend. Otra cookie y cabeceras falsas conservan el 429 del origen agotado. La prueba por cuenta normalizada entre orígenes y la de cuatro trabajos concurrentes pasan en el pipeline .NET.

No se certifica producción ni aceptación docente. La prueba interactiva de lectores/dispositivos y las condiciones reales de proxy/HTTPS siguen pendientes. Consulte el informe privado de Fase 3 para el estado final de Docker y sus intentos de compilación.

## Propuesta CI — sin editar el workflow

Los casos nuevos se incluyen en las suites .NET/Flask que ya ejecuta quality. La validación Compose requiere añadir posteriormente un fixture público `LOGIN_FORWARDING_KEY` al paso de configuración; actualmente ese paso no lo define y fallaría por la nueva guarda. Se conserva el fallo cerrado y no se modifica el workflow sin autorización.

Proponer E2E con API/frontend/PG exclusivos, axe/teclado/zoom sobre esa plataforma y artefactos saneados con retención limitada incluso ante fallos. No subir configuración, claves, sesiones, volcados o logs privados completos. Verificar los jobs remotos para el SHA finalmente publicado; no hay push ni aprobación CI nueva en esta fase.

Referencias de implementación: [limitadores y alcance por instancia de ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/rate-limit?view=aspnetcore-10.0), [confianza en proxies y cabeceras](https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0). El protocolo firmado descrito aquí es una decisión de este proyecto, no una función automática de esas bibliotecas.
