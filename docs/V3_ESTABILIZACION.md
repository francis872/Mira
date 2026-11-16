# Estabilización y cierre V3

Fecha de inicio: 2026-11-05. Rama `feature/mira-v3`; cambios anteriores sin commit conservados. Mandato: corregir pendientes de la revisión, ejecutar verificaciones y conservar V1/V2. Sin commits, publicación, despliegue ni cambios de datos reales.

## Auditoría previa y plan priorizado

Se leyeron revisión final, pruebas, guía, README, Spec Kit V3 y configuración. La matriz R01–R30 sigue siendo la base de trazabilidad; contiene cobertura estática pendiente de pruebas dedicadas (roles desconocidos, límites de contraseña, redirecciones), observaciones de producción y revisiones manuales de interfaz.

1. **Dependencias:** requests 2.32.5, urllib3 2.6.3, idna 3.11 y certifi 2026.2.25 tienen ocho GHSA distintos. Seleccionar versiones corregidas dentro de sus familias compatibles, resolver/fijar transitivas y auditar el entorno resultante con OSV. No modificar Python global.
2. **Docker:** excluir .local, entornos, dumps, sesiones, logs, claves y resultados; reducir COPY a archivos necesarios, ejecutar sin root y probar builds/arranque si el motor está disponible. Estado inicial: cliente Docker 29.9.2 instalado; daemon no responde.
3. **WSGI/sesión:** incorporar Gunicorn para Linux; conservar Flask-Session/CacheLib con directorio privado explícito y persistente para una instancia. Documentar límite de réplica; no provisionar Redis/discos ni costes. Probar reinicio, borrado de sesión, expiración y cookies Secure mediante HTTPS simulado.
4. **Backend/producción:** cerrar pruebas faltantes de R01–R30, revisar bind PORT/entornos/TLS/Swagger. Conservar la validación JWT y RBAC. Preparar configuración revisable Render/Neon sin despliegue.
5. **C06:** contrastar literalmente con el prompt y el SQL oficial. No introducir una FK nullable sin fuente académica: comprobar catálogo real y separar no aplicabilidad técnica de aceptación del profesor.
6. **Accesibilidad:** ampliar pruebas de teclado, modales, zoom/reflow y estados de validación. Conservar Tabler y paleta monocromática; registrar reglas axe incompletas y disponibilidad real de lectores de pantalla.
7. **Cierre:** compilar, suites SQL/.NET/Flask/E2E, navegador, OSV y Docker donde sea posible. Actualizar matriz con capa, prueba y evidencia; clasificar preparación local, staging y producción según lo ejecutado.

## Resultado del cierre

La ejecución terminó con 71 pruebas .NET (0 omitidas), 78 Flask, 43 E2E y cinco suites SQL aprobadas. Los mismos 43 E2E se repitieron sobre Linux/Gunicorn/PostgreSQL. Reiniciar Gunicorn conservó la sesión con volumen y logout revocó el mismo JWT. Las imágenes se construyeron sin root y /app pasó el escaneo de exclusiones. OSV consultó 18 paquetes de la imagen final: cero avisos conocidos; los ocho GHSA anteriores están corregidos para ese runtime. Python global antiguo no fue modificado.

Edge completó 50 combinaciones de página/ancho, seis modales por teclado, tabla, validación y detalle. axe registró 18 comprobaciones, cero infracciones automáticas y diez instancias de reglas incompletas (color-contrast y th-has-data-cells). Se inspeccionaron capturas escritorio/tablet/móvil. Narrator fue detectado, pero no se obtuvo un recorrido asistido verificable en esta sesión headless; no se presenta como aprobado.

Production local arrancó en PORT=10000; se comprobaron health, 401 sin token, Swagger 404 y cookie Secure/HttpOnly/Strict. La configuración exige TLS Npgsql verificado y sesión privada persistente para una instancia. El YAML Render pasó JSON Schema; no se aplicó ni se probó Neon/TLS remoto. No se crearon recursos cloud, commits o staging Git; las pruebas no modificaron datos reales.

C06 se contrastó con las líneas 406/707 del prompt docente y el SQL confirmado: 22 columnas FK públicas, cero opcionales. No se alteró el modelo para inventar una relación opcional; la aceptación académica permanece pendiente del profesor.

Incidencias resueltas: daemon Docker detenido, DNS OSV del host, ACL del temporal pytest y esperas de recarga/transición del verificador. Se corrigió también contraste transitorio del menú y marcado accesible de obligatorio. El aviso recuperable GSSAPI de Npgsql en Linux no impidió bootstrap ni consultas; se registró sin confundir stderr informativo con código de fallo.

Clasificación: sustentación local preparada con observaciones; configuración lista para iniciar despliegue de pruebas controlado, todavía no desplegado; producción no validada. Pendientes: pruebas remotas, operación/recuperación/carga, permisos del disco Render, lectores/dispositivos reales, zoom de escritorio y decisión C06. Las matrices completas y ocho commits propuestos por responsabilidad (ninguno ejecutado) están en [V3_REVISION_FINAL.md](V3_REVISION_FINAL.md); [resultados saneados](evidencias/v3/revision-final.json).
