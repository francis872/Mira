# Pruebas ejecutadas en la auditoría MIRA

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
