# Repetir la verificación funcional

Este instrumento ejecuta formularios reales en Edge mediante CDP, mantiene las sesiones normales de Flask/API y comprueba la persistencia con una segunda conexión PostgreSQL. No despliega ni modifica servicios de producción. Usa Windows, Edge instalado, Node 24 y Python con `requests` y `psycopg`; las pruebas de preparación de cuentas usan las herramientas habituales del proyecto.

1. Obtener un respaldo autorizado con un cliente `pg_dump` compatible. Comprobar su SHA-256 y restaurarlo con `pg_restore --no-owner --no-acl --exit-on-error` en una **instancia local propia**, accesible solo desde `127.0.0.1`. El nombre de base debe comenzar por `mira_audit_functional_`. Conservar el dump privado y no editar su SQL para forzar compatibilidad.
2. Antes de crear fixtures, comparar cantidades y fingerprints de las tablas con la fuente. No apuntar una prueba a Neon. La ejecución documentada restauró 25 tablas/511 filas y las quince migraciones existentes.
3. Arrancar API y frontend locales contra esa copia usando las instrucciones normales del repositorio. La API debe tener una clave JWT aleatoria de pruebas, conexión local y URL `http://127.0.0.1:<puerto>/api` en el frontend. Configurar también las sesiones del frontend contra la copia local. No reutilizar servicios ni secretos de producción.
4. Crear **solo en la copia** tres cuentas QA activas, una por rol: Investigador, Coordinador y Administrador. Usar aprovisionamiento operativo o los procedimientos de autenticación existentes, con contraseñas aleatorias privadas. No cambiar la cuenta institucional restaurada para probar permisos.
5. Guardar un JSON **privado**, por ejemplo `.local/mi-recuperacion/runtime-private.json`, con la forma de abajo. Los valores son referencias a configuración local, nunca datos que deban versionarse. `dsn` usa formato libpq y el mismo PostgreSQL local de la API; `apiport` y `webport` son enteros. `env.API_URL` se verifica antes de empezar. No pasar contraseñas por argumentos de terminal.

```text
{
  "dsn": "<conexión privada a 127.0.0.1 / mira_audit_functional_...>",
  "apiport": <puerto local API>,
  "webport": <puerto local frontend>,
  "env": {"API_URL": "http://127.0.0.1:<puerto local API>/api"},
  "accounts": {
    "Investigador": {"email": "<QA local>", "password": "<privada>"},
    "Coordinador": {"email": "<QA local>", "password": "<privada>"},
    "Administrador": {"email": "<QA local>", "password": "<privada>"}
  }
}
```

Ejecutar desde la raíz, con los servicios locales ya preparados:

```powershell
python scripts/functional-recovery/run-browser.py --runtime .local/mi-recuperacion/runtime-private.json --output .local/mi-recuperacion/evidencias
```

El script crea una persona académica sintética, un perfil Edge propio y fixtures `RECUPERACION-…` exclusivamente locales. Usa los seis catálogos, once recursos, cinco puentes, ficha maestro-detalle, dos reportes y tres exportaciones. Comprueba Investigador, Coordinador y Administrador, administración de cuentas y 35 combinaciones responsive. Retira únicamente las entidades creadas por esa ejecución; las dos filas adicionales de referencia, hijos auxiliares y demás huella local se conservan para auditoría. **La base de pruebas no se importa a producción.**

`browser-functional.json` debe terminar con `status: passed`, 69 recorridos, cero excepciones JavaScript y comprobaciones SQL aprobadas. Las capturas, exportaciones y logs son privados hasta revisar su contenido; el runner no los publica. Si falla, conserva el informe, captura diagnóstica y datos para investigar. Cierra solo el navegador de su perfil; API/PostgreSQL locales se detienen con el procedimiento del entorno que los inició.

La revisión de accesibilidad, zoom real 200 %, permisos de API, respaldo y escaneo de secretos se documenta separadamente en [el informe](../../docs/SAPIENS_RECUPERACION_FUNCIONAL_2026-11-23.md). El resultado del navegador no sustituye esas verificaciones ni acredita dispositivos físicos o producción. Para .NET usar `MIRA_TEST_DB` local y `dotnet test MIRA.sln`; para Flask, esa misma variable habilita las pruebas de sesiones PostgreSQL. Ejecutar la regresión HTTP en proceso separado con `MIRA_E2E_URL`, `MIRA_E2E_API_URL` y credenciales QA privadas, nunca las URL públicas.
