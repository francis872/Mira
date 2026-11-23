# PROMPT MAESTRO DEFINITIVO — MIRA V2
## Implementación integral, cumplimiento académico, arquitectura, PostgreSQL, interfaz y sustentación

### 0. ROL Y MISIÓN

Actúa como un equipo de ingeniería de software compuesto por:

- Arquitecto de software especializado en .NET 10 y ASP.NET Core.
- Ingeniero backend especializado en C#, Dapper y Npgsql.
- Ingeniero de bases de datos PostgreSQL 16.
- Desarrollador frontend especializado en Flask, Jinja y Bootstrap 5.
- Ingeniero de aseguramiento de calidad y pruebas automatizadas.
- Especialista en Spec-Driven Development y documentación técnica.
- Diseñador UI/UX de plataformas académicas.

Trabajarás directamente sobre el repositorio existente de **MIRA — Módulo de Investigación para la Gestión de Proyectos Académicos**, de la Universidad de San Buenaventura Medellín.

**MISIÓN PRINCIPAL: implementar completamente la Versión 2 de MIRA conforme a los requisitos oficiales del profesor, sin inventar funcionalidades, sin alterar el alcance académico y sin deteriorar lo construido en V1.**

No quiero únicamente recomendaciones, diagnósticos, pseudocódigo o un plan de trabajo. Quiero implementación real, integración funcional, pruebas ejecutadas y documentación actualizada.

Trabaja de manera autónoma y secuencial hasta completar todas las tareas técnicamente posibles. Si una actividad depende de información oficial ausente, registra el bloqueo y continúa con las actividades independientes.

---

# 1. JERARQUÍA DE FUENTES Y RECONOCIMIENTO

Antes de modificar cualquier archivo:

1. Ejecuta `git status`.
2. Conserva los cambios existentes.
3. Lee `README.md`.
4. Lee `docs/1_constitution.md`.
5. Lee `docs/fuentes/README.md`.
6. Inspecciona `versiones/` y el Spec Kit.
7. Consulta `prompts/README.md`.
8. Revisa `prompts/PROMPT_MAESTRO_MIRA_V2.md`.
9. Revisa `auditoria-v2.md`, `trazabilidad-v2.md`, `pruebas-v2.md` y `pendientes-v2.md`.
10. Examina el código, las migraciones, las pruebas y los scripts de arranque.

Busca también, si están disponibles, los documentos oficiales:

- `0_METODOLOGIA.md`
- `VERSION_2.md`
- `0_mapa_versiones.md`
- Los documentos `modulo_*.md`
- `proyecto_completo.md`
- El esquema relacional oficial de Entrega 2.

No asumas que estos archivos existen en MIRA con las mismas rutas del repositorio de ejemplo.

## Jerarquía de autoridad

1. Instrucciones oficiales del profesor y rúbrica de evaluación.
2. Modelo relacional y mapa oficial de versiones correspondiente a MIRA.
3. Constitución y Spec Kit aprobado.
4. Decisiones técnicas documentadas del proyecto.
5. Implementación existente.
6. Prompts de trabajo y propuestas de mejora.

Cuando existan contradicciones, identifica la discrepancia y aplica la fuente oficial pertinente. No modifiques silenciosamente una especificación académica para justificar código existente.

El documento VERSION_2.md proporcionado como referencia exige doce criterios de aceptación. Deben utilizarse como requisitos obligatorios de V2.

---

# 2. RESTRICCIONES ARQUITECTÓNICAS

Conserva el stack tecnológico:

**Backend**
- C#.
- ASP.NET Core.
- .NET 10.
- Dapper.
- Npgsql.

**Base de datos**
- PostgreSQL 16.
- Migraciones controladas.
- Funciones y procedimientos almacenados.
- Triggers.
- Restricciones de integridad.

**Frontend**
- Python.
- Flask.
- Jinja.
- Bootstrap 5.
- JavaScript cuando sea necesario.

Arquitectura obligatoria:

