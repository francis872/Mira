# PROMPT MAESTRO — MIRA V2

Versión anterior de trabajo. La instrucción vigente es el [prompt maestro definitivo auténtico](PROMPT_MAESTRO_DEFINITIVO_MIRA_V2.md), que exige rutinas para todo CRUD y la matriz de doce criterios. Se conserva esta adaptación para trazabilidad, sin atribuirla al usuario como texto original.

## Auditoría integral, implementación funcional, estabilización y preparación para sustentación

Adaptado al repositorio MIRA a partir del prompt adjuntado por el usuario. El [original de Backstage](referencias/PROMPT_MAESTRO_BACKSTAGE_V2.md) se conserva como referencia histórica. Este documento es una instrucción de trabajo reutilizable, no una declaración de funcionalidades terminadas ni un sustituto del modelo oficial.

## 1. Rol y misión

Actúa con criterio de arquitectura, desarrollo full-stack, ingeniería de datos, operación, seguridad, calidad y documentación técnica.

Trabaja directamente en el repositorio existente de MIRA (Módulo de Investigación para la Gestión de Proyectos Académicos), de la Universidad de San Buenaventura Medellín. La misión es mantener y completar lo alcanzable del alcance aprobado con persistencia real, integración verificable y evidencia para su sustentación. Inspecciona, implementa, ejecuta pruebas y documenta; no entregues únicamente recomendaciones.

No reconstruyas la aplicación ni reemplaces su arquitectura sin evidencia técnica. No sustituyas funcionalidades por mocks ni números inventados. Los dobles de pruebas pueden aislar componentes, pero no acreditan integración real.

## 2. Alcance y fuentes de verdad

Antes de modificar código:

1. Revisa Git, la rama y las instrucciones locales. Conserva los cambios del usuario.
2. Lee `README.md`, `docs/1_constitution.md` y `docs/fuentes/README.md`.
3. Consulta el Spec Kit correspondiente en `versiones/`, incluidos especificación, plan, contratos y tareas.
4. Contrasta `docs/auditoria-v2.md`, `docs/trazabilidad-v2.md`, `docs/pruebas-v2.md` y `docs/pendientes-v2.md` con el código y el entorno actuales.
5. Identifica requisitos oficiales ausentes y contradicciones; no conviertas un supuesto o una plantilla histórica en un requisito aprobado.

Contexto que debes verificar mediante inspección:

- API C# / ASP.NET Core .NET 10, PostgreSQL con Npgsql/Dapper y frontend Flask/Jinja/Bootstrap.
- Seis catálogos V1: áreas de conocimiento, ODS, áreas de aplicación, términos clave, universidades y líneas de investigación.
- V1 y V2 abren directamente, sin autenticación ni autorización. No añadir login, JWT, gestión de contraseñas, usuarios o restricciones por sesión como parte de estas versiones.
- La migración 003 conserva tablas y rutinas `usuario`→`usuario_rol` solo en PostgreSQL. Su existencia no autoriza una pantalla de usuarios ni demuestra autenticación.
- El modelo relacional académico oficial de la Entrega 2 está pendiente según la documentación actual. Comprueba si ya se incorporó antes de declararlo bloqueado o empezar a implementarlo.
- No existen módulos de Backstage, mapas, motores geoespaciales, SOI o CleanCom en el alcance de MIRA.

## 3. Auditoría inicial

Inspecciona estructura, frameworks, dependencias declaradas e instaladas, configuración, API, servicios, repositorios, modelos, frontend, migraciones, scripts y pruebas.

Para cada catálogo recorre: listado, alta, consulta individual, edición, inactivación y actualización del dashboard. Identifica formularios o botones desconectados, respuestas incompatibles, estados obsoletos, errores silenciosos, datos simulados y rutas que no funcionan.

Actualiza `docs/auditoria-v2.md` con evidencia y rutas. Clasifica cada funcionalidad como operativa y verificada, parcial, defectuosa, simulada/desconectada, pendiente o bloqueada por dependencia externa. Registra por separado hallazgo inicial y estado después de corregirlo.

No des por válido un indicador porque aparezca en pantalla: comprueba su procedencia. Distingue semillas persistidas en PostgreSQL de datos ficticios generados en el frontend.

## 4. Especificaciones y trazabilidad

Conserva la organización SDD de `versiones/`; no atribuyas integración con GitHub Spec Kit si solo existen documentos mantenidos manualmente. Actualiza la especificación antes de cambiar funcionalidad.

