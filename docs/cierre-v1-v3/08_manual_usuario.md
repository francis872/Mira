# Recorridos corregidos en Fase 5

1. Palabras clave: completar español e inglés; ODS V1: seleccionar Social, Económica o Ambiental.
2. Sedes: registrar nombre, ciudad y código institucional. Los formularios abiertos actualizan opciones por aviso entre pestañas, foco/visibilidad y sondeo visible cada 15 segundos. Si falla la red se conserva la entrada y se informa; la API revalida la sede.
3. Líneas académicas: código único, nombre, descripción y al menos una disciplina y un ODS; el guardado es conjunto. Las líneas inactivas conservan relaciones, no admiten articulaciones nuevas y pueden reactivarse desde el archivo por Administrador.
4. Asociaciones: seleccionar varias líneas y asignarlas en una sola operación. Retirar pide confirmación, admite Escape y devuelve el foco al botón. Los resúmenes de grupo/semillero muestran nombres y estados históricos.
5. Semilleros: filtrar por grupo base o docente tutor. Investigación→Reportes muestra correspondencias y filtros Sede/Línea, permite descargar el mismo conjunto en Excel y abrir integrantes de grupo en HTML/Excel/PDF. El acceso requiere sesión; Investigador puede consultar/exportar, no modificar.

Facultad, perfil propio cuenta–docente y periodo académico siguen pendientes de definición; no se anuncian como disponibles. Las bajas y reactivaciones conservan la restricción Administrador de V3.

---

## Corte previo conservado como antecedente

# Manual de usuario del alcance existente

Ingresar con una cuenta activa. Una contraseña incorrecta devuelve un mensaje uniforme. No hay registro abierto ni recuperación por correo: un administrador gestiona cuentas; la recuperación inicial es operativa. Al vencer o invalidarse la sesión, se solicita nuevo login.

Investigador consulta; Coordinador crea/edita datos; Administrador además retira y administra cuentas. Los permisos se aplican también a rutas directas. Una denegación de permiso no debe interpretarse como pérdida de datos. No se ofrece edición del propio perfil docente por identidad de acceso.

En Inicio aparecen conteos reales de seis catálogos y acceso a Investigación. Si la API no responde, se informa indisponibilidad; no se inventan ceros. Cada catálogo permite listar, buscar texto, crear, editar y retirar según rol. Los retiros de catálogos son lógicos. Término clave tiene clave textual que no se edita; su inglés es opcional actualmente. ODS acepta categoría libre: es una diferencia abierta frente al documento original, no una validación ya resuelta.

Investigación ofrece once recursos. Las referencias se seleccionan de datos existentes de la API; si no hay opciones o falla la carga, se informa y no se inventan IDs. Docentes requiere una persona académica previamente cargada; crear una cuenta V3 no crea esa persona. Enlaces CvLAC/GrupLAC deben ser http/https válidos. Las fechas finales opcionales deben superar las iniciales.

La ficha Grupo permite guardar maestro y hasta 200 semilleros en una pantalla. Añadir crea un renglón; quitar un detalle existente lo inactiva al guardar. Conservar los IDs existentes evita crear duplicados. Todos los cambios se confirman juntos; un detalle ajeno a otro grupo impide guardar toda la ficha. Si falla, revisar referencias, conservar los valores y corregir antes de reintentar.

En Asociaciones se selecciona una pareja por envío: grupo–línea, semillero–línea, línea–disciplina, línea–ODS o línea–aplicación. Una relación duplicada se rechaza; retirar elimina el puente. No hay edición de su clave ni selección múltiple en un único envío. Actualmente el retiro de puentes se envía directamente, sin confirmación previa; se documenta como diferencia histórica pendiente.

Las pantallas con estado filtran retirados. Docentes y tutores se eliminan físicamente porque su modelo carece de estado; eliminar un docente puede eliminar participaciones por CASCADE. Leer la advertencia del modal. No se promete historial inmutable ni restauración de registros eliminados.

Al salir, una respuesta correcta confirma revocación y elimina la sesión local. Si aparece advertencia de API no disponible, se cerró la sesión del navegador pero la revocación remota no está confirmada. El token puede continuar válido hasta vencer o revocarse después. No publicar pantallas de tokens, contraseñas o configuración.

El menú se adapta al ancho, las tablas permiten desplazamiento horizontal y los controles tienen foco visible. Se probaron teclado y zoom en Edge; dispositivos físicos y lectores de pantalla siguen pendientes. No existen módulos de proyectos, reportes consolidados, Facultad ni exportación Excel/PDF/CSV. [Alcance y pendientes](12_riesgos_excepciones.md).
