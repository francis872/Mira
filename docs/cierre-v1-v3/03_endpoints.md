# Contratos añadidos en Fase 5

En `MIRA.Api/Peticiones/InvestigacionController.cs`, autenticación y política Datos siguen aplicadas. Rutas bajo `/api/investigacion`:

| Método/ruta | Contrato | Permiso |
|---|---|---|
| POST asociaciones/{grupo_lineas o semillero_lineas}/asignar | padreId y lineas[1..200], IDs distintos; 201 objeto asociaciones; lote atómico | Administrador/Coordinador |
| GET asociaciones/{recurso}/resumen/{id} | Código/nombre/estado, incluidas líneas históricas inactivas | Lectura autenticada |
| GET semilleros/buscar?grupoId=&docenteId= | Filtros positivos opcionales; no modifica nulabilidad FK | Lectura autenticada |
| GET reportes/opciones | Referencias conservadas para filtros | Lectura autenticada |
| GET reportes/integrantes/{grupo} | Grupo e integrantes reales; 404 si no existe | Lectura autenticada |
| GET reportes/articulacion?sedeId=&lineaId= | Correspondencias reales; filtro desconocido o ID no positivo: 400 | Lectura autenticada |
| GET lineas_investigacion/inactivas | Archivo de líneas retiradas | Administrador |
| PUT lineas_investigacion/{id}/estado | activo booleano explícito, 404 inexistente; conserva relaciones | Administrador |

Frontend protegido: `/investigacion/opciones/sedes`, `/investigacion/lineas_investigacion/inactivas`, POST `/investigacion/lineas_investigacion/reactivar/{id}`, `/investigacion/reportes`, `/investigacion/reportes/articulacion.xlsx` y `/investigacion/reportes/grupos/{id}/integrantes[.xlsx|.pdf]`. POST requiere CSRF; exportaciones son GET sin mutación. Ninguna ruta nueva recupera acceso anónimo.

Las listas originales de abajo describen el corte anterior; estas rutas se añaden a sus contratos. [Pruebas actuales](evidencias/fase5-20261107/casos.json).

---

## Corte previo conservado como antecedente

# Catálogo de endpoints y contratos

Fuente: todos los atributos HTTP de `MIRA.Api/Peticiones/*Controller.cs` y `Program.cs`. [Inventario mecánico de 50 patrones](evidencias/endpoints.json). Las rutas genéricas se expanden solo a los recursos declarados; no son SQL o nombres de tabla arbitrarios.

GET/HEAD de datos: cualquiera de los tres roles conocidos. POST/PUT: Coordinador o Administrador. DELETE: Administrador. Login y health son públicos; Me/Logout necesitan sesión válida; cuentas y catálogo de roles solo Administrador. La política se aplica en API; esconder botones no autoriza.

| Contrato | Métodos y comportamiento |
|---|---|
| `/api/{catalogo}` | GET 200 lista activa; POST 201 alta |
| `/api/{catalogo}/{id}` | GET 200/404; PUT 200/400/404; DELETE 200/404, retiro lógico |
| `/api/investigacion/{recurso}` | GET 200; POST 201, once entidades o cinco puentes |
| `/api/investigacion/{recurso}/{id}` | GET/PUT/DELETE para once entidades; 200 o 404 |
| `/api/investigacion/{puente}/{izquierda}/{derecha}` | GET 200/404; DELETE 200/404; no PUT de PK compuesta |
| `/api/investigacion/referencias/usuarios` | GET 200, personas existentes sin password_hash |
| `/api/investigacion/grupos/ficha` | POST 201, cuerpo grupo + colección semilleros |
| `/api/investigacion/grupos/ficha/{id}` | GET/PUT 200/404, edición atómica |
| `/api/auth/login` | POST correo/password; 200 token/expiraEn/usuario; 401 credencial incorrecta/inactiva; 429 cuota; 400 body inválido |
| `/api/auth/me` | GET 200 identidad actual o 401 |
| `/api/auth/logout` | POST 200, revoca jti; token posterior 401 |
| `/api/auth/usuarios` | GET 200; POST 201 con correo/password/roles |
| `/api/auth/usuarios/{id}` | PUT 200 correo/roles; DELETE 200/404, inactiva cuenta |
| `/api/auth/usuarios/{id}/password` | PUT 200 restablece contraseña y revoca sesiones |
| `/api/auth/roles` | GET 200 roles activos existentes; no endpoint para crear roles |
| `/health` | GET 200 liveness; no consulta PostgreSQL ni prueba readiness |

Los catálogos V1 son `area_conocimiento`, `objetivo_desarrollo_sostenible`, `area_aplicacion`, `termino_clave`, `universidad`, `linea_investigacion`. Término clave usa `{termino}` textual inmutable, no ID entero. Campos y límites: clases en `MIRA.Api/Modelos`; inglés y descripción V1 son opcionales. Categoría ODS requiere texto no vacío, sin lista cerrada en la implementación.