Frontend → Controller → Service → Repository → PostgreSQL.

## Responsabilidades

### Controller

Debe encargarse de:

- Recibir solicitudes HTTP.
- Validar el contrato de entrada.
- Invocar servicios.
- Traducir resultados a respuestas HTTP.
- Devolver respuestas consistentes.

No debe contener consultas SQL ni acceder directamente a PostgreSQL.

### Service

Debe encargarse de:

- Reglas de negocio.
- Validaciones semánticas.
- Verificación de referencias activas cuando corresponda.
- Coordinación de operaciones.
- Condiciones para creación, actualización y eliminación.

No debe ejecutar SQL directamente.

### Repository

Debe encargarse exclusivamente de:

- Invocar las rutinas almacenadas PostgreSQL.
- Enviar parámetros mediante Dapper/Npgsql.
- Recuperar los resultados.
- Mapearlos a los modelos correspondientes.

**No debe contener SELECT, INSERT, UPDATE o DELETE escritos directamente para ejecutar el CRUD.**

La lógica SQL del CRUD debe residir en las rutinas PostgreSQL.

### PostgreSQL

Debe contener:

- Persistencia.
- Consultas SQL.
- Rutinas almacenadas.
- Restricciones de integridad.
- Triggers.
- Transacciones maestro-detalle.

No confundas definir una rutina en PostgreSQL con invocarla desde Repository.

---

# 3. CIERRE Y CONSERVACIÓN DE MIRA V1

Antes de implementar las funcionalidades nuevas, verifica que V1 continúe operativa.

MIRA V1 contiene seis catálogos:

1. Áreas de aplicación.
2. Áreas de conocimiento.
3. Líneas de investigación.
4. Objetivos de desarrollo sostenible.
5. Términos clave.
6. Universidades.

Para cada catálogo verifica:

- Listar.
- Consultar por identificador.
- Crear.
- Actualizar.
- Eliminar o retirar según el esquema.
- Validaciones.
- Integridad.
- Funcionamiento desde la interfaz.
- Conteos existentes del dashboard.

No elimines funcionalidades de V1.

Adapta el CRUD existente para cumplir el requisito de utilizar rutinas PostgreSQL en las cinco operaciones, preservando el comportamiento y los datos.

Ejecuta regresión antes y después de las modificaciones.

---

# 4. ALCANCE REAL DE MIRA V2

La Versión 2 incorpora las tablas del módulo de Investigación que contienen claves foráneas, incluidas las tablas puente que correspondan al modelo oficial.

Inspecciona el mapa de versiones y el esquema relacional.

El documento de referencia identifica las siguientes relaciones maestro-detalle para Investigación:

- universidad → grupo_investigacion.
- grupo_investigacion → semillero.
- linea_investigacion → docente.

Estas relaciones son referencias académicas explícitas, pero sus nombres, claves, columnas, cardinalidades y reglas deben contrastarse con el esquema oficial específico de MIRA.

No inventes columnas, relaciones o entidades.

Si falta el modelo relacional de Entrega 2, busca primero si existe en otra ubicación del repositorio.

Si no está disponible, documenta qué partes no pueden implementarse con seguridad y continúa con las tareas independientes.

## Para cada tabla V2 oficialmente definida

Implementa:

1. Listado.
2. Consulta individual.
3. Creación.
4. Actualización.
5. Eliminación física o lógica según el esquema.
6. Validaciones.
7. Integridad referencial.
8. Rutinas PostgreSQL.
9. Repository.
10. Service.
11. Controller.
12. Interfaz funcional.
13. Pruebas.

No consideres terminada una tabla si únicamente existe su endpoint.

---

# 5. CRUD MEDIANTE RUTINAS ALMACENADAS

Todas las operaciones CRUD de V1 y V2 deben ejecutarse mediante rutinas PostgreSQL.

Utiliza una convención consistente:

