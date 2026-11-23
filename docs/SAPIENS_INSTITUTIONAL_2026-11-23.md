# SAPIENS Institutional Edition — revisión del 23 de noviembre de 2026

## A. Diagnóstico inicial comprobado

Repositorio `francis872/Mira`, rama `feature/mira-v3`, árbol limpio al iniciar, HEAD `da90f3a`. Procede de MIRA V1–V3; SAPIENS es la marca pública. API ASP.NET Core/.NET 10, Dapper y Npgsql sobre PostgreSQL 16; frontend Flask/Gunicorn, Jinja y Bootstrap 5.3.3. Autenticación bcrypt coste 12, JWT firmado de veinte minutos, roles Administrador/Coordinador/Investigador y sesiones opacas del navegador almacenadas en Neon. No usa ASP.NET Core Identity.

La auditoría de Render confirmó el servicio oficial **`srv-dbtp70tckfvc7393jn5g`**, URL **https://sapiens-usb.onrender.com/login**, Docker con raíz `frontend`, plan Free y fuente `541a459`. Su API existente es `srv-dbr6i3dckfvc73dc3hr0`, fuente `5b91bf5`. Son dos servicios del mismo aplicativo, no dos aplicaciones nuevas. La actualización de la API requiere aclarar el límite del adjunto que pide actualizar únicamente el servicio oficial; se solicitó autorización antes de desplegarla.

Problemas encontrados antes de modificar código:

- Registro abierto en `frontend/seguridad.py::registro`, `AuthController.Registrar` y `AuthService.Registrar`; pestañas, enlaces y confirmación en `login.html`.
- Tema oscuro con índigo/turquesa y gráfico decorativo, contrario a la nueva identidad monocromática solicitada.
- El login no bloquea doble envío ni informa su procesamiento; la protección existente de otros formularios no cubre esta pantalla.
- Ejemplo de despliegue Render apunta al frontend anterior; no se usará para crear recursos ni sincronizar configuración.
- La cuenta **sapiens@gmail.com** tiene exactamente una coincidencia normalizada, está activa y guarda un hash bcrypt válido con rol **Investigador**. No existe MAESTRO en los roles de acceso. No hay persona académica `usuarios` con ese correo; no se inventará una vinculación.
- No existe `SAPIENS_INITIAL_PASSWORD` configurada en ninguno de los dos servicios. Se pidió al propietario establecerla privadamente con la contraseña vigente, o indicar un restablecimiento administrativo explícito. No se generará una contraseña ficticia ni se restablecerá al arrancar.

Se capturaron en lectura el estado de despliegue, configuración privada y conteos/huellas de **25 tablas académicas**, además de las quince migraciones aplicadas. La sesión OAuth de Render había expirado; se renovó con la CLI oficial. No se modificaron recursos, usuarios ni datos durante esta auditoría.

## B. Acceso institucional y autorización

Retirados `AuthController.Registrar`, `RegistroRequest`, `AuthService.Registrar`, la ruta Flask `/registro`, sus pestañas/enlaces y las pruebas del registro abierto, sustituidas por pruebas de cierre institucional. No se ocultaron mediante CSS: las rutas antiguas no existen y las solicitudes directas no crean cuentas. `POST /api/auth/usuarios` conserva autorización Administrador. Se mantienen login, bcrypt, JWT, roles, sesiones y funciones académicas.

## C. Cuenta preconfigurada y aprovisionamiento

El propietario autorizó actualizar también la API existente para cerrar su endpoint directo y proporcionó la credencial vigente. Se comprobó privadamente que coincide con el hash existente; **no se restableció, duplicó, reactivó ni elevó la cuenta**. Render `sapiens-usb` contiene ahora `SAPIENS_INITIAL_EMAIL`, `SAPIENS_INITIAL_PASSWORD` privada y `SAPIENS_INITIAL_ROLE=Investigador`; todas las variables anteriores se conservaron. No hay contraseña en Git, ejemplos, capturas o este informe.

`AuthService.ProvisionarInstitucional` y `scripts/provision-institutional.ps1` permiten una operación explícita del operador. La búsqueda normaliza correo, reutiliza ID y hash si coinciden, y crea cuenta/rol atómicamente si no existe. Roles incompatibles o secreto incorrecto fallan sin cambios. `--reset-institutional-password` es una operación separada y explícita que revoca sesiones; nunca se ejecuta al arrancar. No se añadió un endpoint de aprovisionamiento anónimo ni un rol MAESTRO ficticio. [Procedimiento administrativo completo](SAPIENS_ADMINISTRACION.md).

