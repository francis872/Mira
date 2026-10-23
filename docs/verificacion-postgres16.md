# Verificación real en PostgreSQL 16

23 de octubre de 2026. Se levantó una plataforma de pruebas completa e independiente sobre **PostgreSQL 16.15**, sin convertir la base PostgreSQL 17 existente ni detener la plataforma habitual.

## Entorno y resultados

Binarios Windows descargados desde EnterpriseDB, proveedor enlazado por [PostgreSQL para Windows](https://www.postgresql.org/download/windows/). La [página de binarios EDB](https://www.enterprisedb.com/download-postgresql-binaries) identifica el paquete PostgreSQL 16.15 utilizado. No se instaló un servicio del sistema.

- Paquete: `https://get.enterprisedb.com/postgresql/postgresql-16.15-1-windows-x64-binaries.zip`.
- Tamaño descargado: 332441502 bytes.
- SHA-256 calculado localmente: `25e6fcdfb8caec38691bf461125e7564508760666f7b8e5dc6a5f0818f58f81e`. Es un registro del artefacto utilizado, no una afirmación de comparación con un checksum publicado por el proveedor.
- Herramientas: `.local/postgresql16/pgsql/bin`, versión confirmada con `postgres.exe --version` y `psql.exe --version`.
- Datos/contraseña/logs de pruebas: `.local/pg16-validation/`, ignorado por Git.
- Puertos de pruebas: PostgreSQL 5547, API 8082 y Flask 5001, todos en `127.0.0.1`.

Las primeras ejecuciones publicaron una copia de la API Release existente; el runner final compila el código actual en Debug a una ruta propia para evitar usar binarios antiguos y no bloquear el Release habitual. Su cadena de conexión apunta a PostgreSQL 16 y el frontend de pruebas consume esa API. El script comprueba `server_version_num` y aplica migraciones 001–005 sin cambiar tablas académicas ni inventar entidades.

| Comprobación | Resultado |
|---|---|
| Migraciones 001–005 en PostgreSQL 16 | Aplicadas |
| `v2_catalogos_rutinas.sql` | DO exitoso y rollback |
| `v2_usuarios_roles.sql` | Dos DO exitosos y rollback |
| .NET con base exclusiva PostgreSQL 16 | 41 aprobadas, 0 fallidas, 0 omitidas |
| Flask y guardia de arquitectura | 42 aprobadas |
| E2E HTTP → Flask de pruebas → API de pruebas → PostgreSQL 16 | 31 aprobadas |

Primera ejecución: base de plataforma `mira_pg16_20261023150312_9184`, base exclusiva .NET/SQL `mira_audit_20261023150337_9184`. Hubo advertencias de escritura de caché pytest en OneDrive, sin fallos de pruebas. El runner desactiva después esa caché auxiliar; no desactiva comprobaciones de seguridad ni elimina archivos del usuario.

Segunda ejecución, para comprobar repetibilidad y limpieza: base de plataforma `mira_pg16_20261023150545_24172`, base .NET/SQL `mira_audit_20261023150552_24172`. Mismos resultados **41/42/31**, sin advertencias de caché. Se comprobó al terminar que 5547/8082/5001 quedaron libres y que dashboard y API habituales seguían devolviendo HTTP 200. Las seis fuentes históricas recuperadas se compararon byte por byte con sus blobs originales.

Ejecución del runner final, compilando la API actual en Debug: base de plataforma `mira_pg16_20261023151043_26140`, base .NET/SQL `mira_audit_20261023151053_26140`. Compilación y migraciones correctas, ambos SQL aprobados, **41 .NET, 42 Flask/arquitectura y 31 HTTP** aprobadas; la instancia se detuvo correctamente. Esta es la ejecución de referencia para reproducir la versión final del script.

## Reproducción

Con las dependencias .NET restauradas, dependencias Python instaladas y binarios 16 extraídos en la ruta indicada:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-postgres16.ps1
```

Puede indicarse otra instalación con `-PostgresBin 'C:\ruta\PostgreSQL\16\bin'`. Los parámetros `-DatabasePort`, `-ApiPort` y `-FrontendPort` permiten elegir puertos libres. Si un puerto está ocupado, el script falla y no detiene el servicio ajeno. No reutiliza el directorio de PostgreSQL 17, no elimina bases y conserva la instancia de datos para inspección. Genera credenciales exclusivas locales y restaura las variables de entorno al terminar.

En su bloque `finally`, detiene únicamente la API, Flask y PostgreSQL iniciados por esa ejecución. La aplicación habitual continúa disponible en `http://localhost:5000`. La prueba acredita la implementación actual en PostgreSQL 16; no acredita el CRUD académico V2 que sigue sin esquema oficial ni un despliegue Docker o productivo.

## Estado de los seis pendientes solicitados

Esta lista corresponde a los seis puntos de trabajo solicitados en la conversación, no a una nueva numeración de C01–C12:

| Punto | Estado actual |
|---|---|
| 1. Conseguir modelo Entrega 2, VERSION_2 y rúbrica | Se recuperaron fuentes históricas, pero el esquema vigente, el documento de versión y la rúbrica siguen ausentes. No se cierra este punto. |
| 2. Nuevas tablas y pantallas V2 | Bloqueado por el esquema oficial, para no inventar columnas o alterar V1. |
| 3. FK con desplegables y referencias opcionales | Bloqueado: faltan claves, cardinalidades y nulabilidad oficiales. |
| 4. Puentes e integridad | Bloqueado: no se conocen las asociaciones y reglas vigentes. La traducción técnica a 409 ya está probada, pero no sustituye el flujo académico. |
| 5. Maestro-detalle académico | Bloqueado: faltan esquema y reglas. No se usa usuario→roles como sustituto. |
| 6. Verificación en PostgreSQL 16 | Verificado para toda la plataforma actualmente implementada: SQL, API, Flask y E2E reales. Las pruebas de un dominio académico aún inexistente siguen pendientes. |

El usuario eligió explícitamente **«Usar únicamente el modelo oficial»**. Por tanto, la recuperación de documentos históricos y la prueba del motor son avances reales, pero no autorizan cerrar los otros cinco puntos mediante un modelo provisional. La [matriz C01–C12](criterios-v2.md) mantiene seis criterios verificados y seis bloqueados.