- `sp_listar_<tabla>`
- `sp_consultar_<tabla>`
- `sp_crear_<tabla>`
- `sp_actualizar_<tabla>`
- `sp_eliminar_<tabla>`

Respeta los nombres existentes cuando renombrarlos genere incompatibilidades injustificadas. Documenta cualquier excepción.

En PostgreSQL pueden utilizarse funciones invocadas mediante SELECT o procedimientos invocados mediante CALL, según corresponda.

No mezcles arbitrariamente ambas formas.

## Listar

Debe:

- Recuperar registros reales.
- Aplicar los filtros definidos.
- Excluir inactivos cuando corresponda.
- Mantener el contrato de respuesta de la API.

## Consultar

Debe:

- Recuperar un registro por identificador.
- Respetar las reglas de visibilidad.
- Permitir distinguir un recurso inexistente.

## Crear

Debe:

- Validar entradas.
- Ejecutar la rutina correspondiente.
- Devolver la fila realmente almacenada.
- Incluir valores por defecto y cambios realizados por triggers.
- Responder HTTP 201 cuando corresponda.

## Actualizar

Debe:

- Verificar la existencia del registro.
- Aplicar validaciones.
- Ejecutar la rutina.
- Devolver el estado persistido cuando el contrato lo requiera.

## Eliminar

Debe:

- Respetar el tipo de borrado definido.
- Aplicar las reglas de negocio.
- Ejecutar la rutina.
- Mantener la integridad referencial.

Si existe la columna `activo` y el esquema define borrado lógico, realiza el retiro mediante UPDATE dentro de la rutina.

No conviertas indiscriminadamente todos los borrados en físicos.

---

# 6. MAESTRO-DETALLE OBLIGATORIO

Este es uno de los puntos críticos señalados por el profesor.

**Una clave foránea por sí sola no constituye una operación maestro-detalle completa.**

Una implementación válida debe permitir administrar un maestro y sus renglones de detalle desde la misma pantalla.

## Comportamiento requerido

Al abrir un registro maestro:

- Mostrar sus datos.
- Mostrar los detalles asociados.
- Permitir agregar renglones.
- Permitir editar detalles cuando corresponda.
- Permitir retirar renglones.
- Mantener la asociación automáticamente.
- Guardar varios detalles en una única operación.

No obligues al usuario a salir de la pantalla y volver a seleccionar el maestro.

## Persistencia atómica

Cuando se guarden conjuntamente maestro y detalles, utiliza una única rutina PostgreSQL para realizar toda la escritura.

Ejemplo conceptual:

- Crear maestro.
- Obtener identificador generado.
- Insertar detalle 1.
- Insertar detalle 2.
- Insertar detalle 3.
- Confirmar la operación completa.

Si falla el tercer detalle, deben revertirse todas las escrituras de esa operación.

No implementes una petición HTTP independiente por cada renglón.

No realices escrituras separadas desde Repository.

Utiliza JSONB para transportar colecciones de detalles cuando resulte adecuado al contrato y a las especificaciones.

La atomicidad debe comprobarse mediante pruebas reales.

## Requisitos del formulario

El formulario debe permitir:

- Agregar renglón.
- Eliminar renglón.
- Editar valores.
- Validar datos.
- Guardar todos los renglones.
- Mostrar errores comprensibles.
- Consultar los detalles existentes.

El frontend no debe simular una operación maestro-detalle si el backend no la soporta.

---

# 7. TRIGGERS POSTGRESQL

Implementa y demuestra al menos un trigger funcional, conforme al esquema y a las reglas académicas.

Primero inspecciona los triggers existentes.

Si ya existe uno que cumple el criterio, consérvalo y comprueba su funcionamiento.

Si no existe, selecciona una funcionalidad respaldada por el modelo:

- Actualización automática de fecha de modificación.
- Bitácora de retiros.
- Conteo de detalles.
- Validación entre maestro y detalle.
- Estado derivado de registros relacionados.

No agregues columnas artificiales únicamente para demostrar un trigger.

## Caso especial: borrado lógico

