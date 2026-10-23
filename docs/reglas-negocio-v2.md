# Reglas implementadas y puntos de explicación

Estas reglas se verifican en el código y esquema existentes, no se atribuyen a un documento académico ausente. Referencias: especificaciones V1/V2, migraciones 001, 004 y 005.

| Entidad | Entrada/Service | PostgreSQL y rutinas | Actualización/retiro y errores |
|---|---|---|---|
| Área de conocimiento | `AreaConocimientoService.CreateAsync/UpdateAsync`: gran área, área y disciplina obligatorias; Trim; 150 caracteres por campo en modelo | `area_conocimiento`: PK serial, NOT NULL, activo; `sp_*_area_conocimiento` | ID válido/existente; inactivación, sin borrar fila; 400 validación, 404 inexistente |
| ODS | `ObjetivoDesarrolloSostenibleService`: nombre/categoría obligatorios, Trim; 200/100 caracteres | `objetivo_desarrollo_sostenible`, `sp_*_objetivo_desarrollo_sostenible` | Mismo contrato de ID y retiro |
| Área de aplicación | `AreaAplicacionService`: nombre obligatorio; longitud 200 en modelo. El servicio valida y deja la normalización final al trigger | `sp_*_area_aplicacion`; `tr_area_aplicacion_normalizar` aplica recorte de espacios extremos INSERT/UPDATE; blank produce 22023 | Actualizar conserva ID; baja lógica no modifica nombre. 400 para nombre inválido; retorno de alta refleja trigger |
| Término clave | `TerminoClaveService`: término obligatorio/Trim, inglés opcional/Trim; 150 por campo; consulta previa de clave activa | PK textual en `termino_clave`; `sp_*_termino_clave`. UNIQUE también protege inactivos y carreras | Clave inmutable desde la aplicación; PUT modifica inglés. Duplicados 409, inactivo no se reactiva, inexistente 404 |
| Universidad | `UniversidadService`: nombre, tipo, ciudad obligatorios y Trim; límites 200/100/100 | `universidad`, `sp_*_universidad`; PK y NOT NULL | ID de ruta, existencia y borrado lógico; no se inventa unicidad de nombre ni listado cerrado de tipos como regla DB |
| Línea de investigación | `LineaInvestigacionService`: nombre obligatorio/Trim (200), descripción opcional | `linea_investigacion`, `sp_*_linea_investigacion`; descripción TEXT | Se conserva comportamiento existente de descripción y retiro; sin FK definida |

`*` en esta tabla representa las cinco funciones `sp_listar`, `sp_consultar`, `sp_crear`, `sp_actualizar`, `sp_eliminar`. El archivo exacto es `database/migrations/004_v2_catalogos_rutinas.sql`. Solo el trigger de área normaliza en PostgreSQL; no se atribuye esa transformación a los otros catálogos.

Los atributos `StringLength` están en `MIRA.Api/Modelos/`. La validación HTTP automática devuelve 400 con `errors`; las reglas semánticas de servicios producen `mensaje` a través de los controladores. El middleware `MIRA.Api/Excepciones/ApiExceptionMiddleware.cs` cubre restricciones no manejadas y fallos inesperados: 23503/23505 → 409, 23502/22023/22001 → 400, resto → 500 seguro. No se devuelve texto bruto de PostgreSQL.

La normalización de área cambió de ubicación, no de objetivo: el frontend y Service transmiten el texto original, PostgreSQL lo recorta y `RETURNING *` devuelve la fila final. Se prueba nombre vacío, espacios, creación y actualización; el trigger no añade campos ni cambia datos antiguos automáticamente.

Los seis catálogos no contienen FK. No hay regla implementada de retiro de universidad con grupos ni de línea con docentes porque faltan sus tablas oficiales. Las rutinas legadas usuario→roles validan referencias en PostgreSQL pero no son parte del CRUD académico expuesto.
