# Guía de sustentación de MIRA

## Objetivo y estado

MIRA apoya la gestión de información de investigación académica de la Universidad de San Buenaventura Medellín. La parte demostrable actualmente gestiona seis catálogos reutilizables; no constituye todavía un sistema completo de proyectos académicos. V2 añade infraestructura de migraciones y rutinas maestro-detalle usuario→roles en PostgreSQL. Su dominio académico está pendiente del modelo oficial.

La auditoría adaptó un prompt de Backstage al repositorio real con confirmación del usuario. `docs/PROMPT.md` aporta contexto de trabajo; los requisitos viven en `versiones/`. Un prompt no sustituye la especificación ni demuestra que una funcionalidad esté terminada.

## Arquitectura que debe explicarse

Flask recibe formularios, valida CSRF y consume HTTP JSON. ASP.NET Core recibe el contrato y aplica validación de modelos. Los servicios contienen reglas y normalización. Los repositorios ejecutan SQL parametrizado con Dapper/Npgsql. PostgreSQL conserva registros, restricciones y rutinas. Consultar el diagrama y la secuencia concreta en [arquitectura-v2.md](arquitectura-v2.md).

## Ejemplos trazables

| Elemento | Universidad | Conflicto de término | Maestro-detalle de base de datos |
|---|---|---|---|
| Requisito | RF-MIRA-CAT-05 | RF-MIRA-TC-01 | RF-V2-MD-01/02/03/04 |
| Documento | `versiones/v0.1_api_inicial/2_spec.md` | `versiones/v2_profesor/2_spec.md` | Mismo Spec Kit V2 |
| Entrada | nombre, tipo y ciudad | término existente activo o inactivo | correo, hash sintético y arreglo de IDs de roles |
| Interfaz | `/universidad/nuevo` | `/termino_clave/nuevo` | Solo SQL; no hay pantalla ni endpoint |
| Código | `frontend/app.py:universidad_create`, `UniversidadController.Create`, `UniversidadService.CreateAsync`, `UniversidadRepository.CreateAsync` | `TerminoClaveService.CreateAsync`, `TerminoClaveRepository.CreateAsync`, `TerminoClaveController.Create` | `database/migrations/003_v2_usuarios_roles.sql:sp_usuario_*`, `fn_usuario_*` |
| Proceso | CSRF → JSON → validación → INSERT parametrizado → listado | Detección previa y captura de UNIQUE para inactivos/carreras | Una rutina PostgreSQL valida roles y escribe maestro y detalles atómicamente |
| Salida | HTTP 201, registro con ID; Flask redirige al listado | HTTP 409 y mensaje; no sobrescribe ni reactiva | JSONB compuesto; fallo FK no deja cambios parciales |
| Prueba | `FullLifecycle_CreateReadListUpdateLogicalDelete`, E2E universidad | `InactiveAndConcurrentDuplicateKeyword_ReturnsConflict`, E2E duplicado | `database/tests/v2_usuarios_roles.sql` |

## Demostración reproducible

1. Ejecutar el arranque local de [scripts/README.md](../scripts/README.md) y abrir `http://localhost:5000`. No se requieren credenciales de aplicación.
2. Mostrar dashboard y conteos reales. Abrir Universidades y contrastar el número de activos.
3. Crear una universidad con nombre identificable, tipo y ciudad. Recargar el listado y abrirlo en otra pestaña: el dato procede de PostgreSQL.
4. Editarla, volver al listado y mostrar el cambio. Inactivarla mediante el control de eliminación; desaparece de los activos. La prueba .NET comprueba que la fila sigue en la base.
5. Crear un término clave y volver a enviarlo. Mostrar el mensaje de duplicado. Inactivarlo e intentar crearlo otra vez: conflicto sin reactivación.
6. Consultar Swagger en `http://localhost:8081/swagger`. Mostrar contrato JSON y un 400 al enviar un texto demasiado largo.
7. Ejecutar `scripts/verify-local.ps1` y explicar sus tres niveles de pruebas y la base exclusiva para SQL/.NET. No confundir clientes HTTP con automatización visual de navegador.
8. Mostrar la matriz de trazabilidad, las decisiones de arquitectura y los pendientes de la entrega.

## Pruebas y limitaciones para declarar

La verificación final ejecutó 41 pruebas .NET con PostgreSQL real, 42 Flask/arquitectura y 31 E2E HTTP; ambos archivos SQL pasaron con rollback. Edge real superó siete comprobaciones de navegación, búsqueda, modales, formularios y menú móvil, sin excepciones JavaScript capturadas. Se comprobó persistencia después de reiniciar los tres servicios con las migraciones nuevas. Los detalles están en [pruebas-v2.md](pruebas-v2.md).

No declarar que están implementados maestro-detalle académicos, selectores FK o triggers del dominio. No presentar las tablas de usuarios como una autenticación disponible. La interfaz mantiene identidad MIRA; no incluye mapas, inteligencia geoespacial ni SOI. No afirmar despliegue, operación productiva, accesibilidad completa o ausencia de vulnerabilidades. Docker no está disponible en el entorno utilizado.

