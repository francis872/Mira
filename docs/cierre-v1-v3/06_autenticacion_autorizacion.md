# Autenticación, autorización y sesiones

| Operación exacta | Archivo :: símbolo |
|---|---|
| Generar bcrypt | `MIRA.Api/Servicios/Seguridad/AuthService.cs::PasswordService.Hash`, coste 12; Crear, Restablecer, Bootstrap y recuperación CLI lo llaman |
| Verificar bcrypt | Mismo archivo, `PasswordService.Verify`, llamado por `AuthService.Login`; hash dummy para credencial inexistente/inválida |
| Emitir JWT | `AuthService.GenerarJwt`: HS256; iss MIRA, aud MIRA.Frontend; sub/email/role/iat/exp/jti; registra sesión antes de responder |
| Validar firma/claims | `JwtSettings.Validation`: RequireSignedTokens, algoritmo HS256, firma, issuer, audience, lifetime, exp obligatorio, ClockSkew cero |
| Validar estado persistente | `MIRA.Api/Configuracion/SeguridadConfiguration.cs::AddSeguridad`, JwtBearer.OnTokenValidated → `AuthService.ValidarSesion` |
| Consultar roles | `CuentaRepository.Columns`, Buscar/Sesion: subconsulta usuario_rol JOIN rol activo; Roles lista rol activo |
| Revocar | `AuthService.Revocar` → `CuentaRepository.Revocar`: DELETE jti/usuario; cambio de cuenta/password revoca por procedimientos 008 |
| Autorizar | Política Datos en AddSeguridad y atributos `Authorize(Roles="Administrador")` de AuthController |

Contraseñas de alta/restablecimiento: mínimo 12 caracteres de .NET y máximo 72 bytes UTF-8. No se equiparan bytes y caracteres ni se llama “cifrado” al hash irreversible. El JWT está firmado, su payload es legible; no debe contener secretos. Duración por defecto 20 minutos, configurable 1–60, sin refresh.

| Rol | Leer datos | Crear/editar | Eliminar | Cuentas/roles |
|---|---|---|---|---|
| Investigador | Sí | No | No | No |
| Coordinador | Sí | Sí | No | No |
| Administrador | Sí | Sí | Sí | Sí |

Roles desconocidos no otorgan acceso. En cada petición se consulta jti, cuenta activa, correo y conjunto actual de roles: un cambio invalida el token anterior y requiere login. No basta con que un JWT esté correctamente firmado. No hay permisos por propiedad de docente, grupo o proyecto; no se afirma autoservicio docente. No existe alta pública ni recuperación pública de contraseña.

Flask: `seguridad.py::configure_session/register_security`, `app.py::verify_csrf/api_call`. CacheLib conserva JWT/identidad en archivos del servidor; cookie opaca HttpOnly/SameSite Strict, Secure en producción, rotación tras login, expiración absoluta y CSRF para todos los POST. El JWT no se almacena en localStorage, sessionStorage o cookie legible del navegador. El directorio de sesiones debe ser privado y persistente para una instancia; no se validó escalado horizontal.

## D01 corregido y verificado de nuevo

La cuota anterior veía la IP del intermediario Flask. `frontend/login_protection.py::LoginOriginAuth` firma la IP validada, timestamp, método, ruta y digest del cuerpo realmente enviado. `LoginProtectionMiddleware` valida HMAC en tiempo constante, ventana ±60 s, cuerpo máximo 8192 bytes y firma antes de aplicar cuotas. La clave compartida es independiente de JWT/SECRET_KEY. La IP procede del socket; X-Forwarded-For solo se acepta en cadenas de proxies CIDR configurados explícitamente. X-Real-IP y cabeceras arbitrarias no cambian identidad.

Límites actuales: diez intentos/minuto por origen autenticado y diez por cuenta normalizada; cuatro verificaciones concurrentes sin cola. La conexión directa sin firma usa IP TCP. Una cabecera de origen firmada inválida falla 401; cuerpo excesivo 413. No se aumentaron cuotas para ocultar el fallo.

[Prueba integrada actual](evidencias/d01-sesiones.json): atacante 10×401, siguiente 429; primer login de otro origen 302; nuevas cookies/cabeceras falsas no reinician cuota del origen agotado. Se repitieron los 19 casos .NET y 18 Flask añadidos en Fase 3. Esto verifica clientes Flask sintéticos con REMOTE_ADDR distintos y HTTP real a API/PG aislados; no valida todavía los proxies reales de Render.

Riesgos: NAT puede compartir cuota, cuenta agotada sigue afectando esa cuenta, estado de rate limiting es por instancia y se reinicia con API. Escalado requeriría diseño compartido autorizado. La firma no establece TLS ni vuelve confiable cualquier proxy.

## Logout degradado: resultado observado

Con API caída se elimina la sesión local y se muestra advertencia. Tras reiniciar API, **el mismo JWT todavía respondió 200**. Al revocarlo explícitamente, respondió 401. Esto no es revocación remota confirmada durante la caída. El riesgo dura hasta exp o revocación posterior y debe explicarse en la sustentación. No se añadió cola de revocación ni renovación automática.

Las pruebas de expiración usan tokens deterministas de Testing con exp pasado y validación real JwtBearer. No se alteró el reloj ni se expuso una clave productiva. Un ensayo de esperar el vencimiento real completo permanece pendiente. [Casos y resultados](evidencias/casos.json), [guía](10_pruebas.md).
