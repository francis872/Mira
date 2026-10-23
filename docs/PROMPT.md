# Prompt de trabajo para MIRA

Este prompt resume el contexto del repositorio para continuar el desarrollo con un asistente de programación. No reemplaza los documentos oficiales ni el Spec Kit. Verifica el estado actual del código antes de actuar.

Todos los prompts están en el [índice de prompts](prompts/README.md), incluido el [prompt maestro adaptado a MIRA V2](prompts/PROMPT_MAESTRO_MIRA_V2.md) y el original de Backstage conservado como referencia.

La auditoría adaptada a MIRA está registrada en [auditoria-v2.md](auditoria-v2.md), con [trazabilidad](trazabilidad-v2.md), [pruebas](pruebas-v2.md) y [pendientes](pendientes-v2.md). Consulta esas evidencias antes de repetir verificaciones o declarar completa V2.

## Prompt para copiar

```text
Trabaja sobre MIRA (Módulo de Investigación para la Gestión de Proyectos Académicos), de la Universidad de San Buenaventura Medellín.

Antes de modificar archivos:
1. Revisa el estado de Git y conserva los cambios existentes del usuario.
2. Lee README.md, docs/1_constitution.md y docs/fuentes/README.md.
3. Consulta el Spec Kit de la versión involucrada en versiones/ y las instrucciones locales que correspondan.
4. Contrasta este contexto con el código actual. No presentes funcionalidades pendientes como terminadas.

Contexto de partida:
- API C# / ASP.NET Core sobre .NET 10, PostgreSQL 16 con Npgsql y Dapper, frontend Flask con Jinja y Bootstrap 5.
- Arquitectura obligatoria: Controller → Service → Repository → PostgreSQL. Solo Repository accede directamente a la base de datos.
- V1 implementa seis catálogos: áreas de aplicación, áreas de conocimiento, líneas de investigación, objetivos de desarrollo sostenible, términos clave y universidades. Conserva su CRUD, borrado lógico y conteos reales del dashboard.
- V1 y V2 no incluyen autenticación ni autorización. El dashboard abre directamente, sin login. No agregues JWT, contraseñas, restricciones por sesión ni pantallas de usuarios como parte de estas versiones.
- Conserva las validaciones, SQL parametrizado, integridad referencial, escape de HTML y protección CSRF en los formularios.
- La migración 003 mantiene usuario, rol y usuario_rol y sus rutinas almacenadas con JSONB; no están integradas a la API ni a las pantallas. No elimines sus datos.
- V2 está parcialmente implementada. El modelo relacional oficial de la Entrega 2 falta en docs/fuentes/. No inventes entidades, relaciones, triggers ni reglas académicas para completar ese vacío. Si una tarea depende de ese documento, explica la dependencia y continúa con el trabajo independiente que sí esté definido.

Forma de trabajar:
- Sigue Spec-Driven Development: actualiza primero la especificación correspondiente si cambia la funcionalidad y después implementa.
- Aplica inyección de dependencias, SOLID y YAGNI; evita anticipar versiones futuras.
- Realiza escrituras maestro-detalle de forma atómica dentro de una única rutina PostgreSQL. No las dividas en escrituras independientes desde la API.
- Usa las migraciones controladas de database/migrations y su registro schema_migrations. Conserva los datos existentes; no borres volúmenes o bases para resolver una migración.
- No publiques secretos, archivos .env ni artefactos locales.
- Mantén instrucciones de arranque reproducibles. En Windows sin Docker, consulta scripts/README.md y usa scripts/start-local.ps1; para detener, scripts/stop-local.ps1.

Verificación según el cambio:
- API: dotnet test MIRA.sln. La regresión con PostgreSQL requiere MIRA_TEST_DB y una base de pruebas con las migraciones aplicadas.
- Frontend: instala frontend/requirements-dev.txt y ejecuta las pruebas de frontend/tests/test_app.py.
- Integración: con la plataforma activa, configura MIRA_E2E_URL y MIRA_E2E_API_URL y ejecuta frontend/tests/e2e.
- Rutinas PostgreSQL: consulta y ejecuta database/tests/v2_usuarios_roles.sql sobre una base de pruebas cuando corresponda.
- Para cambios solo de documentación, revisa coherencia, enlaces y git diff --check; no es necesario arrancar la plataforma.
- Reporta los resultados realmente obtenidos y las comprobaciones que no pudiste ejecutar.

Al finalizar, explica brevemente qué cambió, por qué, cómo se verificó y qué dependencias quedan pendientes. Actualiza la documentación y las tareas afectadas. Solo crea commits, sube cambios o fusiona ramas cuando el usuario lo haya autorizado; respeta las condiciones de revisión de la versión.

Tarea concreta a realizar:
[Escribe aquí el cambio solicitado y sus criterios de aceptación.]
```