Once recursos V2: sedes, docentes, grupos_investigacion, semilleros, lineas_investigacion, grupo_docentes, semillero_tutores, areas_conocimiento, ods, areas_aplicacion, palabras_clave. Campos, tipos, opciones y obligatoriedad exactos: `RecursosInvestigacion.cs`. Un campo desconocido se rechaza; no se aceptan activo, IDs o timestamps como campos libres.

Puentes: grupo_lineas(grupo_id,linea_id), semillero_lineas(semillero_id,linea_id), linea_disciplinas(linea_id,disciplina_id), linea_ods(linea_id,ods_id), linea_aplicacion(linea_id,aplicacion_id).

Ejemplo ficticio de ficha (sede_id debe ser una referencia activa real de la base de pruebas):

```json
{"grupo":{"sede_id":1,"gruptac_url":"https://example.invalid/grupo","categoria":null},"semilleros":[{"nombre":"Semillero de demostración A","descripcion":null},{"nombre":"Semillero de demostración B","descripcion":"Dato ficticio"}]}
```

En PUT, los detalles existentes incluyen su id; los nuevos lo omiten; omitir uno existente lo inactiva. No se puede trasladar a la ficha un ID de otro grupo. Una excepción revierte toda la operación. No se acepta grupo_id independiente en un detalle.

401 significa falta/invalidación de identidad; 403 identidad válida sin permiso; 409 integridad o duplicado; 400 forma/campo inválido; 404 ausencia/inactivo/recurso desconocido. `ApiExceptionMiddleware` no expone SQLSTATE, SQL, stack o conexión. Flask muestra 503 cuando la API no está disponible; los contratos de error no garantizan disponibilidad remota.

Las páginas Flask usan POST+CSRF para mutaciones (la API usa PUT/DELETE). Rutas: `/login`, `/logout`, dashboard `/`, seis módulos V1, `/investigacion`, `/investigacion/{recurso}`, formularios nuevo/editar y `/investigacion/asociaciones/{puente}`; cuentas bajo el blueprint de seguridad. Swagger solo Development. Los nombres históricos de algunos tests contienen “sin autenticación”, pero su fixture ahora inicia sesión; se verificó su código, no solo el nombre.

No existen endpoints de reportes, exportación, Facultad, autoservicio docente o proyectos. La conclusión procede del catálogo completo y de los recursos, rutas Flask y plantillas inspeccionados; no de buscar la palabra MIRA.

## Archivo y método por patrón HTTP