## D. Diseño y componentes

Sustituido el tema índigo/turquesa por `sapiens-institutional.css`: lienzo claro, paneles blancos, barra lateral negra, bordes discretos y botones negros. Tipografía de sistema consistente, sin descargas de fuentes ni nuevos bundles. Login con texto institucional, campos vacíos, contraseña visible opcionalmente, foco y procesamiento accesible; sin ilustración ni invitación al registro. Se revisaron composición, tipografía, componentes, navegación, puntos de ruptura y estados antes de seleccionar las capturas finales.

Se modernizaron inicio, tarjetas, tablas, formularios, mensajes y diálogos sin retirar columnas, operaciones o módulos. Se corrigió contraste en acciones de tablas y cierre de modales. La revisión automática final no conservó infracciones ni `incomplete` de contraste; no equivale a una certificación humana de accesibilidad.

## E. Navegación y experiencia

Navegación existente conservada, estado activo y menú móvil; ninguna ruta académica duplicada. La búsqueda mantiene su filtro en la URL y al recargar, sin petición adicional por pulsación. Los formularios POST admiten protección de doble envío independientemente de las mayúsculas del atributo `method`. Login anuncia procesamiento y bloquea el segundo envío; `pageshow` permite volver a utilizar el formulario tras regresar. Se respetan las preferencias de movimiento reducido. Se conserva la arquitectura de páginas del servidor y el arranque asistido de Render.

## F. Base de datos y recuperación

**Cero migraciones nuevas y cero escrituras académicas de prueba en producción.** Las quince migraciones aplicadas permanecen iguales. Se compararon conteos y huellas de las 25 tablas académicas: **511 registros**, coincidentes antes/después de API y frontend. La identidad, estado y hash institucional también coinciden.

Se creó un respaldo completo privado con `pg_dump` **16.16**, SHA-256 `80ce3aa07d4576a1bfbb46a8cd67f8f5e7617dacae55cdf9611c8241cdd4bb18`, y se restauró únicamente en PostgreSQL local exclusivo. Coinciden las 25 tablas/511 registros y se preserva la cuenta institucional. No se restauró ni se creó una base en Neon.

