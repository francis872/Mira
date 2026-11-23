# Administración institucional de SAPIENS

Acceso oficial: **https://sapiens-usb.onrender.com/login**. Servicio web existente: `srv-dbtp70tckfvc7393jn5g`; API existente: `srv-dbr6i3dckfvc73dc3hr0`. Mantener nombres, URLs, base Neon y claves de sesión. Esta guía no contiene contraseñas.

## Configuración privada y cuenta inicial

En Render → **sapiens-usb → Environment**, el propietario configura `SAPIENS_INITIAL_EMAIL` con `sapiens@gmail.com`, `SAPIENS_INITIAL_PASSWORD` con su contraseña privada vigente y `SAPIENS_INITIAL_ROLE` con `Investigador`. No añadir el valor de contraseña a Git, ejemplos, comandos con argumentos, capturas ni informes. La cuenta ya existía activa con ese rol; no se duplicó ni se le concedió Administrador.

Estas variables son configuración operativa. **El login siempre verifica el hash en PostgreSQL**: no compara con la variable de entorno ni la usa como acceso alternativo. Cambiar una variable no cambia automáticamente la contraseña o el correo de una cuenta. El arranque normal no crea cuentas ni restablece contraseñas.

No existe MAESTRO en los roles de acceso actuales. Investigador conserva consulta de catálogos, investigación y reportes; Coordinador puede crear/modificar, y Administrador gestiona cuentas y retiros. No confundir `usuario` (acceso) con `usuarios`/`docentes` (dominio académico). Configurar el correo no crea una persona docente ni modifica sus relaciones.

## Aprovisionar o verificar sin duplicar

Un operador autorizado configura privadamente `ConnectionStrings__PostgreSql`, `Jwt__Key` y, si corresponde a Production, `LoginProtection__ForwardingKey` y TLS como en la API existente. Usar credenciales operativas de la instalación, nunca la conexión de sesiones `mira_web`. Para trabajar con producción debe seleccionar expresamente su conexión; para pruebas, una base desechable.

Desde la raíz del repositorio:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/provision-institutional.ps1
```

El script toma las variables `SAPIENS_INITIAL_*` del entorno privado o solicita la contraseña con entrada protegida. Ejecuta `--provision-institutional`, que normaliza el correo y consulta la cuenta. Si existe activa, con rol compatible y contraseña coincidente, conserva ID, hash, roles y sesiones. Si no existe, crea cuenta y rol en una operación SQL atómica con bcrypt coste 12. Si falta el secreto, el rol no existe, la cuenta está inactiva o sus permisos requieren revisión, falla sin reactivarla ni modificarla. No permite crear Administrador ni inventa MAESTRO.

Esta operación no tiene endpoint HTTP público y requiere acceso operativo a la base. Render Free no ofrece una consola de ejecución para asumir este paso: se ejecuta desde el entorno privado del operador, o se gestionan cuentas desde la administración autenticada de la aplicación.

## Comprobar y administrar usuarios

1. Entrar por `/login` con la cuenta institucional y comprobar el panel y la salida. Un Investigador no debe acceder a `/usuarios` ni a formularios de escritura.
2. Un Administrador entra a **Cuentas de acceso** (`/usuarios`) para revisar estado/roles y crear futuras cuentas autorizadas. El backend exige Administrador en `POST /api/auth/usuarios`; Investigador y Coordinador no pueden provisionar cuentas por esa vía.
3. Cambiar correo o roles desde administración conserva el ID e invalida las sesiones previas según las rutinas existentes. No renombrar cuentas cambiando únicamente las variables de Render.
4. Para restablecer una contraseña, usar la acción de contraseña en Cuentas de acceso. La API almacena un nuevo hash y revoca los JWT de esa identidad.
5. Alternativa operativa explícita para una cuenta institucional no administrativa: `scripts/provision-institutional.ps1 -ResetPassword`, con la nueva contraseña en el entorno privado o en la entrada protegida. Nunca ejecutarlo automáticamente al reiniciar.
6. Inactivar desde Cuentas de acceso bloquea nuevos accesos y revoca sesiones. No elimina las tablas académicas ni requiere borrar la identidad.
7. **Salir** llama a `POST /api/auth/logout`, revoca el JWT y borra la sesión local. Cambios críticos revocan los JWT; una cookie web anterior no evita la siguiente comprobación de la API. No borrar globalmente las sesiones de otras personas para cerrar una cuenta.

## Despliegue y recuperación

Antes de publicar: probar en una base aislada, compilar, revisar secretos, guardar el commit/despliegue actual y un respaldo privado compatible. No ejecutar migraciones destructivas ni sincronizar Blueprints de ejemplo. El producto tiene dos servicios existentes: bloquear altas directas requiere publicar la API además de la web, manteniendo sus recursos y URLs. El propietario autorizó ambos en esta revisión.

Publicar primero la API, esperar `live` y comprobar `/ready`, login y ausencia de `/api/auth/registro`. Publicar después el frontend oficial, esperar `live` y abrir **la misma URL `/login`**. Verificar acceso institucional, rutas antiguas cerradas, permisos, estáticos y continuidad de sesión. `build_in_progress` no acredita un despliegue correcto.

Si el frontend falla, mantener la API con el registro cerrado y recuperar el despliegue web anterior desde Render → Deploys; comprobar el login de nuevo. No restaurar la base cuando el fallo es solo de interfaz. El frontend anterior puede mostrar enlaces obsoletos, pero la API debe seguir rechazando altas. Para una falla de API, conservar el bloqueo de registro durante la reparación; volver sin revisión a la versión que tenía registro abierto reabriría el problema.

El respaldo se conserva fuera de Git. Para una emergencia de datos, probar primero su restauración en un destino aislado con herramientas compatibles y comparar registros antes de autorizar cualquier restauración sobre el origen. En esta actualización no se requieren migraciones ni restauración sobre Neon.

Render Free puede suspender instancias y compartir cuotas; no se garantiza cero interrupciones. [Informe, versiones y resultados de esta revisión](SAPIENS_INSTITUTIONAL_2026-11-23.md).