Si un trigger mantiene conteos o estados derivados de detalles activos, debe considerar que un borrado lógico es un UPDATE.

Verifica los eventos INSERT, UPDATE y DELETE según corresponda.

Prueba que retirar lógicamente un detalle actualice correctamente los valores derivados.

## Evidencia

Demuestra que:

1. El usuario realiza una operación desde la interfaz.
2. PostgreSQL ejecuta el trigger.
3. El dato derivado cambia.
4. La API no envió manualmente el valor derivado.

---

# 8. CLAVES FORÁNEAS EN LA INTERFAZ

Ninguna clave foránea debe introducirse manualmente como código numérico o identificador arbitrario.

Utiliza controles de selección cargados desde la API.

## Reglas

- Mostrar nombres o descripciones comprensibles.
- Enviar el identificador correspondiente.
- Cargar opciones reales desde la API.
- Excluir registros inactivos cuando corresponda.
- Manejar estados vacíos.
- Manejar errores de carga.
- Permitir null cuando la clave foránea sea opcional.

No envíes cadenas vacías cuando se espera null.

En formularios maestro-detalle, la clave del maestro debe quedar asociada automáticamente al contexto de la pantalla.

No obligues al usuario a volver a seleccionar el padre.

---

# 9. TABLAS PUENTE

Identifica las tablas puente oficialmente definidas.

Verifica:

- Claves primarias compuestas.
- Dos claves foráneas.
- Asignación de relaciones.
- Retiro de relaciones.
- Prevención de duplicados.
- Respuesta HTTP 409 cuando corresponda.

No implementes un formulario de edición de la pareja de claves si el contrato académico establece que la asociación simplemente existe o no existe.

La interfaz debe permitir asignar y retirar asociaciones claramente.

---

# 10. VALIDACIONES Y REGLAS DE NEGOCIO

El profesor exige que cada integrante pueda identificar dónde están programadas las reglas de negocio.

Para cada entidad documenta:

- Validaciones de entrada.
- Reglas de Service.
- Restricciones PostgreSQL.
- Validaciones de referencias.
- Reglas de borrado.
- Reglas de actualización.
- Mensajes de error.

Verifica:

- Campos obligatorios.
- Formatos.
- Longitudes.
- Duplicados.
- Claves foráneas inexistentes.
- Referencias inactivas.
- Restricciones de integridad.
- Estados no permitidos.

Cuando el esquema no defina una regla, no la inventes.

Si se requiere decidir si un maestro con detalles activos puede retirarse lógicamente, documenta la decisión y su justificación antes de implementarla.

---

# 11. RESPUESTAS HTTP E INTEGRIDAD REFERENCIAL

Implementa un manejo consistente de errores.

Códigos esperados:

- 200: operación exitosa.
- 201: recurso creado.
- 204: operación exitosa sin contenido.
- 400: solicitud inválida.
- 404: recurso inexistente.
- 409: conflicto de integridad o estado.
- 500: error inesperado del servidor.

Traduce los errores PostgreSQL pertinentes:

- SQLSTATE 23503: violación de clave foránea.
- SQLSTATE 23505: violación de unicidad.
- SQLSTATE 23502: violación de campo NOT NULL.

Conforme al criterio académico, las violaciones de integridad referencial y los duplicados deben responder 409 cuando correspondan.

Los campos obligatorios ausentes o inválidos deben tratarse como errores de validación de entrada, normalmente 400.

No conviertas indiscriminadamente todos los errores de PostgreSQL en 409.

No expongas trazas internas, cadenas de conexión ni detalles sensibles.

Los mensajes de interfaz deben estar en español y ser comprensibles para usuarios no técnicos.

No muestres mensajes como:

- Foreign key violation.
- SQLSTATE 23503.
- HTTP 422.
- Null reference exception.

En su lugar, comunica qué ocurrió y qué puede hacer el usuario.

---

# 12. MODERNIZACIÓN DE INTERFAZ UI/UX

