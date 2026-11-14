# Plan operativo Render/Neon — entorno de pruebas

Propuesta del 2026-11-07. No aplicada. No se crearon recursos, costes remotos ni migraciones remotas. Requiere autorización explícita antes de ejecutarse. La preparación local no valida producción.

## Recursos y presupuesto por aprobar

Dos servicios Docker Render (API y frontend) y un disco persistente privado de 1 GB para sesiones del frontend; una base/proyecto o branch Neon **exclusiva de pruebas con datos ficticios**, región acordada y roles de operador/runtime. El ejemplo `deploy/render.yaml.example` usa starter, una instancia y autoDeployTrigger off. No trasladar el Compose local ni su PostgreSQL a Render sin adaptar el procedimiento.

Presupuesto estimable: `2 × coste del servicio elegido + coste de 1 GB de disco + consumo/plan Neon + tráfico/builds/otros cargos aplicables`. No se fija una cifra que no se haya cotizado. Confirmar [tarifas Render](https://render.com/pricing), [discos](https://render.com/docs/disks) y [tarifas Neon](https://neon.com/pricing), límites, impuestos y duración antes de crear nada. El disco requiere servicio de pago y limita el frontend a una instancia; el filesystem ordinario es efímero.

## Procedimiento propuesto

1. **Inventario y autorización:** acordar nombres de pruebas, región, presupuesto, responsables, alcance SQL y ventana. Confirmar que ninguna URL/credencial corresponde a producción; mantener auto-deploy apagado.
2. **Versión y respaldo:** registrar SHA/digests de las imágenes aprobadas, schema_migrations y configuración privada. Comprobar un backup/restauración en destino aislado antes de modificar una base existente de pruebas. Definir RPO/RTO y criterios de aborto; no generar ni publicar dumps de datos reales.
3. **Neon:** crear el destino solo tras autorización, sin copiar datos institucionales. Conexión directa administrativa para DDL y bootstrap; rol runtime con privilegios comprobados para rutinas, sesiones y lecturas. Validar pooler por separado si se usa. La documentación no prueba que un rol remoto esté ya restringido.
4. **Migraciones:** revisar 001–008 y schema_migrations; aplicar únicamente pendientes en orden, con ON_ERROR_STOP y transacción por archivo. Usar configuración privada libpq/PGPASSFILE o equivalente, sin contraseña en historial/argumentos. La API no ejecuta SQL al arrancar; no borrar registros de migraciones ni reescribir archivos históricos.
5. **API:** Dockerfile raíz; ASPNETCORE_ENVIRONMENT=Production; PORT=10000; Jwt__Key externa aleatoria >=32 bytes, issuer MIRA, audience MIRA.Frontend y minutos 20. Mantener estas identidades durante el cierre MIRA V3.
6. **TLS de PostgreSQL:** ConnectionStrings__PostgreSql en formato Npgsql con Host/Database/Username/Password privados, SSL Mode=VerifyFull, Channel Binding=Require, Include Error Detail=false y GSS Encryption Mode=Disable para TLS/SCRAM sin Kerberos. No usar TrustServerCertificate o deshabilitar validación para resolver una conexión fallida. Verificar certificados y hostname reales. [Neon/Npgsql](https://neon.com/docs/guides/dotnet-npgsql).
7. **Frontend:** contexto frontend y Gunicorn de la imagen; MIRA_ENV=production, PORT=10000, API_URL=https://<api>/api, SECRET_KEY externa estable >=32 caracteres, SESSION_COOKIE_SECURE=true, SESSION_DIRECTORY=/var/lib/mira/sessions y WEB_CONCURRENCY=1. Confirmar directorio privado escribible por UID 10001 (0700, archivos 0600) después del montaje. No crear réplicas con discos independientes. Swagger productivo no se habilita poniendo Development.
8. **Conectividad y HTTPS:** comprobar DNS, certificado/cadena/hostname, redirecciones del ingreso y cookie opaca Secure/HttpOnly/Strict sobre HTTPS. Probar un certificado inválido en pruebas y comprobar rechazo, conservando VerifyFull. /health es liveness: confirmar conectividad BD mediante operación autorizada, no inferirla del 200.
9. **Bootstrap y permisos:** por CLI con variables privadas, sin ruta pública ni contraseñas en capturas. Usar cuentas ficticias de los tres roles y ejecutar los seis casos de la guía, todas las acciones administrativas, CSRF (POST sin token → 400), CRUD, triggers y rollback. Proteger JWT/hash/credenciales en logs y reportes.
10. **Sesiones y fallos:** verificar login/expiración/logout, reinicio y redeploy con la misma sesión dentro de su vigencia; caída o disco vacío exige nuevo login. Medir arranque frío/caída de Neon, timeout, errores comprensibles y recuperación sin conceder acceso. Confirmar persistencia real del disco, carga, límites por IP y proxies conocidos.
11. **Acta de pruebas:** registrar SHA, servicios, resultados, incidentes, permisos, consumo y responsables. Solo después evaluar el paso siguiente; retirar o suspender recursos también requiere una decisión operativa explícita.

## Reversión y recuperación

- Detener pruebas y acceso al entorno afectado y conservar evidencia saneada. No usar un rollback como excusa para exponer endpoints V2 públicos.
- Volver únicamente a una imagen **V3 segura, compatible y previamente aprobada** con su configuración privada. Si esta es la primera versión V3 aprobable y no existe anterior segura, mantener el entorno suspendido hasta corregirla.
- No ejecutar down migrations destructivas, borrar 008 ni quitar schema_migrations. Restaurar un respaldo comprobado solo en destino aislado de pruebas y con aprobación del operador. Una migración compensatoria exige revisión independiente.
- Comprobar esquema, datos, HTTPS/TLS, roles, login y revocación antes de reabrir. Si hubo exposición de secretos, el operador debe revocarlos/rotarlos; un cambio de clave JWT invalida tokens y debe registrarse.
- Criterio de cierre: restauración ensayada, RPO/RTO registrados y seis escenarios de seguridad aprobados en el destino recuperado. Ninguno de esos resultados remotos se ha obtenido aún.

El Blueprint pasó JSON Schema en la revisión histórica del 5 de noviembre; eso no valida una cuenta, permisos, costes, dominio o branch remotos. [Referencia Render](https://render.com/docs/blueprint-spec). Los checks Production/Docker locales solo usan destinos ficticios.