| Verbo | Ruta con restricción de ASP.NET | Archivo :: método |
|---|---|---|
| GET | `/api/area_aplicacion` | `MIRA.Api/Peticiones/AreaAplicacionController.cs::GetAll` |
| GET | `/api/area_aplicacion/{id:int}` | `MIRA.Api/Peticiones/AreaAplicacionController.cs::GetById` |
| POST | `/api/area_aplicacion` | `MIRA.Api/Peticiones/AreaAplicacionController.cs::Create` |
| PUT | `/api/area_aplicacion/{id:int}` | `MIRA.Api/Peticiones/AreaAplicacionController.cs::Update` |
| DELETE | `/api/area_aplicacion/{id:int}` | `MIRA.Api/Peticiones/AreaAplicacionController.cs::Delete` |
| GET | `/api/area_conocimiento` | `MIRA.Api/Peticiones/AreaConocimientoController.cs::GetAll` |
| GET | `/api/area_conocimiento/{id:int}` | `MIRA.Api/Peticiones/AreaConocimientoController.cs::GetById` |
| POST | `/api/area_conocimiento` | `MIRA.Api/Peticiones/AreaConocimientoController.cs::Create` |
| PUT | `/api/area_conocimiento/{id:int}` | `MIRA.Api/Peticiones/AreaConocimientoController.cs::Update` |
| DELETE | `/api/area_conocimiento/{id:int}` | `MIRA.Api/Peticiones/AreaConocimientoController.cs::Delete` |
| POST | `/api/auth/login` | `MIRA.Api/Peticiones/AuthController.cs::Login` |
| GET | `/api/auth/me` | `MIRA.Api/Peticiones/AuthController.cs::Me` |
| POST | `/api/auth/logout` | `MIRA.Api/Peticiones/AuthController.cs::Logout` |
| GET | `/api/auth/usuarios` | `MIRA.Api/Peticiones/AuthController.cs::Usuarios` |
| GET | `/api/auth/roles` | `MIRA.Api/Peticiones/AuthController.cs::Roles` |
| POST | `/api/auth/usuarios` | `MIRA.Api/Peticiones/AuthController.cs::Crear` |
| PUT | `/api/auth/usuarios/{id:long:min(1)}` | `MIRA.Api/Peticiones/AuthController.cs::Editar` |
| DELETE | `/api/auth/usuarios/{id:long:min(1)}` | `MIRA.Api/Peticiones/AuthController.cs::Inactivar` |
| PUT | `/api/auth/usuarios/{id:long:min(1)}/password` | `MIRA.Api/Peticiones/AuthController.cs::Restablecer` |
| GET | `/api/investigacion/referencias/usuarios` | `MIRA.Api/Peticiones/InvestigacionController.cs::Usuarios` |
| GET | `/api/investigacion/grupos/ficha/{id:int:min(1)}` | `MIRA.Api/Peticiones/InvestigacionController.cs::Ficha` |
| POST | `/api/investigacion/grupos/ficha` | `MIRA.Api/Peticiones/InvestigacionController.cs::CrearFicha` |
| PUT | `/api/investigacion/grupos/ficha/{id:int:min(1)}` | `MIRA.Api/Peticiones/InvestigacionController.cs::ActualizarFicha` |
| GET | `/api/investigacion/{recurso}` | `MIRA.Api/Peticiones/InvestigacionController.cs::Listar` |
| GET | `/api/investigacion/{recurso}/{id:int:min(1)}` | `MIRA.Api/Peticiones/InvestigacionController.cs::Obtener` |
| GET | `/api/investigacion/{recurso}/{izquierda:int:min(1)}/{derecha:int:min(1)}` | `MIRA.Api/Peticiones/InvestigacionController.cs::ObtenerPuente` |
| POST | `/api/investigacion/{recurso}` | `MIRA.Api/Peticiones/InvestigacionController.cs::Crear` |
| PUT | `/api/investigacion/{recurso}/{id:int:min(1)}` | `MIRA.Api/Peticiones/InvestigacionController.cs::Actualizar` |
| DELETE | `/api/investigacion/{recurso}/{id:int:min(1)}` | `MIRA.Api/Peticiones/InvestigacionController.cs::Eliminar` |
| DELETE | `/api/investigacion/{recurso}/{izquierda:int:min(1)}/{derecha:int:min(1)}` | `MIRA.Api/Peticiones/InvestigacionController.cs::RetirarPuente` |
| GET | `/api/linea_investigacion` | `MIRA.Api/Peticiones/LineaInvestigacionController.cs::GetAll` |
| GET | `/api/linea_investigacion/{id:int}` | `MIRA.Api/Peticiones/LineaInvestigacionController.cs::GetById` |
| POST | `/api/linea_investigacion` | `MIRA.Api/Peticiones/LineaInvestigacionController.cs::Create` |
| PUT | `/api/linea_investigacion/{id:int}` | `MIRA.Api/Peticiones/LineaInvestigacionController.cs::Update` |
| DELETE | `/api/linea_investigacion/{id:int}` | `MIRA.Api/Peticiones/LineaInvestigacionController.cs::Delete` |
| GET | `/api/objetivo_desarrollo_sostenible` | `MIRA.Api/Peticiones/ObjetivoDesarrolloSostenibleController.cs::GetAll` |
| GET | `/api/objetivo_desarrollo_sostenible/{id:int}` | `MIRA.Api/Peticiones/ObjetivoDesarrolloSostenibleController.cs::GetById` |
| POST | `/api/objetivo_desarrollo_sostenible` | `MIRA.Api/Peticiones/ObjetivoDesarrolloSostenibleController.cs::Create` |
| PUT | `/api/objetivo_desarrollo_sostenible/{id:int}` | `MIRA.Api/Peticiones/ObjetivoDesarrolloSostenibleController.cs::Update` |
| DELETE | `/api/objetivo_desarrollo_sostenible/{id:int}` | `MIRA.Api/Peticiones/ObjetivoDesarrolloSostenibleController.cs::Delete` |
| GET | `/api/termino_clave` | `MIRA.Api/Peticiones/TerminoClaveController.cs::GetAll` |
| GET | `/api/termino_clave/{termino}` | `MIRA.Api/Peticiones/TerminoClaveController.cs::GetByTermino` |
| POST | `/api/termino_clave` | `MIRA.Api/Peticiones/TerminoClaveController.cs::Create` |
| PUT | `/api/termino_clave/{termino}` | `MIRA.Api/Peticiones/TerminoClaveController.cs::Update` |
| DELETE | `/api/termino_clave/{termino}` | `MIRA.Api/Peticiones/TerminoClaveController.cs::Delete` |
| GET | `/api/universidad` | `MIRA.Api/Peticiones/UniversidadController.cs::GetAll` |
| GET | `/api/universidad/{id:int}` | `MIRA.Api/Peticiones/UniversidadController.cs::GetById` |
| POST | `/api/universidad` | `MIRA.Api/Peticiones/UniversidadController.cs::Create` |
| PUT | `/api/universidad/{id:int}` | `MIRA.Api/Peticiones/UniversidadController.cs::Update` |
| DELETE | `/api/universidad/{id:int}` | `MIRA.Api/Peticiones/UniversidadController.cs::Delete` |