Moderniza la interfaz existente sin cambiar el stack ni ampliar el alcance funcional de V2.

## Identidad visual

Diseña una plataforma académica moderna, elegante y tecnológica.

Características:

- Interfaz limpia.
- Navegación intuitiva.
- Tipografía legible.
- Jerarquía visual consistente.
- Colores institucionales utilizados con moderación.
- Componentes uniformes.
- Diseño responsive.
- Accesibilidad básica.
- Estados de interacción claros.

## Componentes

Mejora:

- Navegación principal.
- Listados.
- Formularios.
- Tablas.
- Desplegables.
- Paginación cuando corresponda.
- Confirmaciones.
- Mensajes de éxito.
- Mensajes de error.
- Estados vacíos.
- Indicadores de carga.
- Formularios maestro-detalle.

## Restricciones

No implementes:

- Landing pages corporativas.
- Publicaciones institucionales.
- Gráficos avanzados de V4.
- Funcionalidades de autenticación de V3.
- Componentes decorativos sin utilidad.
- Datos ficticios.

Conserva los conteos reales del dashboard existente sin ampliar su alcance a analítica de V4.

No sacrifiques funcionalidad por estética.

---

# 13. SEGURIDAD Y CALIDAD

Conserva:

- SQL parametrizado.
- Protección CSRF.
- Escape HTML.
- Validaciones del servidor.
- Manejo seguro de excepciones.
- Integridad referencial.
- Configuración mediante variables de entorno.

No publiques secretos.

No agregues autenticación, JWT, contraseñas, sesiones de usuario ni permisos por rol.

V1 y V2 deben continuar accesibles sin login.

Preserva las estructuras usuario, rol y usuario_rol de la migración 003 y sus datos existentes, pero no integres su CRUD a las pantallas de V2.

---

# 14. SPEC KIT Y DOCUMENTACIÓN

La documentación debe permitir demostrar la interpretabilidad exigida por el profesor.

Prepara el Spec Kit V2 conforme a la estructura oficial de nueve documentos utilizada por el curso, sin inventar nombres si no están definidos.

Incluye o actualiza según corresponda:

- Especificaciones.
- Historias de usuario.
- Criterios de aceptación.
- Diagramas de secuencia.
- Arquitectura.
- Contratos API.
- Reglas de negocio.
- Modelo relacional.
- Plan de pruebas.
- Guía de implementación con IA.

El Spec Kit debe reflejar las decisiones que guían la implementación.

No lo redactes al final únicamente para justificar código ya escrito.

## Prompts

Conserva los prompts originales utilizados por cada integrante.

Distingue entre:

- Prompt inicial auténtico.
- Prompt de corrección.
- Prompt de refactorización.
- Prompt maestro actual.
- Prompts de IDE agéntico.

No fabriques retrospectivamente prompts ni los atribuyas a integrantes que no los utilizaron.

Documenta qué instrucciones produjeron cambios y cuáles corrigieron errores de la IA.

## Trazabilidad

Para cada requisito genera:

Requisito → Historia de usuario → Especificación → Prompt → Controller → Service → Repository → Rutina PostgreSQL → Interfaz → Prueba.

Incluye rutas reales de archivos y métodos.

---

# 15. PRUEBAS OBLIGATORIAS

No basta con que la solución compile.

Ejecuta las pruebas disponibles y agrega las necesarias para demostrar los criterios oficiales.

## API

Ejecuta:

`dotnet test MIRA.sln`

Para integración con PostgreSQL utiliza MIRA_TEST_DB y una base de pruebas con migraciones aplicadas.

## Frontend

Instala las dependencias de `frontend/requirements-dev.txt`.

Ejecuta las pruebas de `frontend/tests/test_app.py`.

## End-to-end

Cuando el entorno esté disponible, configura:

- MIRA_E2E_URL.
- MIRA_E2E_API_URL.

Ejecuta las pruebas de `frontend/tests/e2e`.

## PostgreSQL

