# Pruebas vigentes: modelo oficial integrado

23 de octubre de 2026. Suite final sobre **PostgreSQL 16.15**, instancia/API/Flask aisladas: `mira_pg16_20261023153150_28080`; base SQL/.NET `mira_audit_20261023153202_28080`.

| Comprobación | Resultado |
|---|---|
| Build API y solución | 0 errores, 0 advertencias |
| Migraciones 001–007 | Aplicadas en bases nuevas y 006/007 después en plataforma existente |
| SQL | Cuatro archivos aprobados con rollback: catálogos, usuarios legados, investigación y comparación contra fuente |
| .NET | **47 aprobadas, 0 fallidas, 0 omitidas** |
| Flask/arquitectura | **63 aprobadas** |
| HTTP Flask→API→PostgreSQL 16 | **46 aprobadas, sin omisiones** |
| Edge en plataforma habitual PostgreSQL 17.9 | **14 controles, 72 combinaciones página/ancho, cero excepciones JavaScript** |
| Columnas/nulabilidad/PK/FK/UNIQUE/CHECK/índices oficiales | 19 tablas fuente reconstruidas en esquema aislado: sin diferencias |
| Actualización plataforma habitual | Respaldo `.local/before-official-model-20261023153312.dump`, aplicación aditiva y reinicio correctos |

El SQL prueba fallo tardío en alta y actualización atómica, conservación completa del estado anterior, inactivación de detalles omitidos, rechazo de padre inactivo por trigger, puentes, FK y duplicados. .NET recorre las cinco operaciones de once recursos y los cinco puentes; HTTP usa formularios para todos, incluida persona exclusiva de la base de prueba para docentes. No se crea esa persona en la plataforma habitual.

La primera ejecución ampliada pasó 47/63/45; la final añadió comparación del esquema y el formulario independiente de semillero, y pasó 47/63/46. Edge creó sede/grupo/dos semilleros/línea, editó/retiró renglones y asignó/duplicó/retiró un puente. Captura de ficha inspeccionada visualmente. Datos de navegador retirados al terminar.

El primer intento de conectar Edge se adelantó a su arranque y recibió ECONNREFUSED; al estar disponible, el recorrido completo pasó. No se presenta el intento fallido como prueba aprobada. El navegador aislado se cerró al finalizar.

Los puertos de pruebas se liberaron y las bases se conservaron. Docker/Python 3.11, carga, CVE y accesibilidad exhaustiva no se probaron. C06 sigue bloqueado por modelo sin FK opcionales. [Matriz](criterios-v2.md) · [Reproducción](../versiones/v2_profesor/7_quickstart.md).

## Registro histórico de etapas anteriores

Los bloqueos y resultados siguientes corresponden a etapas anteriores a la confirmación del SQL. El estado vigente es el indicado arriba y en la matriz actual.

# Pruebas ejecutadas en la auditoría MIRA

## PostgreSQL 16 y recuperación de fuentes (actualización vigente)

Se ejecutó la plataforma completa de pruebas en **PostgreSQL 16.15**, con API y Flask propios en 8082/5001, separados de la plataforma habitual. `scripts/verify-postgres16.ps1` aplicó migraciones 001–005 y ejecutó **41 .NET, 42 Flask/arquitectura, 31 HTTP y ambos SQL con rollback**, sin fallos. Las repeticiones confirmaron los mismos resultados, reutilización de la instancia y cierre de procesos. La ejecución final compiló la API actual en Debug y usó la base exclusiva `mira_audit_20261023151053_26140`.

Los puertos 5547/8082/5001 quedaron libres al terminar; la plataforma habitual respondió HTTP 200. [Detalle y reproducción](verificacion-postgres16.md). La falta de PostgreSQL 16 indicada en etapas anteriores queda resuelta para esta verificación local; Docker y Python 3.11 no se probaron. Las nuevas entidades académicas siguen sin implementarse porque el usuario indicó usar únicamente el esquema oficial, aún no localizado.

## Rediseño y publicación autorizados (actualización)