Reutiliza los IDs existentes (`RF-MIRA-*`, `RNF-MIRA-*`, `RF-V2-*`). Si falta uno, asígnale un identificador único y define comportamiento y criterios de aceptación. Mantén trazabilidad requisito → especificación → archivo/función → prueba → resultado.

Actualiza `docs/trazabilidad-v2.md`, registra decisiones en `docs/arquitectura-v2.md` y mantén tareas coherentes con lo implementado. Distingue descripción retrospectiva del código de requisitos oficiales aprobados.

## 5. Operatividad de V1

Verifica extremo a extremo los seis catálogos:

- Campos obligatorios, normalización y límites de longitud alineados con las columnas reales.
- SQL parametrizado y manejo de valores que contienen caracteres especiales.
- Alta persistida y recuperable desde un cliente nuevo.
- Edición de datos y actualización del listado.
- Inactivación que conserva la fila y la excluye de consultas de activos.
- Clave textual de término inmutable, URL correcta y manejo de duplicados activos, inactivos y concurrentes sin error interno.
- Contratos de respuesta, `Location`, validaciones y recursos inexistentes.
- Conteos reales del dashboard, ausencia de datos y fallos de API claramente diferenciados.
- Mensajes de validación/conflicto conservados en HTTP 400/409, sin confundirlos con una desconexión.
- Respuestas no JSON o incompatibles sin provocar un error 500 del frontend ni perder los valores del formulario.

Recorre el flujo completo: interfaz → CSRF/validación → API → servicio → repositorio → PostgreSQL → respuesta → interfaz. Corrige contratos y fallos concretos con pruebas de regresión.

## 6. V2 académica y rutinas PostgreSQL

Si falta el modelo oficial, documenta exactamente qué entidades, relaciones o reglas impide implementar y continúa con tareas independientes. No inventes maestro-detalle a partir de seis catálogos independientes.

Cuando exista el modelo y el alcance esté definido:

1. Identifica PK/FK, cardinalidades, restricciones y borrado lógico.
2. Define entradas y salidas de las rutinas, incluido JSONB para datos compuestos.
3. Implementa alta y modificación de maestro y detalles de forma atómica en una única rutina PostgreSQL; no hagas escrituras independientes desde la API.
4. Implementa consulta, listado e inactivación conforme a los contratos.
5. Valida FK en el servidor y presenta selectores con etiquetas comprensibles en la interfaz.
6. Añade triggers y vistas solo cuando una regla real o consulta recurrente los justifique.
7. Prueba fallos de validación/FK y rollback: no deben quedar maestros o detalles parcialmente escritos.

Conserva las rutinas y datos legados de la migración 003. No afirmes que comprobar `usuario`→roles completa los flujos académicos de V2.

## 7. Arquitectura, seguridad e interfaz

Mantén Controller → Service → Repository → PostgreSQL. Solo Repository accede a la base. Conserva DI, SQL parametrizado, validación e integridad; aplica SOLID y YAGNI sin refactorizaciones masivas.

Verifica escape HTML, CSRF en todos los POST del frontend, rechazo de tokens ausentes/incorrectos/no ASCII, flags de cookie y manejo de secretos. La cookie CSRF no identifica a un usuario. No introduzcas autenticación para satisfacer secciones del prompt original que no corresponden a MIRA.

Revisa exposición de red, CORS, errores internos y dependencias según el alcance de la tarea. No declares ausencia de vulnerabilidades por haber compilado o comparado versiones; si no se ejecuta un escaneo actualizado, indícalo.

Conserva la identidad visual MIRA y la navegación existente. Comprueba estados de éxito, error y vacío, modales, diseño responsivo y accesibilidad. Un cliente HTTP no acredita funcionamiento del JavaScript ni una revisión visual; declara esa limitación si no tienes un navegador disponible.

## 8. Operación y persistencia

Mantén instrucciones reproducibles en `scripts/README.md` y el README principal. Consulta `scripts/start-local.ps1` y `scripts/stop-local.ps1` para Windows sin Docker.

Respeta `schema_migrations`: las migraciones pendientes y su registro deben aplicarse transaccionalmente. No borres bases o volúmenes para resolver errores. Comprueba persistencia tras reiniciar PostgreSQL, API y frontend con un registro de prueba identificable y conserva los datos de usuarios.

Distingue PostgreSQL 16 y Python 3.11 declarados para Docker del entorno local realmente instalado. No extrapoles pruebas de una versión a otra. `/health` solo acredita vida del proceso salvo que su implementación demuestre otra cosa.

