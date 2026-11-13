# Guía de sustentación MIRA V3

> Fase 6 (2026-11-12): SAPIENS está operativa en [https://sapiens-v1-v3-20261112.vercel.app](https://sapiens-v1-v3-20261112.vercel.app), con Flask en Vercel Hobby, API .NET en Render Free y PostgreSQL 16 en Neon Free. La URL Production aprobó **50 criterios funcionales (49 en la pasada completa y repetición puntual aprobada tras un timeout TLS)**, 14 comprobaciones de seguridad/operación y una revisión de navegador de escritorio, tableta y móvil emulados. GitHub Actions continúa bloqueado por la cuenta; el PR #7 permanece Draft y sin merge. V2 mantiene pendientes académicos, incluido C06. No se declara cerrada toda la misión ni certificada producción institucional. [Informe vigente](FASE6_INFORME_FINAL.md). Las secciones fechadas de abajo se conservan como antecedentes.

> Seguimiento local 2026-11-07: [cierre académico](V3_CIERRE_ACADEMICO.md). La propuesta local pasa 71 .NET/80 Flask/43 E2E, 18 axe sin infracciones ni incomplete y zoom real Edge; requiere autorización antes de publicarse. El PR #7 conserva su evidencia anterior y la marca MIRA. C06 sigue pendiente del profesor.

Revisión final: 2026-11-05. [Dictamen, matriz de requisitos y producción](V3_REVISION_FINAL.md). Preparada para demostración académica local; no declarar validado Render/Neon. Los ocho avisos Python anteriores están corregidos en el runtime Linux auditado; conservar la distinción entre prueba local y producción.

## Preparación reproducible

1. Instalar .NET 10, Python y PostgreSQL 16/17 para ejecución local. `dotnet restore MIRA.sln`; `python -m pip install -r frontend/requirements-dev.txt` (incluye requirements.lock).
2. Configurar `.env.local` como indica scripts/README.md. `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-local.ps1`. Aplica migraciones pendientes sin borrar datos; genera una clave JWT local si falta JWT_KEY.
3. Una sola vez: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/bootstrap-admin.ps1`. Introducir correo y contraseña privada; no mostrarla en la sustentación. Si ya existe un administrador activo, el procedimiento rechaza duplicar el aprovisionamiento.
   Si el administrador existente tiene una credencial heredada no verificable, el operador con acceso a PostgreSQL puede ejecutar el mismo script con `-ResetPassword`. `AuthService.RestablecerAdmin` exige que esa cuenta sea administradora y esté activa, almacena bcrypt y revoca sus sesiones.
4. Abrir `/login`; entrar con el administrador; crear cuentas de prueba Investigador y Coordinador en `/usuarios`. Nunca usar datos o contraseñas institucionales reales para la demostración.
5. Verificación completa aislada: `scripts/verify-postgres16.ps1` usa puertos 5547/8082/5001 y crea bases nuevas, aplica SQL, ejecuta regresiones y detiene sus procesos. Ver resultados registrados, no asumir que una ejecución futura pasará.

## Recorrido cronometrado de diez minutos

Preparar antes de iniciar: dependencias restauradas, solución compilada, navegador en la plataforma local de demostración y una cuenta de prueba con rol Investigador. Crear cuentas únicamente en una instalación de demostración. Abrir los archivos de la tabla en el editor y el [resumen saneado](evidencias/v3/revision-final.json). No consumir los diez minutos instalando o aprovisionando.

| Tiempo | Demostración y explicación | Archivo / método que se abre |
|---|---|---|
| 00:00–01:00 | Mostrar tres capas, Flask como intermediario y PostgreSQL. Identificar que usuario (acceso) y usuarios (personas académicas) son distintos. | `MIRA.Api/Program.cs`; `MIRA.Api/Configuracion/DbConnectionFactory.cs::CreateConnection`; migraciones 003/006. |
| 01:00–02:00 | Seguir correo y contraseña desde HTTP hasta verificación bcrypt. Distinguir generar hash y verificarlo; coste 12 y límite UTF-8. | `MIRA.Api/Peticiones/AuthController.cs::Login`; `MIRA.Api/Servicios/Seguridad/AuthService.cs::Login`; `PasswordService.Hash/Verify` en ese mismo archivo. |
| 02:00–03:00 | Mostrar los claims, la firma y la respuesta JSON. Explicar que firmar no cifra el contenido. | `MIRA.Api/Servicios/Seguridad/AuthService.cs::GenerarJwt`; `MIRA.Api/Modelos/Seguridad.cs::SesionRespuesta`; `AuthController.Login` devuelve Ok(result). |
| 03:00–05:00 | Ejecutar los seis casos de la siguiente tabla: 200, contraseña incorrecta, sin token, expirado, revocado y 403. Mantener los tokens fuera de capturas. | Swagger local y `tests/MIRA.Api.Tests/SeguridadTests.cs` mediante el comando de demostración. |
| 05:00–06:00 | Explicar por qué un JWT revocado deja de servir, dónde se consulta el rol y por qué un botón oculto no basta. | `MIRA.Api/Configuracion/SeguridadConfiguration.cs::AddSeguridad`; `AuthService.ValidarSesion`; `MIRA.Api/Repositorios/Seguridad/CuentaRepository.cs::Sesion/Revocar` y constante Columns. |
| 06:00–07:30 | Mostrar continuidad V1/V2: catálogo con trigger, grupo con semilleros y asociación. Señalar rollback de toda la ficha si falla un detalle. Apoyarse en las pruebas ya ejecutadas. | `database/migrations/005_v2_area_aplicacion_trigger.sql::fn_area_aplicacion_normalizar`; `MIRA.Api/Servicios/InvestigacionService.cs::GuardarFicha`; `tests/MIRA.Api.Tests/InvestigacionIntegrationTests.cs::SheetCreatesManyRowsThenRollsBackAllChangesWhenLastRowBelongsElsewhere`. |
| 07:30–08:30 | Cambiar 1440 → 768 → 390 px; mostrar menú, formulario, desplazamiento interno de tabla y foco con Tab. Explicar cookie opaca y CSRF. | `frontend/seguridad.py::configure_session/protect_routes`; `frontend/app.py::verify_csrf/api_call`; `frontend/static/mira.css`. |
| 08:30–09:30 | Responder dos preguntas del simulacro: 401 frente a 403 y revocación persistente. Mostrar 71 .NET, 78 Flask, 43 E2E y cinco SQL aprobados; los mismos 43 E2E pasan en Gunicorn/Docker. | `docs/evidencias/v3/revision-final.json`; preguntas de esta guía. |
| 09:30–10:00 | Mostrar OSV sin avisos conocidos y WSGI/sesiones probados localmente; declarar TLS/Render/Neon reales pendientes, revisión asistiva y aceptación C06. | `docs/V3_REVISION_FINAL.md`, riesgos y dictamen. |

## Seis casos reproducibles de autenticación y autorización

Comando sin credenciales reales, desde la raíz, con dependencias restauradas:

```powershell
dotnet test tests/MIRA.Api.Tests/MIRA.Api.Tests.csproj --no-restore --filter 'FullyQualifiedName~MIRA.Api.Tests.SeguridadTests' --logger 'console;verbosity=normal'
```

El comando ejecuta los casos de la tabla y otros de endurecimiento. `TestHostFactory.Create(authenticated:false)` conserva el pipeline real JwtBearer; sustituye los repositorios por datos aislados. No simula el resultado de la validación JWT. La persistencia y revocación contra PostgreSQL se verifican además con `SeguridadDatabaseTests.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration` dentro de la pasada completa del verificador. Un test host no demuestra conectividad Render/Neon.

| Caso obligatorio | Pasos de demostración | Esperado y prueba exacta |
|---|---|---|
| Autenticación correcta | En Swagger local, POST /api/auth/login con una cuenta Investigador de demostración. Autorizar con el token recibido; GET /api/universidad. | 200 en login, token/expiraEn/usuario, y 200 en consulta. `SeguridadTests.Login_Read_ForbiddenWrite_Logout_Revoked`. |
| Contraseña incorrecta | Repetir POST login con el mismo correo válido y una contraseña no vacía incorrecta. No usar un body vacío: eso demuestra validación 400, no credenciales 401. | 401 y mensaje uniforme. `SeguridadTests.BadLoginIs401`, caso test@invalid.test/incorrecta (datos ficticios). |
| Solicitud sin token | Quitar Authorize y hacer GET /api/universidad. | 401 y desafío Bearer. `SeguridadTests.MissingOrInvalidTokenIs401`, caso null. |
| Token expirado | En el comando automatizado abrir el caso InvalidJwtClaimsAre401(kind: expired). El test firma un JWT con exp en el pasado, conserva jti y utiliza solo la clave aislada de Testing. | GET /api/auth/me devuelve 401 por vigencia. No se cambia el reloj ni se imprime el token. `SeguridadTests.InvalidJwtClaimsAre401`. |
| Sesión revocada | Login nuevamente; autorizar; GET /api/universidad → 200; POST /api/auth/logout → 200; repetir GET con exactamente el mismo token antes de que expire. | La última solicitud devuelve 401 por jti eliminado. `SeguridadTests.Login_Read_ForbiddenWrite_Logout_Revoked`; la prueba PostgreSQL confirma DELETE persistente. |
| Autorización denegada | Con un token válido de Investigador, GET /api/auth/usuarios. No intentar esta demostración con un administrador. | 403; consulta ordinaria continúa permitida. `SeguridadTests.Login_Read_ForbiddenWrite_Logout_Revoked` y `SeguridadTests.RoleMatrix`. |

Para demostrar caducidad por paso del tiempo en un navegador real, esperar a la expiración del token en una sesión de prueba y repetir la operación. Esa comprobación manual no se realizó durante esta auditoría y no cabe asumirla equivalente al test determinista. No reducir la duración de tokens de una instalación real para la sustentación.

## Ubicación exacta de las seis piezas de seguridad

| Pregunta del profesor | Archivo y método reales |
|---|---|
| ¿Dónde se genera el hash? | `MIRA.Api/Servicios/Seguridad/AuthService.cs`, clase PasswordService, método Hash: BCrypt.Net.BCrypt.HashPassword(password, 12). Crear, Bootstrap, Restablecer y RestablecerAdmin coordinan su uso. |
| ¿Dónde se verifica? | Mismo archivo, PasswordService.Verify: BCrypt.Net.BCrypt.Verify; invocado por AuthService.Login. No se vuelve a hashear para comparar. |
| ¿Dónde nace el JWT? | Mismo archivo, AuthService.GenerarJwt: claims, SigningCredentials, JwtSecurityToken y WriteToken; antes registra el jti. |
| ¿Dónde se valida? | JwtSettings.Validation, en ese archivo, define parámetros; `MIRA.Api/Configuracion/SeguridadConfiguration.cs::AddSeguridad` los conecta a AddJwtBearer y OnTokenValidated. |
| ¿Dónde están los roles de base? | `MIRA.Api/Repositorios/Seguridad/CuentaRepository.cs`, constante Columns y métodos Buscar/Sesion: JOIN usuario_rol → rol con r.activo. Roles lista el catálogo activo. |
| ¿Dónde se autoriza? | `SeguridadConfiguration.AddSeguridad`, política Datos, y atributos Authorize(Roles="Administrador") de `MIRA.Api/Peticiones/AuthController.cs`. `AuthController.Me` lee los claims ya revalidados. |

## Comprobaciones manuales pendientes de interfaz

El snapshot publicado comprueba 50 combinaciones y 18 revisiones axe, sin infracciones y con diez instancias incomplete. La propuesta local del 7 de noviembre vuelve a comprobar 50/18 con cero infracciones y cero incomplete, más 27 comprobaciones de zoom real (nueve páginas al 100/200/400 %). Las evidencias se conservan separadas. Ninguno de estos tamaños de Edge sustituye dispositivos reales.

- Navegar con NVDA/VoiceOver: encabezados, etiquetas, tablas, errores de formulario, avisos y anuncios de sesión expirada.
- Narrator está instalado, pero no se obtuvo un recorrido asistido verificable en esta sesión de navegador headless. Debe completarlo un operador con interacción visual/audio; no se declara prueba de lector aprobada.
- Repetir con tecnología asistiva los seis modales ya probados automáticamente por teclado: nombre, Enter, Tab/Shift+Tab, Escape y retorno de foco. Tabla desplazable, foco de validación y detalle también pasan; esto no sustituye la lectura asistida.
- Repetir tareas completas con zoom en otros navegadores, textos largos y fichas con varios semilleros. La nueva propuesta sí comprobó zoom real de página Edge al 200/400 %, además del zoom visual/reflow anteriores; no certifica todos los navegadores o escenarios.
- Probar Safari/iOS y Chrome/Android reales, orientación horizontal, teclado virtual y áreas táctiles.
- Revisar visualmente y con tecnología asistiva los componentes corregidos: las diez instancias automáticas se resolvieron localmente sin excluir reglas. El PR publicado sigue mostrando su snapshot anterior hasta autorizar los cambios. No declarar conformidad AA a partir de cero infracciones/incomplete.
- Verificar vencimiento por tiempo real y cookie Secure sobre HTTPS en staging autorizado. La sesión ya sobrevivió un reinicio real de Gunicorn con volumen local; falta redeploy/disco Render real.

## Recorrido por archivos

1. `MIRA.sln`, `Program.cs`, `Configuracion/DbConnectionFactory.cs`: composición y dependencias. Controlador → servicio → repositorio, sin SQL en controladores/servicios.
2. `Peticiones/AuthController.cs::Login`: contrato correo/password, 200 o 401, JSON por MVC.
3. `Servicios/Seguridad/AuthService.cs::Login`: normaliza correo; repository.Buscar; passwords.Verify; estado y rol; GenerarJwt.
4. Mismo archivo, `PasswordService.Hash/Verify`: abrir la llamada real a BCrypt.Verify y coste 12. Explicar por qué no se guarda texto plano ni se genera otro hash para comparar.
5. Mismo archivo, `AuthService.GenerarJwt`: claims, jti, firma, expiración y registro de sesión antes de responder.
6. `Configuracion/SeguridadConfiguration.cs::AddSeguridad`: ver validación de firma/emisor/audiencia y política Datos. `AuthController` añade la restricción Administrador para cuentas.
7. `Repositorios/Seguridad/CuentaRepository.cs::Sesion/Revocar`; migración 008: enseñar cómo se revoca realmente el JWT.
8. `frontend/seguridad.py::configure_session/login/protect_routes`: JWT en servidor; cookie opaca; rotación y expiración. `frontend/app.py::verify_csrf/api_call`: CSRF y Bearer.
9. `database/migrations/003_v2_usuarios_roles.sql`: `sp_usuario_crear` y `fn_usuario_crear` realizan maestro-detalle atómicamente. `sp_v3_usuario_actualizar` añade revocación sin reescribir V2.
10. `database/migrations/005_v2_area_aplicacion_trigger.sql`: `fn_area_aplicacion_normalizar`, trigger que recorta nombre en PostgreSQL. Crear un área con espacios y consultar el resultado persistido.
11. `database/migrations/006_v2_modelo_oficial.sql`: `mira_validar_grupo_activo_semillero` y trigger; `007_v2_rutinas_investigacion.sql`: rutinas de ficha/grupo. `InvestigacionIntegrationTests` demuestra rollback de la ficha completa ante un detalle inválido.
12. Abrir interfaz a 1440 y 390 píxeles: login, menú móvil, tabla desplazable, formulario, foco con Tab y modales. No declarar accesibilidad certificada por una captura.

## Demostración HTTP en Swagger

Swagger en `/swagger` (Development). POST `/api/auth/login` devuelve token; copiarlo temporalmente a Authorize/Bearer, sin fotografiarlo ni guardarlo en documentación.

- Sin Authorize, GET `/api/universidad`: 401.
- Login erróneo: 401 con mensaje uniforme.
- Login Investigador y Authorize: GET catálogo devuelve 200; POST catálogo o GET `/api/auth/usuarios` devuelve 403.
- Login Coordinador: crear/modificar devuelve 201/200; DELETE devuelve 403.
- Login Administrador: puede administrar cuentas y eliminar conforme a integridad referencial.
- POST logout y repetir GET con el mismo token: 401, porque se elimina jti.
- Un 409 por referencia o duplicado no equivale a error de credenciales. El frontend muestra texto comprensible.

## Simulacro de preguntas

| Pregunta | Respuesta conectada al código |
|---|---|
| ¿Qué es JWT? | Token con claims firmados, no cifrado. `AuthService.GenerarJwt` construye JwtSecurityToken; no incluir información secreta en claims. |
| ¿Quién verifica la firma? | El middleware JwtBearer registrado en `SeguridadConfiguration.AddSeguridad`, usando `JwtSettings.Validation`. |
| ¿Cómo expira? | exp en JWT y expira_en en PostgreSQL. `protect_routes` también comprueba expires absoluto. No hay renovación automática. |
| ¿Logout invalida el token? | Sí, para futuras solicitudes: `AuthService.Revocar` llama CuentaRepository.Revocar, que elimina sesion_acceso; OnTokenValidated rechaza jti ausente. |
| ¿Para qué sirve bcrypt? | Sal y coste encarecen ataques offline. `PasswordService.Hash` utiliza coste 12 y `Verify` valida el hash, sin implementar criptografía propia. |
| ¿200, 401 o 403? | 200 operación exitosa; 401 falta identidad válida; 403 identidad válida sin permiso. Abrir AuthController.Login y SeguridadTests.RoleMatrix. Crear devuelve 201. |
| ¿Qué es serialización? | MVC transforma SesionRespuesta a JSON en AuthController.Login con Ok(result); requests y response.json en app.py realizan la deserialización en Flask. |
| ¿Cómo evita SQL injection? | CuentaRepository pasa parámetros Dapper; los catálogos ejecutan rutinas parametrizadas. InvestigacionService limita recursos y campos a una lista explícita. |
| ¿Qué hace cada capa? | AuthController maneja HTTP, AuthService autentica y coordina, CuentaRepository ejecuta SQL. DbConnectionFactory crea conexiones. |
| ¿Qué es un trigger? | Función automática por evento de tabla; `tr_area_aplicacion_normalizar` en 005 normaliza INSERT/UPDATE incluso fuera de la API. |
| ¿Qué hace un procedimiento? | `sp_usuario_crear` en 003 ejecuta maestro y roles dentro de la operación PostgreSQL; error de FK revierte ambos. Ver database/tests/v2_usuarios_roles.sql. |
| ¿Por qué hay usuario y usuarios? | Dos modelos heredados: credenciales 003 y personas 006. No se fusionan IDs ni se crean docentes al crear cuentas; ver V3_ARQUITECTURA. |
| ¿Es suficiente esconder un botón? | No. `Datos` controla operaciones en API y Authorize protege cuentas. El frontend solo facilita navegación coherente. |
| ¿Por qué no React Icons? | El frontend es Jinja. La macro includes/icons.html usa SVG Tabler sin runtime React, conservando arquitectura y licencia. |
| ¿Dónde se manejan los errores? | ApiExceptionMiddleware.InvokeAsync traduce dominio/SQL; app.py api_call gestiona 401/403 y error_message traduce JSON a mensajes. |
| ¿Puede funcionar sin PostgreSQL? | El health verifica proceso; login y validación de sesión necesitan PostgreSQL. Se falla de forma cerrada; no se concede acceso si el repositorio falla. |
| ¿Qué queda para producción? | HTTPS, secretos externos, almacenamiento compartido si hay réplicas, pruebas del entorno real y evaluación completa de accesibilidad. La entrega no despliega infraestructura. |

No mostrar `.env`, `.local/*`, passwords, hashes o tokens durante la sustentación. Abrir los archivos de ejemplo y los resultados saneados de docs.

## Hoja verificable de los seis casos — cierre local

Las peticiones siguientes usan exclusivamente una cuenta y base de demostración ficticias. No ejecutar la demostración contra la instalación habitual o datos reales. Preparar el entorno antes del cronómetro; `verify-postgres16.ps1 -Browser -Zoom` reproduce pruebas aisladas y detiene sus procesos al terminar, por lo que no deja una sesión interactiva abierta. Para una demostración interactiva, preparar y comprobar por separado una instalación de pruebas con configuración privada propia.

Abreviaturas de archivo: AC=`MIRA.Api/Peticiones/AuthController.cs`; AS=`MIRA.Api/Servicios/Seguridad/AuthService.cs`; SC=`MIRA.Api/Configuracion/SeguridadConfiguration.cs`; CR=`MIRA.Api/Repositorios/Seguridad/CuentaRepository.cs`. TS=`tests/MIRA.Api.Tests/SeguridadTests.cs`; TD=`tests/MIRA.Api.Tests/SeguridadDatabaseTests.cs`.

| Escenario | Petición y endpoint | Respuesta esperada | Archivo / clase / método | Prueba y explicación para el profesor |
|---|---|---|---|---|
| Autenticación correcta | POST /api/auth/login, JSON correo/password ficticios válidos; GET /api/universidad con Bearer | 200, SesionRespuesta JSON; consulta 200 | AC AuthController.Login; AS AuthService.Login/GenerarJwt y PasswordService.Verify | TS.Login_Read_ForbiddenWrite_Logout_Revoked: verificación bcrypt, firma JWT y serialización MVC; no imprimir el token. |
| Contraseña incorrecta | POST /api/auth/login con correo existente y contraseña errónea no vacía | 401 uniforme | AC AuthController.Login; AS AuthService.Login/PasswordService.Verify | TS.BadLoginIs401. Un body inválido que dé 400 no demuestra este caso. |
| Sin token | GET /api/universidad sin Authorization | 401 con desafío Bearer | SC SeguridadConfiguration.AddSeguridad; Program MapControllers.RequireAuthorization | TS.MissingOrInvalidTokenIs401: el pipeline impide entrar al controlador de datos. |
| Token expirado | GET /api/auth/me con JWT firmado de exp pasado y clave Testing | 401 | AS JwtSettings.Validation; SC SeguridadConfiguration.AddSeguridad/AddJwtBearer | TS.InvalidJwtClaimsAre401(kind:expired). Es prueba determinista del middleware, no espera por reloj real ni cambio de clave productiva. |
| Sesión revocada | Login, GET con JWT → 200, POST /api/auth/logout, repetir GET con el mismo JWT | Logout 200; GET 401 antes de exp | AC AuthController.Logout; AS AuthService.Revocar/ValidarSesion; CR CuentaRepository.Revocar/Sesion | TS.Login_Read_ForbiddenWrite_Logout_Revoked y TD.RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration; jti ausente invalida acceso futuro. |
| Rol insuficiente | Investigador autenticado: GET /api/auth/usuarios | 403; el catálogo ordinario sigue 200 | AC AuthController.Usuarios y Authorize; SC SeguridadConfiguration.AddSeguridad | TS.RoleMatrix y EveryAdministrativeActionRequiresAdministrator: permiso real en backend, no solo botones ocultos. |

Los seis casos pueden recorrerse con el comando filtrado de SeguridadTests ya documentado. Mostrar solo estados/resultados y código; las claves/contraseñas de Testing son fixtures, no ejemplos para desplegar. El límite 429 se prueba aparte para no bloquear por IP el login de la demostración.

## Preguntas difíciles adicionales

| Pregunta | Respuesta verificable |
|---|---|
| ¿Todo nombre sp_ es un procedimiento? | No. En 003, sp_usuario_crear es CREATE PROCEDURE y CuentaRepository.Crear lo invoca con CALL. En 007, sp_guardar_v2_ficha_grupo es CREATE FUNCTION RETURNS jsonb, y InvestigacionRepository.GuardarFicha usa SELECT. El prefijo no define el tipo SQL. |
| ¿Dónde está la transacción de grupo-semilleros si no hay BeginTransaction en C#? | GuardarFicha realiza una única llamada a la función PL/pgSQL de 007. El error de un detalle revierte la sentencia y sus escrituras; SheetCreatesManyRowsThenRollsBackAllChangesWhenLastRowBelongsElsewhere lo prueba con PostgreSQL. No se promete agrupar varias llamadas HTTP independientes. |
| ¿JWT no debía ser stateless? | La firma/claims se validan sin reconstruir el token, pero este diseño añade una sesión jti y consulta roles/cuenta para revocar. AuthService.ValidarSesion y CuentaRepository.Sesion muestran el coste: dependencia de BD y una lectura por petición. |
| ¿Logout cancela una escritura que ya empezó? | No necesariamente: la petición que ya pasó autenticación puede terminar. La revocación impide solicitudes posteriores; no es un mecanismo de cancelación de transacciones en curso. |
| ¿Por qué cambiar el nombre del producto no cambia issuer, tablas o volúmenes? | Son contratos/estado persistente, no etiquetas de interfaz. Cambiar issuer/audience invalida JWT; cambiar Compose name puede crear otro volumen. El plan SAPIENS separa marca visible de identificadores internos y mantiene la historia MIRA. |
| ¿Por qué C06 no figura aprobado si el SQL pasa? | La prueba verifica que el modelo confirmado no tiene FK opcionales, no una relación nullable inexistente. La aceptación de esa no aplicabilidad requiere decisión del profesor. Ver V3_C06_JUSTIFICACION. |
| ¿Cero incomplete axe significa accesibilidad certificada? | No. Se corrigieron las causas automáticas concretas y se repitieron 18 auditorías; faltan lectura asistida y dispositivos reales. El zoom actual usa perfiles Chromium y comprueba DPR/viewport sin Emulation, pero tampoco prueba todos los navegadores. |
| ¿Por qué el workflow no tiene un check verde todavía? | Sigue local, sin commit/push autorizado. actionlint y componentes locales no son una ejecución de GitHub. Solo el run real del SHA publicado podrá validar el CI. |
| ¿12 caracteres son 72 bytes? | No. PasswordService.Hash usa string.Length de .NET y Encoding.UTF8.GetByteCount; los caracteres Unicode pueden ocupar varios bytes. El servidor aplica el máximo UTF-8 aunque el formulario también limite longitud. No es una medición de entropía ni de grafemas. |

En el tramo 06:00–07:30 abrir los procedimientos de 003, el trigger de normalización de 005 y la función/triggers de 007. Distinguir función, procedimiento, FK y trigger; usar el rollback real de las pruebas como evidencia de conservación V1/V2. No consumir los diez minutos instalando herramientas, creando recursos o esperando una expiración de veinte minutos.


## Cierre de estabilización y defensa de producción

Abrir `frontend/gunicorn.conf.py`: un worker, cuatro threads, bind PORT; logs sin query/body/token. Abrir `frontend/Dockerfile`: usuario mira, dependencias fijadas, instaladores retirados y CMD Gunicorn. `seguridad.configure_session` exige API HTTPS, secreto externo, cookie Secure y directorio absoluto en producción. Compose monta volumen; este perfil admite una instancia. `verify-docker.ps1` y docker-session-results.json demuestran reinicio/logout/revocación con datos ficticios.

Abrir `DbConnectionFactory` constructor y ProductionConfigurationTests: VerifyFull/Channel Binding Require/ErrorDetail false. `verify-production-smoke.py` confirma PORT=10000, health y Swagger deshabilitado con destinos ficticios. No demuestra una conexión Neon. `deploy/render.yaml.example` pasa JSON Schema, no fue aplicado y contempla recursos con coste.

Pregunta C06: citar literalmente las líneas 406/707 de PROMPT_MAESTRO_DEFINITIVO_MIRA_V2. La consulta SQL halló 22 columnas FK públicas, cero opcionales. El requisito es condicional; no se inventó una relación opcional ni se modificó el SQL docente. La aceptación académica corresponde al profesor. Un test de texto/fecha null no se presenta como prueba de FK nullable.

Pregunta sobre OSV: requests/urllib3/idna/certifi se actualizaron a 2.33.0/2.8.0/3.15/2026.10.18; OSV consultó las 18 distribuciones de la imagen Linux y devolvió cero avisos conocidos. Es una observación fechada, no una garantía absoluta ni una auditoría del SO completo. Instalar el lock en entorno limpio; el Python global antiguo no fue actualizado.