Con el nuevo diseño se volvió a ejecutar `scripts/verify-local.ps1`: **41 .NET, 42 Flask/arquitectura, 31 E2E HTTP**, ambos SQL con rollback y cero fallos; base `mira_audit_20261023144423_9628`. Se ejecutó además el verificador Edge ampliado: **10 comprobaciones, 52 combinaciones de página/ancho, cero excepciones JavaScript capturadas**. La comprobación de teclado envía una pulsación Enter completa por CDP y espera el resultado del desplegable nativo.

El detalle del rediseño, capturas, archivos y autorización de GitHub está en [diseno-v2.md](diseno-v2.md). El usuario confirmó que la publicación solicitada es el código en el repositorio, no un despliegue público. Las ejecuciones siguientes documentan etapas anteriores de la misma implementación.

## Ejecución del prompt definitivo (actual)

Regresión antes de modificar persistencia: **31 .NET, 40 Flask y 30 E2E HTTP aprobadas**, base exclusiva `mira_audit_20261023140939_4908`.

Verificación final mediante `scripts/verify-local.ps1`, con migraciones 001–005 y base exclusiva `mira_audit_20261023142533_15620`:

| Comprobación | Resultado |
|---|---|
| Build Release al arrancar | 0 advertencias, 0 errores |
| .NET con PostgreSQL real | **41 aprobadas, 0 fallidas, 0 omitidas** |
| Flask y guardia arquitectónica | **42 aprobadas** |
| E2E HTTP Flask → API → PostgreSQL | **31 aprobadas** |
| SQL `v2_catalogos_rutinas.sql` | Inventario de 30 funciones, trigger habilitado/deshabilitado, INSERT/UPDATE, vacío e inactivación: DO exitoso y rollback |
| SQL `v2_usuarios_roles.sql` | Dos DO exitosos y rollback; evidencia legada separada del dominio académico |
| Navegador real Edge con CDP, `node scripts/verify-browser.mjs` | **7 comprobaciones**, sin excepciones JavaScript capturadas |
| Persistencia después de reiniciar PostgreSQL/API/Flask con nuevas rutinas | Misma fila e ID recuperados; registro `audit-routines-*` inactivado al terminar |
| Migraciones tras segundo arranque | Omitidas por registro existente; datos conservados |

La prueba de arquitectura confirma 30 invocaciones en seis repositorios y ausencia de Dapper/Npgsql/SQL en Controller/Service. La integración confirma comportamiento real de esas llamadas. La prueba de trigger compara el resultado con el disparador habilitado y deshabilitado dentro de una transacción exclusiva: no se atribuye a un trigger una transformación hecha en la API.

El navegador verificó navegación activa, búsqueda sin coincidencias, apertura/cancelación de modal, alta/edición/retiro desde formularios reales, normalización visible y menú móvil a 390px sin desbordamiento. Capturas en [evidencias](evidencias/README.md). La primera ejecución de la comprobación de navegador intentó cerrar durante la transición de Bootstrap y agotó el tiempo; se corrigió la espera del verificador y se ejecutó nuevamente con éxito. No se presentó ese intento como aprobado.

Las nuevas pruebas detectaron que el proveedor predeterminado Event Log falla en la cuenta restringida al registrar una advertencia. Se corrigió la API para registrar a consola, capturada por los scripts locales. Las 41 pruebas finales incluyen siete escenarios de excepción segura y dos de contrato JSON inválido en español.

SQLSTATE se prueba técnicamente con excepciones controladas en el host HTTP; no se afirma integración de una FK académica inexistente. PostgreSQL 16 no se probó: no está disponible `C:\Program Files\PostgreSQL\16\bin\psql.exe` y Docker no está instalado. El entorno real sigue siendo PostgreSQL 17.9. Tampoco se afirma escaneo CVE, carga o accesibilidad exhaustiva. Los resultados de la sección siguiente son históricos.

## Primera auditoría (histórico)

Fecha: 23 de octubre de 2026, Colombia. Entorno observado: Windows, Python 3.14, .NET SDK 10.0.402, PostgreSQL 17.9. Dependencias: Flask 3.1.3, Werkzeug 3.1.9, requests 2.32.5, python-dotenv 1.2.2, pytest 9.1.1. Las versiones declaradas se alinearon con este entorno.

## Resultados obtenidos

