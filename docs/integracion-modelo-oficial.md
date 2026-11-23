# Integración del SQL oficial confirmado

Especificación previa a la implementación, 23 de octubre de 2026. El usuario identificó y confirmó los dos archivos de [fuentes/sql](fuentes/sql/README.md). Esta confirmación resuelve la incertidumbre sobre el modelo físico; no aporta los nombres de los nueve documentos ni `VERSION_2.md`.

## Modelo y compatibilidad

Se añaden las 19 tablas del SQL, con sus columnas, tipos, PK, FK, índices y restricciones. Las seis tablas singulares de V1 y las tres de la migración 003 permanecen intactas. Los catálogos plurales del modelo confirmado se administran por separado: no se reasignan sus identificadores a los catálogos singulares ni se simula equivalencia entre sede y universidad. Los únicos datos iniciales son los que ya incluye el SQL fuente; no se crean docentes, grupos, usuarios ni credenciales de ejemplo en la plataforma.

Las once entidades editables son `sedes`, `docentes`, `grupos_investigacion`, `semilleros`, `lineas_investigacion`, `grupo_docentes`, `semillero_tutores`, `areas_conocimiento`, `ods`, `areas_aplicacion` y `palabras_clave`. Las cinco asociaciones sin atributos editables son `grupo_lineas`, `semillero_lineas`, `linea_disciplinas`, `linea_ods` y `linea_aplicacion`.

No existe ninguna FK nullable en este SQL. Los campos opcionales son texto o fechas; se envían como null cuando están vacíos. No se inventa una FK opcional para aparentar el criterio C06.

## Contratos y responsabilidades

La API usa el prefijo `/api/investigacion/` y conserva los nombres de campos del SQL en sus documentos JSON. Controller resuelve HTTP; Service valida campos, longitudes, formatos, fechas y recursos permitidos; Repository solo ejecuta funciones parametrizadas. Las entidades se seleccionan de una lista cerrada en código; ningún nombre de tabla o función proviene directamente del solicitante. Las funciones PostgreSQL realizan las cinco operaciones y las restricciones conservan la integridad incluso ante concurrencia.

Cada recurso con id tiene listado, consulta, creación, actualización y eliminación. Los recursos con `activo` o `activa` se retiran lógicamente y se excluyen de lecturas. `docentes` y `semillero_tutores` no tienen columna de estado: su eliminación es física según el SQL y respeta las acciones FK existentes. Las asociaciones puras se asignan y retiran, sin editar sus parejas de claves. Los duplicados y FK inválidas devuelven 409; campos inválidos, 400. No se añade una prohibición de retirar padres con detalles que el modelo no define.

`usuarios`, `roles` y `usuario_roles` no tienen CRUD de aplicación. Un selector de solo lectura entrega id y nombre de usuarios existentes para la FK obligatoria de docentes. Si no hay personas cargadas, el formulario explica el estado vacío: no se crean contraseñas ni identidades ficticias. La incorporación de personas es una precondición de datos para registrar docentes, no un login.

## Maestro y detalles

Una ficha de grupo muestra su sede, GrupLAC, categoría y los semilleros asociados. Permite agregar, editar y retirar varios semilleros sin volver a seleccionar el grupo. Una única llamada envía maestro y renglones a una función JSONB PostgreSQL; cualquier error revierte todas las escrituras. Los renglones omitidos se retiran lógicamente; solo se pueden editar semilleros del grupo actual. Se bloquea el maestro al guardar para serializar fichas concurrentes. El trigger original exige grupo activo al insertar o cambiar el padre de un semillero y se conserva.

## Verificación prevista

Regresión V1, inventario de columnas/FK frente a la fuente, CRUD por entidad, listas FK reales, null en campos opcionales, duplicados de puentes, FK inexistente con 409, trigger del grupo activo y rollback de maestro-detalle con un renglón inválido. Se ejecutan SQL, API, Flask, HTTP y navegador cuando corresponde, con PostgreSQL 16 aislado y posteriormente en la plataforma local.
