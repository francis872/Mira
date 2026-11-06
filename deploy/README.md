# Preparación Render y Neon — sin aplicar

El ejemplo render.yaml.example se validó con el JSON Schema público de Render, sin API autenticada, despliegue ni comprobación de branch remoto. Dos servicios starter y disco 1 GB generarían coste si se crearan; no se aprovisionaron. autoDeployTrigger está off y no se ofrece una orden de despliegue automático.

## Configuración privada necesaria

API: Production, PORT=10000, Jwt__Key aleatoria externa >=32 bytes, issuer MIRA, audience MIRA.Frontend, minutos 20. ConnectionStrings__PostgreSql debe usar formato Npgsql, no DATABASE_URL:

```text
Host=<endpoint-neon>;Port=5432;Database=<base>;Username=<rol>;Password=<secreto>;SSL Mode=VerifyFull;Channel Binding=Require;Include Error Detail=false;GSS Encryption Mode=Disable
```

Los valores son placeholders; no copiarlos a appsettings, Git ni logs. GSSAPI se desactiva para el perfil TLS/SCRAM sin Kerberos; VerifyFull y Channel Binding permanecen requeridos. Confirmar CA/hostname, mecanismo de autenticación y funcionamiento real con Neon antes de aprobar.

Frontend: MIRA_ENV=production, PORT=10000, SECRET_KEY externa >=32 caracteres, API_URL=https://<api>/api, SESSION_COOKIE_SECURE=true, SESSION_DIRECTORY=/var/lib/mira/sessions, WEB_CONCURRENCY=1. Gunicorn atiende 0.0.0.0; Render termina HTTPS. No necesita habilitar Swagger productivo; SWAGGER_URL es opcional, vacío por defecto.

## Sesiones y privilegios

El directorio del disco debe pertenecer al UID 10001 y tener acceso privado (directorio 0700, archivos 0600). La imagen prepara el directorio, pero un disco montado puede cambiar sus permisos: comprobarlo antes de probar login. No configurar múltiples instancias o directorios efímeros independientes. Probar reinicio y redeploy real, capacidad, limpieza y pérdida controlada de sesión. El test Docker usa volumen local; no demuestra persistencia del disco Render.

La API usa UID 1654. Separar operador de migraciones/bootstrap y rol runtime con los privilegios necesarios para rutinas, sesiones y lecturas; no asumir que el rol SQL ya está restringido. Mantener DDL fuera del arranque de la API.

## Migraciones y operación pendientes

Respaldar y comprobar schema_migrations; revisar 001–008 y aplicar solo pendientes sobre una base de pruebas autorizada, con transacciones. Preferir conexión directa administrativa para DDL; comprobar pooler en las operaciones runtime. El Dockerfile API no incluye migraciones ni las ejecuta al iniciar. Bootstrap es CLI por variables privadas, nunca endpoint público ni contraseña en historial.

En staging autorizado comprobar: certificados/hostname y fallo con TLS inválido; login correcto/erróneo, sin token, expiración, revocación y 403; CSRF, cookie opaca Secure en HTTPS, disco/redeploy; arranque frío/caída de Neon, latencia, permisos, restauración de backup y carga. /health solo confirma vida del proceso. Revisar límites por IP y proxies conocidos, monitoreo y CDN Bootstrap. No hay evidencia remota de estos puntos todavía.

Fuentes: [Blueprint Render](https://render.com/docs/blueprint-spec), [Flask](https://render.com/docs/deploy-flask), [discos](https://render.com/docs/disks), [Neon/Npgsql](https://neon.com/docs/guides/dotnet-npgsql), [TLS Npgsql](https://www.npgsql.org/doc/security.html).