| Ejecución | Resultado observado |
|---|---|
| `scripts/start-local.ps1` después de las correcciones | Build Release exitoso, 0 advertencias y 0 errores; API y frontend responden |
| `scripts/verify-local.ps1`: migraciones 001–003 sobre base nueva | Exitosas, sin modificar la base operativa |
| `database/tests/v2_usuarios_roles.sql` en base exclusiva | `BEGIN`, dos bloques `DO`, `ROLLBACK`, sin errores |
| `dotnet test MIRA.sln --no-restore --verbosity minimal` con `MIRA_TEST_DB` | **31 aprobadas, 0 fallidas, 0 omitidas** |
| Desde `frontend/`: `python -m pytest tests/test_app.py -q` | **40 aprobadas**, 0 fallidas |
| `python -m pytest frontend/tests/e2e -q` con URLs configuradas | **30 aprobadas**, 0 fallidas |
| Registro persistido → parada de tres servicios → arranque → lectura del mismo ID y nombre | Exitoso; registro `audit-persist-*` inactivado al terminar |
| GET dashboard, `/health`, `/swagger/v2/swagger.json` tras reinicio | HTTP 200 en los tres casos |
| `git diff --check` | Sin errores de espacios; aviso informativo de conversión CRLF/LF del archivo SQL |
| `docker compose config --quiet` | No ejecutable: comando `docker` no disponible en PATH |

Base exclusiva creada y conservada: `mira_audit_20261023111544_27224`. Contiene semillas y tablas; las pruebas SQL hacen rollback y las .NET eliminan únicamente datos con su prefijo dentro de esa base. No se publica su cadena de conexión. Los E2E crean datos `e2e-*` en la plataforma y los inactivan; no son pruebas sobre una instalación productiva.

## Cobertura y límites

- .NET: ciclos CRUD reales de seis catálogos, fila conservada tras borrado lógico, recursos inexistentes, campos vacíos, duplicados, colisión concurrente, clave inactiva y longitud excesiva en POST/PUT. Además, pruebas sin autenticación existentes.
- Flask: cliente API sustituido por dobles para aislar presentación, red, validación, cookies y CSRF. `FakeResponse` ahora reproduce la falsedad de HTTP 4xx/5xx de requests.
- E2E: clientes HTTP con cookies recorren Flask → API → PostgreSQL, formularios y tokens CSRF, recarga desde otro cliente, actualización, inactivación, conteos, semillas, conflictos y longitudes. No automatizan un navegador real ni ejecutan JavaScript de modales.
- Seguridad: texto de inyección almacenado como dato, HTML escapado, token faltante/incorrecto/no ASCII rechazado, flags HttpOnly y SameSite. No se probaron roles/aislamiento organizacional, porque no existen en el alcance vigente.
- SQL: FK inválidas, alta atómica, actualización de detalles, fallo que conserva el estado previo, funciones JSONB, procedimientos CALL, inactivación y bootstrap legado. El hash sintético es solo dato de prueba; no acredita autenticación.
- Reinicio: se creó un registro por API, se guardó su ID localmente, se reiniciaron PostgreSQL/API/Flask, se volvió a consultar y se inactivó. Evidencia auxiliar ignorada por Git: `.local/persistence-check.json`.

No se ejecutó escaneo de CVE/advisories, prueba de carga, auditoría visual/accesibilidad, pruebas exhaustivas de claves textuales con caracteres reservados, Docker/PostgreSQL 16, ni despliegue. No se declara preparación para producción.

## Reproducción

Preparar dependencias y arrancar según [scripts/README.md](../scripts/README.md). Con la plataforma iniciada:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-local.ps1
```

El script usa `.env.local`, crea una base con nombre único y conserva esa base para inspección. Requiere permisos locales para crear bases, .NET restaurado y dependencias de desarrollo Python. Falla inmediatamente si una etapa falla. No imprime contraseñas ni borra bases existentes. Cada ejecución deja una base adicional; su eliminación no está automatizada.

Para pruebas aisladas del frontend, ejecutar desde `frontend/`, porque `test_app.py` importa `app` desde ese directorio. El intento inicial desde la raíz sin configurar PYTHONPATH falló por `ModuleNotFoundError`; no es una prueba aprobada. La ejecución posterior desde el directorio correcto sí pasó.
