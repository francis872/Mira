# Perfil vigente de Fase 6

Nuevas instalaciones aplican 001–015. Docker conserva Python 3.11 y el modo filesystem privado existente; Vercel selecciona Python 3.14 y exige MIRA_SESSION_BACKEND=postgres, MIRA_SESSION_DATABASE_URL privada con verify-full/channel_binding=require y MIRA_PLATFORM=vercel. No se usa /tmp para sesiones. Instalar requirements-dev.txt para verificar, requirements.lock para runtime Docker; requirements.txt también fija transitivos para Vercel. [Instalación y recuperación cloud](../FASE6_DESPLIEGUE.md). Los apartados Fase 5 y anteriores siguientes se conservan como antecedentes.

# Instalación de las correcciones Fase 5

El paquete ZIP del corte anterior permanece histórico: no contiene estas correcciones. Usar este árbol revisado, un entorno limpio y las migraciones 001–014. Frontend conserva Python 3.11 en Docker; se añaden OpenpyXL 3.1.5 y ReportLab 5.1.0 (lock incluye et-xmlfile y Pillow); pypdf 7.0.0 se usa solo para verificar PDFs en pruebas. La instalación limpia y pip check aprobaron; el entorno global no se actualizó.

Antes de actualizar datos útiles: respaldo verificable, ensayo en copia y preflight 009. Si faltan inglés V1 o hay categorías ajenas, 009 aborta atómicamente: completar con contenido aprobado y repetir; no desactivar CHECK. En V2 los CHECK NOT VALID preservan datos incompletos, pero exigen completarlos al escribir esas filas. Auditar NULL/vacíos en código sede, inglés plural y descripción, y mínimos de disciplinas/ODS; completar con decisión académica y ejecutar VALIDATE CONSTRAINT sobre los tres CHECK de 010. No ejecutar esta fase contra producción.

Recuperación ensayada: pg_dump -Fc antes de 009; migrar copia; restaurar en otra base vacía; comprobar todas las filas originales, registro 001–008 y seguridad V3. No aplicar down migrations destructivas. Las claves y dumps de prueba permanecen en .local ignorado. [Evidencia](evidencias/fase5-20261107/upgrade-recuperacion.json).

---

## Corte previo conservado como antecedente

# Instalación y reproducción local

Usar únicamente una base nueva con datos ficticios. Raíz auditada: `C:\Users\Usuario\OneDrive\Escritorio\MIRA`. Rama `feature/mira-v3`, HEAD 3105273af84e55a1e813e76f2acb92acc31caa94 **más los cambios locales identificados por el manifiesto de fuentes**. Hacer checkout de HEAD solamente no reproduce D01 corregido. [Versión exacta](evidencias/version-fuentes.json).

Requisitos de las ejecuciones realizadas: SDK .NET 10, Python 3.14 en Windows o 3.11 en imagen Linux, PostgreSQL 16.15, Docker Linux; Node y Edge para navegador/zoom. Usar entorno Python limpio y requirements-dev.txt/requirements.lock. El Python global no se actualizó como parte de este cierre. Los scripts PowerShell existentes fijan la raíz auditada: no se certifica portabilidad a cualquier ruta del disco.

## Entorno Windows

```powershell
dotnet restore MIRA.sln
python -m venv .local/venv-cierre
.local/venv-cierre/Scripts/python -m pip install -r frontend/requirements-dev.txt
.local/venv-cierre/Scripts/python -m pip check
```

Crear `.env.local` privado a partir de los nombres documentados en `.env.example`, con base/rol de pruebas y valores aleatorios. No pegar credenciales en comandos, capturas o documentos. `start-local.ps1` admite -PostgresBin y genera claves persistentes privadas JWT y forwarding cuando no están configuradas. Revisar que el nombre de base/puerto corresponde al entorno de demostración, antes de iniciar: el script aplica migraciones pendientes en la base configurada.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-local.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/bootstrap-admin.ps1
```

Bootstrap solicita correo/contraseña, permite el primer administrador y usa procedimiento atómico. Usar correo `demo-admin@example.invalid` y contraseña privada generada para esta base. Abrir http://localhost:5000/login y Swagger Development http://localhost:8081/swagger. Crear un Investigador y Coordinador ficticios desde Cuentas. `bootstrap-admin.ps1 -ResetPassword` es recuperación operativa con acceso a BD, no una ruta pública.

Detener con scripts/stop-local.ps1 solo si esa instancia fue iniciada con el script; no detener servicios de otros proyectos. Se conservan datos. Para las pruebas completas aisladas usar verify-postgres16 con puertos libres y binarios PG16, según la guía de pruebas, en lugar de apuntar E2E a una base real.

## Docker en una instalación nueva aislada

Copiar `.env.example` a un archivo privado **dentro de .local**, sustituir todos los REPLACE y configurar POSTGRES_DB/USER/PASSWORD, JWT_KEY, SECRET_KEY y **LOGIN_FORWARDING_KEY independiente**. Definir tres puertos libres. No usar valores de fixtures CI en un despliegue. Mantener la misma clave forwarding en API y frontend; Compose la distribuye a LoginProtection__ForwardingKey/MIRA_LOGIN_FORWARDING_KEY.

```powershell
docker compose --project-name mira-cierre-demo --env-file .local/demo.env config --quiet
docker compose --project-name mira-cierre-demo --env-file .local/demo.env up -d --build
```

Elegir un nombre de proyecto nuevo para no reutilizar el volumen habitual `mira-v2`. En base nueva el contenedor PostgreSQL inicializa las migraciones; en una base existente se requiere el operador y apply.sh para las pendientes. El API no aplica DDL al arrancar. El frontend usa Gunicorn un worker/cuatro threads y volumen de sesiones. Aprovisionar administrador pasando MIRA_ADMIN_EMAIL/MIRA_ADMIN_PASSWORD como variables privadas de proceso a `docker compose ... exec -e MIRA_ADMIN_EMAIL -e MIRA_ADMIN_PASSWORD mira-api dotnet MIRA.Api.dll --bootstrap-admin`; limpiar esas variables después.

Para terminar la demostración, ejecutar `down` con el **mismo proyecto y env-file**, sin `-v`. No borrar imágenes/volúmenes ajenos ni hacer `prune`. El verificador Docker automatizado crea nombres mira-v3-check-<fecha> y conserva sus propios volúmenes de prueba.

## Paquete de revisión

El cierre entrega un ZIP local sin publicar en `.local/cierre-integral-20261107/SAPIENS-MIRA-V1-V3-revision-local.zip`, con fuentes ejecutables, migraciones, pruebas, documentación consolidada y manifiesto SHA256. Excluye .git, .local, .env privados, sesiones, dumps, logs privados y documentos Office originales con posibles metadatos personales. No incluye una base institucional ni credenciales preconfiguradas. El comprobante privado `package.json` registra SHA256 del ZIP y todos sus miembros. El paquete permite reconstruir la aplicación local; la configuración privada se genera en el destino.

No se ejecutó despliegue. Render/Neon y HTTPS real requieren otra autorización y una prueba específica. La reproducción local no acredita producción.