El primer intento con clientes 16.15 falló al restaurar por `pg_attver`, metadato interno del origen actualizado. Se conservaron ese archivo y diagnóstico; se obtuvo un respaldo nuevo con el cliente oficial 16.16, sin editar ni filtrar SQL del dump. La documentación de PostgreSQL describe esa incompatibilidad entre revisiones y el tratamiento interno del atributo. [Cambio oficial de versionado de atributos](https://www.postgresql.org/message-id/E1xQQG1-000000003jI-2cdw%40gemulon.postgresql.org), [versión 16.16](https://www.postgresql.org/docs/release/16.16/), [binarios de EDB enlazados por PostgreSQL](https://www.postgresql.org/download/windows/).

## G. Seguridad

Conservados bcrypt coste 12, JWT de veinte minutos, validación de firma/emisor/audiencia/expiración, consulta de estado/roles en backend, revocación y CSRF. Mismos SECRET_KEY, clave JWT, HMAC, cookies Secure/HttpOnly/SameSite Strict, perfil Cloudflare y conexiones TLS verificadas. El JWT sigue en el servidor. La contraseña inicial configurada no es un acceso alternativo al hash PostgreSQL.

Se revisaron archivos publicables y valores privados conocidos, incluida la credencial suministrada: sin coincidencias. El literal de `InstitutionalAccessTests.cs` es una credencial aislada de test, revisada expresamente. Secreto, volcados, conexiones, cookies y logs completos permanecen fuera de Git. OSV consultó los 25 paquetes Python fijados y no informó avisos conocidos; no es auditoría del sistema operativo ni de todas las imágenes.

## H. Pruebas verificadas

| Verificación | Resultado y alcance |
|---|---|
| Compilación Release | `dotnet build MIRA.sln -c Release --no-restore`: aprobada |
| API/SQL | **156/156**, PostgreSQL 16 aislado, sin fallos ni omisiones |
| Flask/sesiones | **182/182**, incluida base local de sesiones; sin omisiones |
| E2E HTTP V1/V2/V3 | **51/51**, únicamente datos aislados locales |
| Vistas responsive | Siete vistas × cinco anchos: **35 combinaciones**, más dos muestras repetidas para capturas, tanto localmente como en la URL pública |
| Accesibilidad automática | Axe 4.10.3 sobre siete vistas distintas, más repetición del login: sin infracciones ni incomplete; teclado, foco y diálogos comprobados |
| Zoom real | Edge 100 % y 200 %, ventana externa 1440, viewport 1416 → 708, DPR 1 → 2, escala visual 1; acceso/inicio/tabla sin desbordamiento |
| API pública | **13 comprobaciones**: alta pública 404, alta administrativa anónima 401, login institucional 200, incorrecto/sin token/revocado 401, administración 403, catálogo disponible |
| Web oficial | **13 comprobaciones**: salud/readiness/login/HEAD 200, rutas antiguas GET y POST 404, permisos 403, salida y cierre del inicio protegido |
| Continuidad | La misma cookie/SID de la sesión previa siguió abriendo el inicio después de reemplazar ambos servicios |
| Datos y cuenta | Las 25 tablas/511 registros, migraciones e identidad/hash institucional permanecieron iguales |
| Respaldo | Restauración local del archivo nuevo y comparación completa de tablas aprobadas |

La primera corrida sin base tuvo 74 .NET aprobadas/37 omitidas y 177 Flask aprobadas/cinco omitidas. Se distinguen de las pasadas integradas completas. El primer recorrido visual encontró contraste insuficiente en acciones de tabla; se corrigió y repitió. Otro intento falló por esperar exactamente el texto «Cerrar» en el instrumento, cuando el control ya se llamaba «Cerrar confirmación»: se corrigió el selector y se comprobaron foco/Escape. No se presentan esos intentos iniciales como aprobados.

[Evidencia saneada, trazabilidad y capturas](evidencias/institutional-20261123/verificacion.json), [pruebas y reproducción](V3_PRUEBAS.md).

## I. Render y secuencia controlada

Publicado el mismo commit **`250b46129ec46c9fa430e955441f33bfac9a07e9`**, después de pruebas y revisión de secretos:

| Componente existente | Servicio | Despliegue confirmado `live` |
|---|---|---|
| API | `srv-dbr6i3dckfvc73dc3hr0` | `dep-dc29m888v5cc73c0g900` |
| Frontend oficial | `srv-dbtp70tckfvc7393jn5g` | `dep-dc29nf5g1s2s73dq4mv0` |

Se publicó primero la API; solo tras aprobar sus pruebas remotas se publicó la web. Render y Neon conservaron Free. Ningún servicio, dominio o base nuevos; sin renombrar recursos, sin actualizar el frontend antiguo paralelo y sin cambiar las conexiones existentes. El ejemplo Render ahora identifica `sapiens-usb`; no se sincronizó ningún Blueprint.

Los despliegues anteriores quedaron identificados para recuperación. No fue necesario activarla: ambos servicios pasaron verificaciones. La guía conserva la precaución de mantener cerrada la API de registro durante cualquier recuperación del frontend.

## J. URL y acceso real

**https://sapiens-usb.onrender.com/login permanece exactamente igual.** GET/HEAD público aprobado, pantalla nueva y recursos estáticos cargados, sin enlaces de registro ni credenciales prellenadas. Acceso real de `sapiens@gmail.com` aprobado, con rol Investigador; salida y denegación de gestión de cuentas/escritura comprobadas. También se verificó el acceso administrativo existente para revisar las vistas de formulario, sin modificar registros académicos de producción.

## K. Pendientes y dictamen

- Faltan pruebas humanas con lector de pantalla, dispositivos físicos, Safari/iOS y gestores de contraseñas/teclados virtuales. La validación automática y emulada no acredita todas esas plataformas.
- No se repitió el ensayo completo de suspensión de quince minutos en esta revisión. Render Free puede suspender instancias y compartir cuotas; no se promete cero interrupciones o disponibilidad institucional continua.
- C06 y las decisiones académicas V2 anteriores siguen pendientes de su revisión docente; esta actualización no modifica esas reglas. El PR #7 conserva Draft, cero revisiones formales y ningún merge. Actions sigue excluido por instrucción del usuario; las pruebas descritas se ejecutaron fuera de Actions.

**La actualización institucional está desplegada y el acceso del profesor está probado, sin registro ni provisión adicional para la cuenta existente.** La aceptación académica integral, la revisión asistiva humana y una certificación de operación institucional continua conservan sus pendientes. No se incluye la contraseña en esta entrega.