Ejecuta las pruebas SQL correspondientes en una base de pruebas.

Incluye las existentes de `database/tests/v2_usuarios_roles.sql` cuando corresponda, sin confundirlas con el CRUD de V2.

## Casos críticos

Verifica:

1. CRUD V1.
2. CRUD V2.
3. Cinco operaciones mediante rutinas PostgreSQL.
4. Trigger funcional.
5. Desplegables cargados desde API.
6. Claves opcionales con null.
7. Integridad referencial con 409.
8. Tablas puente y duplicados.
9. Maestro-detalle en una sola pantalla.
10. Borrado lógico.
11. Mensajes comprensibles.
12. Regresión completa de V1.
13. Atomicidad de escrituras maestro-detalle.
14. Navegación y formularios.
15. Correspondencia entre documentación y código.

Registra los resultados reales.

No marques pruebas como aprobadas si no se ejecutaron.

---

# 16. MATRIZ DE LOS DOCE CRITERIOS OFICIALES

Construye una matriz verificable con estos criterios:

C01. CRUD V1 completo desde interfaz.

C02. CRUD V2 completo desde interfaz.

C03. Las cinco operaciones CRUD utilizan rutinas almacenadas.

C04. Al menos un trigger funciona y puede demostrarse.

C05. Todas las claves foráneas se seleccionan desde desplegables API.

C06. Las claves opcionales admiten null.

C07. Violaciones de integridad referencial responden 409 en español.

C08. Tablas puente permiten asignar y retirar; duplicados producen 409.

C09. Los detalles se visualizan y agregan desde la pantalla del maestro.

C10. El borrado lógico filtra registros retirados.

C11. La interfaz utiliza lenguaje comprensible.

C12. La regresión V1 se supera completamente.

Para cada criterio registra:

- Estado.
- Requisito.
- Implementación.
- Archivos involucrados.
- Prueba.
- Resultado.
- Evidencia.
- Bloqueos.

Estados permitidos:

- Pendiente.
- En implementación.
- Implementado sin verificar.
- Verificado.
- Bloqueado.

No utilices el estado Verificado sin una prueba o comprobación real.

---

# 17. PREPARACIÓN PARA SUSTENTACIÓN

Genera una guía técnica para que cualquier integrante pueda explicar el sistema completo.

Debe responder:

1. ¿Dónde está implementado el GET de una tabla?
2. ¿Dónde está el POST?
3. ¿Dónde se ejecuta el borrado?
4. ¿Dónde están las reglas de negocio?
5. ¿Cómo se comprueban las claves foráneas?
6. ¿Dónde están los procedimientos almacenados?
7. ¿Dónde se invocan desde Repository?
8. ¿Qué maestro y detalle se utilizan?
9. ¿Cómo se agregan varios renglones?
10. ¿Cómo se garantiza la atomicidad?
11. ¿Dónde está el trigger?
12. ¿Qué sucede ante una violación de integridad?
13. ¿Por qué se devuelve 409 y no 500?
14. ¿Qué hace el frontend?
15. ¿Cómo se relaciona el código con el Spec Kit?
16. ¿Qué prompts se utilizaron?
17. ¿Cómo se ejecutan las pruebas?

No entregues explicaciones genéricas.

Cada respuesta debe referenciar archivos y métodos reales del repositorio.

---

# 18. ORGANIZACIÓN DEL PROYECTO

Revisa la estructura del repositorio.

Mejora la organización cuando sea necesario, preservando las convenciones existentes.

Mantén separados:

- Backend.
- Frontend.
- Migraciones.
- Rutinas PostgreSQL.
- Pruebas.
- Scripts.
- Spec Kit.
- Prompts.
- Evidencias.
- Documentación.

No crees carpetas duplicadas ni reestructuraciones innecesarias.

Corrige referencias y enlaces afectados.

---

# 19. EJECUCIÓN LOCAL

Mantén instrucciones reproducibles.

Para Windows sin Docker:

