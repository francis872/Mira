# Operación de Fase 6

[Arquitectura, configuración, migraciones, readiness y recuperación actuales](../FASE6_DESPLIEGUE.md). Frontend Vercel usa PostgreSQL compartido y firma del contenido; Docker/servidor persistente conserva el perfil de archivos opcional. El rol web solo accede a mira_web.sesion; el rol API tiene DML/EXECUTE de dominio y ningún DDL. Las migraciones 001–015 se administran fuera del arranque de servicios. /health y /ready tienen propósitos distintos. La autorización de Fase 6 permite operar únicamente los recursos nuevos identificados; no modifica datos remotos ajenos. El perfil previo de dos contenedores Render con disco, descrito abajo, es histórico y no es el seleccionado para esta fase gratuita.

# Manual técnico y preparación de operación

La composición se encuentra en Program.cs; usar Controller→Service→Repository y parámetros Dapper, sin SQL procedente de nombres enviados por el cliente. `RecursosInvestigacion` es la lista cerrada de recursos/campos/tipos. El prefijo de rutinas se elige de ese catálogo, no de texto arbitrario. `ApiExceptionMiddleware` traduce conflictos y oculta internos. Las pruebas de inyección almacenan texto como datos y verifican escape HTML; no equivalen a una auditoría exhaustiva de penetración.

Migraciones: consultar schema_migrations, respaldar antes, aplicar solo pendientes con ON_ERROR_STOP/transacciones. No reescribir 001–008, inventar FK opcionales ni aplicar cambios al Neon real. El usuario confirmó las fuentes SQL de 880ac75; 006 es incorporación aditiva y 007 sus rutinas. Una corrección de alcance nueva necesita migración posterior aprobada y prueba de reversión cuando sea viable. Restaurar una base sobre datos existentes requiere autorización operativa aparte.

## Configuración de producción inspeccionada, no desplegada

| Componente | Configuración necesaria y comprobación disponible |
|---|---|
| API | Production, PORT=10000; Jwt__Key externa ≥32 bytes; issuer MIRA/audience MIRA.Frontend; minutos 20; LoginProtection__ForwardingKey independiente |
| PostgreSQL | ConnectionStrings__PostgreSql privada Npgsql; SSL Mode=VerifyFull, Channel Binding=Require, Include Error Detail=false; permisos runtime/migración separados por verificar |
| Frontend | MIRA_ENV=production; API_URL HTTPS; SECRET_KEY externa; MIRA_LOGIN_FORWARDING_KEY igual al API; SESSION_COOKIE_SECURE=true; SESSION_DIRECTORY absoluta privada |
| Proxy | MIRA_TRUSTED_PROXY_NETWORKS vacía hasta identificar redes efectivas; nunca confiar en 0.0.0.0/0 o ::/0; validar cadena real antes de usar XFF |
| WSGI/sesión | Gunicorn un worker/cuatro threads; una instancia; volumen persistente escribible por UID 10001; directorio/archivos privados |
| Render ejemplo | Dos servicios Docker, branch feature/mira-v3, autoDeployTrigger off, health /health, disco declarado; secrets sync:false; no recursos creados |

Los Dockerfiles usan usuarios no root (API UID1654; frontend mira UID10001), COPY limitado y exclusión de privados. Las nuevas imágenes se construyeron y se inspeccionaron. El smoke Production local usa dominios example.invalid y loopback: confirma PORT/health/401/Swagger404/cookie, **sin contactar Neon/API remotos, sin TLS real ni login productivo**. `/health` es liveness, no evidencia de conexión o readiness de base.

Pendientes antes de producción: destino aislado autorizado, permisos reales, TLS/hostname/certificados, proxy efectivo, migraciones con datos representativos, pooler/transacciones, caída/arranque frío, disco tras redeploy, respaldo/restauración, rollback a imagen V3 segura, carga y límites por instancia. No volver automáticamente a una V2 pública como recuperación. No se cotizaron recursos actuales ni se asume gratuidad.

## CI propuesto y defecto confirmado

`.github/workflows/mira-v3-ci.yml`: quality prepara PG16, compila, SQL/.NET/Flask, checks de secretos, NuGet/OSV y Compose; containers construye e inspecciona imágenes y smoke. Acciones fijadas por SHA, contents:read, checkout sin persistir credenciales. No cubre actualmente HTTP E2E Docker ni navegador/axe/zoom, ni publica sus artefactos. No se ejecutó Actions para el SHA publicado.

El paso quality de Compose no define LOGIN_FORWARDING_KEY, exigida por docker-compose tras D01. Se reprodujo exit1 sin variable y exit0 con fixture pública nueva en un env privado ([evidencia](evidencias/ci-config.json)). Propuesta mínima, **no aplicada**:

```diff
           JWT_KEY: ci-only-signing-fixture-at-least-32-bytes-not-for-deploy
+          LOGIN_FORWARDING_KEY: ci-only-forwarding-fixture-at-least-32-bytes-not-for-deploy
```

No usar esa fixture en ningún entorno real. Se solicita autorización para corregir el workflow y repetir el gate; no se modificó por inferir permiso para nuevas correcciones. Un lint histórico correcto no demuestra que el job vaya a aprobar ahora.

## Diagnóstico sin secretos

Usar los reportes saneados de este directorio. Los logs, config.env, sesiones y reportes crudos quedan en .local, fuera del paquete público. No imprimir env completos ni cadenas de conexión. OSV cubrió 18 distribuciones de la imagen; NuGet ambos proyectos/transitivos, sin avisos reportados por esas fuentes a esta fecha. No se auditó exhaustivamente el SO base ni todas las imágenes/capas/historia Git o servicios externos.

La política de rate limiting es de memoria por instancia. El filesystem de sesión requiere confianza local. Bootstrap y reset son operaciones privilegiadas CLI; no exponerlas como endpoints. JWT no contiene secreto pero es una credencial portadora. Revisar rotación de claves y caducidad antes de cualquier operación real.
