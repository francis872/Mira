# SAPIENS Institutional Edition — revisión del 23 de noviembre de 2026

## Diagnóstico inicial comprobado

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

## Estado de la intervención

En ejecución. Este diagnóstico no certifica todavía la actualización ni el acceso con la contraseña inicial. Los resultados de código, pruebas, recuperación y despliegue se incorporarán después de verificarlos.