- Consulta `scripts/README.md`.
- Utiliza `scripts/start-local.ps1`.
- Utiliza `scripts/stop-local.ps1`.

No introduzcas Docker como dependencia obligatoria.

No elimines bases de datos existentes ni reinicies volúmenes destructivamente.

Respeta las migraciones y su registro en `schema_migrations`.

---

# 20. ESTRATEGIA DE EJECUCIÓN

Ejecuta el trabajo en el siguiente orden:

**Etapa 1 — Auditoría**
Inspecciona repositorio, Spec Kit, esquema, migraciones y pruebas.

**Etapa 2 — Cierre V1**
Corrige regresiones y verifica los seis catálogos.

**Etapa 3 — Persistencia**
Implementa o corrige rutinas PostgreSQL para las cinco operaciones CRUD.

**Etapa 4 — Arquitectura**
Corrige Controller, Service y Repository para que cumplan sus responsabilidades.

**Etapa 5 — V2**
Implementa las entidades con claves foráneas y las tablas puente oficialmente definidas.

**Etapa 6 — Maestro-detalle**
Implementa formularios, operaciones conjuntas y persistencia atómica para las relaciones oficiales.

**Etapa 7 — Triggers**
Implementa o verifica al menos un disparador funcional.

**Etapa 8 — Integridad**
Corrige validaciones, códigos HTTP y manejo de errores.

**Etapa 9 — Interfaz**
Moderniza las pantallas de V1 y V2 sin ampliar el alcance.

**Etapa 10 — Pruebas**
Ejecuta regresión, integración y pruebas funcionales.

**Etapa 11 — Documentación**
Completa trazabilidad, evidencias, Spec Kit y guía de sustentación.

En cada etapa:

1. Diagnostica.
2. Identifica los requisitos aplicables.
3. Actualiza primero la especificación cuando cambie el comportamiento.
4. Implementa.
5. Ejecuta pruebas.
6. Corrige defectos.
7. Documenta resultados.
8. Continúa.

No te detengas tras presentar un diagnóstico o una propuesta.

Implementa todo lo que esté suficientemente definido y pueda verificarse de manera segura.

---

# 21. CONDICIONES DE FINALIZACIÓN

MIRA V2 únicamente puede declararse completa cuando:

- Los doce criterios oficiales estén verificados.
- V1 siga funcionando.
- Las tablas V2 estén operativas.
- El CRUD pase por rutinas PostgreSQL.
- Exista un trigger comprobable.
- Las claves foráneas se gestionen correctamente.
- Las relaciones maestro-detalle funcionen.
- La persistencia conjunta sea atómica.
- Los errores HTTP sean correctos.
- La interfaz esté integrada.
- Las pruebas relevantes estén aprobadas.
- El Spec Kit corresponda al código.
- Los prompts auténticos estén organizados.
- Exista evidencia para sustentación.

Si una dependencia documental o del entorno impide cumplir algún criterio, informa el bloqueo. No declares terminada la versión.

No crees commits, tags, push ni merges sin autorización explícita.

---

# 22. INFORME FINAL OBLIGATORIO

Al terminar presenta:

1. Estado general de MIRA V2.
2. Archivos modificados.
3. Requisitos implementados.
4. Cambios arquitectónicos.
5. Rutinas PostgreSQL implementadas.
6. Triggers verificados.
7. Relaciones maestro-detalle operativas.
8. Mejoras de interfaz.
9. Resultados de pruebas.
10. Estado de los doce criterios.
11. Pendientes y bloqueos.
12. Instrucciones de ejecución.
13. Guía de sustentación.

Actualiza los documentos de auditoría, trazabilidad, pruebas y pendientes existentes.

**ORDEN FINAL: comienza inspeccionando el repositorio real y continúa con la implementación integral de MIRA V2. No sustituyas el trabajo de ingeniería por una respuesta explicativa. Tu resultado debe ser código funcional, pruebas verificables y documentación académicamente consistente.**