## Criterio de cierre

La sustentación puede demostrar los seis catálogos y las rutinas SQL existentes. La entrega V2 completa requiere incorporar el modelo oficial, actualizar contratos y especificaciones, implementar el dominio pendiente y volver a verificarlo. Operatividad local, preparación de despliegue y producción son estados distintos.

## Respuestas concretas para la sustentación

| Pregunta | Ruta y comportamiento demostrable |
|---|---|
| 1. ¿Dónde está GET? | `MIRA.Api/Peticiones/UniversidadController.cs:GetAll/GetById` → Service → `UniversidadRepository.GetAllActivosAsync/GetByIdAsync` → `sp_listar_universidad/sp_consultar_universidad` de 004. |
| 2. ¿Dónde está POST? | `UniversidadController.Create` → `UniversidadService.CreateAsync` → `UniversidadRepository.CreateAsync` → `sp_crear_universidad`. Devuelve 201 y fila persistida. |
| 3. ¿Dónde se ejecuta el borrado? | `UniversidadController.Delete` → `SoftDeleteAsync` → `sp_eliminar_universidad`, que hace UPDATE de activo en PostgreSQL. |
| 4. ¿Dónde están las reglas? | `MIRA.Api/Servicios/*Service.cs`, atributos en `Modelos/`, restricciones 001 y trigger 005. [Reglas por entidad](reglas-negocio-v2.md). |
| 5. ¿Cómo se comprueban FK? | PostgreSQL comprueba las legadas en 003; no hay FK en los seis catálogos. La validación y UI académicas aún no existen por falta del esquema. |
| 6. ¿Dónde están las rutinas? | 004 contiene las 30 funciones CRUD; 003 contiene funciones/procedimientos legados. |
| 7. ¿Dónde se invocan? | Cada método de `MIRA.Api/Repositorios/*Repository.cs` usa Dapper y `SELECT public.sp_*` o `SELECT * FROM public.sp_*`; nunca CRUD directo sobre tablas. |
| 8. ¿Qué maestro y detalle académico funcionan? | Ninguno todavía. `usuario`→roles solo funciona en SQL y no acredita la entrega académica. C02/C09 bloqueados. |
| 9. ¿Cómo se agregan varios renglones? | No hay formulario académico de renglones. `sp_usuario_crear` acepta JSONB solo como evidencia legada de base de datos. No demostrarlo como pantalla inexistente. |
| 10. ¿Cómo se garantiza atomicidad? | Rutina PostgreSQL y transacción; `database/tests/v2_usuarios_roles.sql` comprueba fallos sin estado parcial. Para dominio académico faltan esquema y operación definida. |
| 11. ¿Dónde está el trigger? | 005: `tr_area_aplicacion_normalizar` → `fn_area_aplicacion_normalizar`; enviar nombre con espacios desde `/area_aplicacion/nuevo` o edición y observar el nombre persistido sin espacios. API y Flask no recortan ese valor. |
| 12. ¿Qué sucede ante violación de integridad? | `ApiExceptionMiddleware.InvokeAsync` traduce 23503/23505 a 409 español sin detalles internos; la prueba técnica usa excepciones controladas. Falta una FK académica real para demostrar C07 completo. |
| 13. ¿Por qué 409 y no 500? | Se reconoce un conflicto de datos mediante SQLSTATE; 500 se reserva para errores inesperados. `ErroresPersistenciaTests` comprueba ambos casos y que no se filtre información. |
| 14. ¿Qué hace frontend? | `frontend/app.py` protege POST con CSRF, arma JSON, llama API y muestra resultado. `static/mira.js` agrega búsqueda, navegación activa y estado de envío; no escribe en PostgreSQL. |
| 15. ¿Cómo se relaciona con Spec Kit? | `versiones/v2_profesor/2_spec.md` define requisitos e historias antes de implementar; `6_contracts.md` define llamadas, `8_tasks.md` estado, y `docs/trazabilidad-v2.md` enlaza toda la cadena. |
| 16. ¿Qué prompts se utilizaron? | `docs/prompts/README.md` conserva procedencia; nuevo adjunto en `PROMPT_MAESTRO_DEFINITIVO_MIRA_V2.md`. No se atribuyen prompts inventados a estudiantes. |
| 17. ¿Cómo ejecutar pruebas? | `scripts/verify-local.ps1`, ambos SQL y `scripts/verify-browser.mjs`, según `scripts/README.md`; resultados actuales en `docs/pruebas-v2.md`. |

La matriz de [doce criterios](criterios-v2.md) tiene seis verificados y seis bloqueados. Capturas reales: [escritorio](evidencias/mira-v2-desktop.png) y [móvil](evidencias/mira-v2-mobile.png). Son evidencia visual del recorrido descrito, no un reemplazo de las pruebas.
