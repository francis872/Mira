# Contratos MIRA V2

Los seis contratos V1 `/api/{area_conocimiento,objetivo_desarrollo_sostenible,area_aplicacion,termino_clave,universidad,linea_investigacion}` permanecen. El nuevo prefijo es `/api/investigacion/`; sus documentos JSON conservan los nombres de columnas en snake_case.

| Recursos | Operaciones |
|---|---|
| sedes, docentes, grupos_investigacion, semilleros, lineas_investigacion, grupo_docentes, semillero_tutores, areas_conocimiento, ods, areas_aplicacion, palabras_clave | GET colección; GET `{id}`; POST colección; PUT `{id}`; DELETE `{id}` |
| grupo_lineas, semillero_lineas, linea_disciplinas, linea_ods, linea_aplicacion | GET colección; GET `{izquierda}/{derecha}`; POST pareja JSON; DELETE `{izquierda}/{derecha}`. No PUT de claves |
| referencias/usuarios | Solo GET: colección de `{id,nombre}`; sin correo/hash ni escrituras |
| grupos/ficha | POST de grupo y semilleros; 201 con Location |
| grupos/ficha/{id} | GET documento compuesto y PUT de colección completa; 404 si ausente/inactivo |

POST/PUT aceptan únicamente los campos editables de `Modelos/RecursosInvestigacion.cs`. Los generados por DB, las claves y los estados no se asignan masivamente. Los límites son los del SQL. Crear devuelve valores realmente almacenados, incluidos defaults; actualizar devuelve la fila persistida. DELETE devuelve 200 y mensaje; recurso ausente/inactivo, 404.

Ejemplo de ficha:

```json
{"grupo":{"sede_id":1,"gruptac_url":"https://example.org/grupo","categoria":null},"semilleros":[{"nombre":"Semillero","descripcion":null}]}
```

El identificador de sede debe venir del selector API; el ejemplo muestra la forma, no datos semilla. En edición los detalles existentes incluyen `id`. No se envía `grupo_id` dentro de los renglones; la función lo deriva del maestro. Omitir un semillero en la colección lo retira lógicamente. Campo `semilleros` es obligatorio aunque sea `[]`. Máximo 200 renglones por solicitud. Un id ajeno/inactivo devuelve 409 y revierte toda la ficha.

Las funciones de las once entidades son `sp_{listar,consultar,crear,actualizar,eliminar}_v2_{recurso}`. Listar devuelve JSON array; consultar/crear/actualizar, JSON object; eliminar, boolean. Puentes tienen las cuatro funciones correspondientes, con dos argumentos para consultar/eliminar. `sp_consultar_v2_ficha_grupo`, `sp_guardar_v2_ficha_grupo` y `sp_referencias_v2_usuarios` completan las 78 funciones de 007. Son FUNCTION invocadas con SELECT, no PROCEDURE/CALL.

Errores: 400 campos/formato/longitud/fechas/CHECK; 404 recurso inexistente; 409 FK/duplicados; 500 inesperado seguro. El cuerpo de error contiene `mensaje` en español. Los null se admiten solo según columnas originales. No hay FK opcional en este modelo.
