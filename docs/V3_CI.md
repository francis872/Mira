# GitHub Actions propuesto para MIRA V3

Preparado localmente el 2026-11-07 para el PR #7. **Sin commit/push ni ejecución remota de este workflow. No hay CI aprobado por GitHub.**

Archivo: `.github/workflows/mira-v3-ci.yml`. Usa pull_request hacia main, push a main y workflow_dispatch; no usa pull_request_target, despliegues, registry push, entornos productivos ni secretos reales. El checkout conserva el merge ref predeterminado del PR para comprobar la integración. Permisos contents:read y persist-credentials:false; concurrencia por PR y límites de tiempo de 20/25 minutos.

## Trabajos y controles

| Trabajo | Operaciones | Límite y evidencia |
|---|---|---|
| quality | SDK .NET 10, Python 3.11, PostgreSQL 16 temporal; restore/build Release; migraciones 001–008 y cinco SQL; .NET integrado; Flask con dependencias fijadas | check_results.py rechaza TRX/JUnit vacíos, fallidos u omitidos. La falta de MIRA_TEST_DB no puede producir una integración aprobada por omisión. |
| quality / seguridad | Escaneo heurístico de archivos versionados; NuGet JSON; OSV sobre venv runtime con lock e instaladores retirados, equivalente al conjunto Python de la imagen | Avisos NuGet, OSV vulnerable o OSV blocked fallan el trabajo. No cubre todos los CVE del SO ni garantiza ausencia futura. |
| quality / configuración | docker compose config --quiet con fixtures y carga de gunicorn.conf.py | Solo valida configuración; no arranca el Compose habitual. Las guardas Production también están en las pruebas de API/Flask. |
| containers | Build de API/frontend, export e inspección de /app y usuario; smoke Production local en PORT=10000 | Destinos example.invalid, sin conexión Neon/API real; cookies y health no certifican TLS remoto. Sin publicación de imágenes. |

PostgreSQL se ejecuta como service container del job Linux y termina con él. Los valores `ci-only-*` y `mira_ci_*` son fixtures públicos exclusivos; no son contraseñas institucionales ni deben reutilizarse. El inicializador exige PostgreSQL 16, loopback, usuario/base de prueba y **cero tablas públicas preexistentes**; no borra ni vacía una base para hacerla pasar. [Modelo oficial de service containers](https://docs.github.com/en/actions/tutorials/use-containerized-services/create-postgresql-service-containers).

No se añaden dependencias al runtime de la aplicación. Las E2E completas y el navegador continúan en los verificadores locales; este primer CI no afirma automatizar lector de pantalla, dispositivos físicos o Render/Neon. No se suben .env, sesiones, logs privados ni TRX como artefactos indiscriminados; GitHub conservará los logs del job y sus resúmenes saneados según la política de la cuenta.

## Acciones fijadas

Se resolvieron los tags oficiales por git ls-remote y se fijaron los commits, sin usar tags móviles en uses:

| Acción | SHA preparado |
|---|---|
| actions/checkout v6 | d23441a48e516b6c34aea4fa41551a30e30af803 |
| actions/setup-dotnet v5 | 26b0ec14cb23fa6904739307f278c14f94c95bf1 |
| actions/setup-python v6 | ece7cb06caefa5fff74198d8649806c4678c61a1 |

Referencias: [checkout](https://github.com/actions/checkout/tree/v6), [setup-dotnet](https://github.com/actions/setup-dotnet/tree/v5), [setup-python](https://github.com/actions/setup-python/tree/v6). Revisar futuras actualizaciones por PR; pinning no garantiza ausencia de vulnerabilidades.

## Validación realizada y límites

- actionlint 1.7.12: sintaxis, expresiones y estructura aprobadas; binario descargado del release oficial y comparado con su checksum. Se desactivaron integraciones ShellCheck/Pyflakes no instaladas; no se afirma haberlas ejecutado.
- Restore/build Release local: cero advertencias/errores. PostgreSQL 16 Docker vacío: ocho migraciones/cinco SQL; 71 tests .NET, cero omisiones. Flask: 80 aprobadas, incluidas dos nuevas de estado vacío/detalles reales.
- Los gates rechazaron fixtures de TRX con omisión, JUnit vacío y NuGet vulnerable. La guardia SQL rechazó una invocación sin MIRA_CI antes de conectar. NuGet JSON real de dos proyectos sin avisos fue aceptado; el parser admite UTF-8 y el BOM UTF-16 de PowerShell.
- Compose/Gunicorn e imágenes se comprobaron localmente. Esto no reproduce todos los aspectos de ubuntu-24.04/GitHub: permisos del token, descarga del SDK/acciones, servicios, red y cuota solo podrán comprobarse en Actions tras publicar.

Evidencia local privada: `.local/academic-close/actionlint.log`, `ci-local.log`, `ci/ci.trx`, `flask.xml`, `nuget.json`. Los reportes saneados del cierre se conservan separados de los históricos.

## Publicación y criterio de cierre

1. Autorizar los commits locales propuestos y su push a feature/mira-v3, conservando el PR #7.
2. El evento synchronize deberá iniciar los dos trabajos. Si GitHub requiere aprobación de ejecución o permisos, registrar ese bloqueo; no convertirlo en éxito.
3. Revisar logs y SHA del run real. Solo después de ambos trabajos correctos se puede declarar CI aprobado para ese commit.
4. Proponer al propietario configurar esos checks como obligatorios antes de merge; identificar sus nombres reales en el run. No se cambió protección de ramas.

No existe autorización para fusionar main ni desplegar. Los minutos/cuotas de Actions y la disponibilidad de red deben revisarse en la cuenta; no se ejecutó ningún job remoto nuevo en esta revisión.