## 9. Pruebas reales

Usa las suites existentes y añade pruebas para defectos y riesgos concretos:

- API: `dotnet test MIRA.sln`. Para integración usa `MIRA_TEST_DB` apuntando a una base exclusiva con migraciones aplicadas; no uses la base operativa para suites que eliminan datos de prueba.
- Frontend: desde `frontend/`, `python -m pytest tests/test_app.py -q`, con las dependencias de `frontend/requirements-dev.txt`.
- E2E HTTP: configura `MIRA_E2E_URL` y `MIRA_E2E_API_URL` y ejecuta `python -m pytest frontend/tests/e2e -q` desde la raíz con la plataforma iniciada. Usa registros identificables y su limpieza prevista.
- SQL: `database/tests/v2_usuarios_roles.sql` en una base exclusiva, respetando su rollback.
- Verificación local integrada: consulta y ejecuta `scripts/verify-local.ps1` cuando corresponda; conserva su separación de base de pruebas y plataforma operativa.
- Documentación: comprueba coherencia, enlaces relativos y `git diff --check`. No arranques servicios ni repitas toda la suite por cambios exclusivamente documentales.

Documenta comando, entorno, resultado y omisiones en `docs/pruebas-v2.md`. No copies conteos de ejecuciones anteriores como si acabaran de pasar. No declares pruebas aprobadas si fueron omitidas o no pudieron ejecutarse.

## 10. Documentación para sustentación

Mantén estos entregables basados en el repositorio real:

- `docs/auditoria-v2.md`: inventario, hallazgos, correcciones y clasificación funcional.
- `docs/arquitectura-v2.md`: capas, responsabilidades, flujos y decisiones verificables.
- `docs/trazabilidad-v2.md`: requisitos, especificación, código y pruebas.
- `docs/pruebas-v2.md`: ejecuciones, resultados, entorno y límites.
- `docs/sustentacion-v2.md`: objetivo, ejemplos concretos, demostración y limitaciones.
- `docs/pendientes-v2.md`: prioridad, bloqueo y siguiente paso verificable.

En los ejemplos de sustentación incluye requisito, documento, archivo/función, entrada, procesamiento, salida y prueba. Explica la relación entre prompts, especificaciones y código sin presentar documentación como evidencia de implementación.

## 11. Priorización y ejecución

Trabaja en ciclos de inspección → requisito → corrección → prueba → integración → documentación.

- P0: fallos que impiden iniciar, persistir o utilizar los flujos esenciales definidos.
- P1: catálogos defectuosos, errores internos, contratos incompatibles o integración incompleta del alcance aprobado.
- P2: mensajes, estados, casos de frontera, operación, experiencia de usuario y mantenibilidad.
- P3: optimizaciones y extensiones futuras justificadas.

No te detengas en recomendaciones si puedes corregir un defecto dentro del alcance. Ante un bloqueo externo continúa con trabajo independiente y explica la dependencia concreta. Evita repetir verificaciones ya suficientes salvo cambios, fallos o riesgos nuevos que lo justifiquen.

## 12. Criterios de aceptación

Solo declara operativos los componentes que hayas comprobado. Deben compilar, arrancar con instrucciones reproducibles, persistir datos, completar sus flujos críticos, manejar errores relevantes y mantener trazabilidad entre requisitos, código y pruebas.

No declares completa la entrega académica V2 mientras falten su modelo oficial o sus flujos especificados. Distingue operatividad local, preparación para despliegue y producción. No declares producción sin verificar infraestructura, seguridad, observabilidad y despliegue.

No elimines datos ni controles de seguridad para hacer pasar pruebas. No ejecutes migraciones destructivas, fusiones a la rama estable, despliegues productivos ni servicios de pago sin autorización aplicable. Respeta la autorización ya dada por el usuario para registrar o subir cambios al destino y rama acordados; no la extiendas a acciones distintas.

## 13. Entrega final

Resume el comportamiento corregido, los archivos afectados, las pruebas realmente ejecutadas, los resultados y los bloqueos. Deja enlaces a auditoría, sustentación y pendientes, e instrucciones de arranque vigentes. Si subes cambios con autorización, identifica repositorio, rama y commit.

Comienza por inspeccionar el repositorio y continúa hasta completar los criterios alcanzables en el entorno disponible. El resultado debe ser MIRA funcional y sustentable dentro de su alcance comprobado, con las limitaciones explícitas.
