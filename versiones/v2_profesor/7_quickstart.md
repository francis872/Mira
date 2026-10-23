# Ejecutar MIRA V1 y V2

Requisitos locales: .NET 10 y dependencias restauradas, Python con `frontend/requirements-dev.txt`, PostgreSQL configurado según [scripts/README.md](../../scripts/README.md). No se necesita login de aplicación.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-local.ps1
```

Abrir `http://localhost:5000` y el acceso **Investigación**, o directamente `http://localhost:5000/investigacion`. Swagger: `http://localhost:8081/swagger`. Se aplican migraciones pendientes 001–007 por registro, sin reemplazar V1. Después de cambiar código de un proceso ya iniciado, detener con `stop-local.ps1` y volver a iniciar.

Para probar la ficha: crear una sede; crear grupo, seleccionar sede y añadir dos semilleros; guardar; volver a abrir y editar/retirar renglones. Para asociaciones: crear línea y usar Líneas de grupos o los demás puentes. Docentes requiere una persona institucional ya cargada; el selector de personas es de solo lectura y nunca pide contraseñas. No hay datos de docentes/grupos inventados en el arranque.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-postgres16.ps1
```

El verificador usa binarios 16.15 en `.local/postgresql16/pgsql/bin`, instancia separada, puertos 5547/8082/5001 y bases únicas. Crea una persona sintética solo dentro de su base exclusiva para probar docentes; al finalizar detiene sus procesos. Resultado: 47 .NET, 63 Flask/arquitectura, 46 HTTP y cuatro SQL. Las bases se conservan para inspección.

También existe `verify-local.ps1` para plataforma ya iniciada, con base SQL/.NET aislada. El recorrido docente HTTP se omite si la plataforma habitual carece de personas disponibles; no genera identidades para completarlo. Navegador: `node scripts/verify-browser.mjs` con Edge/CDP preparado según scripts/README; 14 controles y 72 combinaciones.

Docker conserva su configuración, pero no se verificó su ejecución. No borrar volúmenes para aplicar las nuevas migraciones. C06 y documentación del curso: [criterios](../../docs/criterios-v2.md).
