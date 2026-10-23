# Matriz C01–C12 de MIRA V2

Actualización del 23 de octubre de 2026. Fuente de criterios: prompt definitivo auténtico. Fuente física: los SQL de `880ac75`, identificados y confirmados por el usuario; copias exactas en [fuentes/sql](fuentes/sql/README.md). Las conclusiones anteriores sobre ausencia del modelo quedan corregidas.

| Criterio | Estado | Implementación y evidencia real | Límite |
|---|---|---|---|
| C01 CRUD V1 desde interfaz | Verificado | Seis catálogos; regresión .NET, formularios HTTP y Edge | Alcance local |
| C02 CRUD V2 desde interfaz | Verificado | Once recursos en `frontend/investigacion.py`; formularios HTTP para todos, API y persistencia reales | Docentes requiere personas existentes; la prueba integral usa una persona solo en base aislada |
| C03 Cinco operaciones por rutinas | Verificado | 30 funciones V1 y 55 funciones para once recursos V2; Repository solo invoca funciones parametrizadas | Los cinco puentes tienen cuatro operaciones: no se edita su pareja de claves |
| C04 Trigger demostrable | Verificado | Normalización V1 demostrada desde navegador; trigger oficial `semilleros_grupo_activo_trigger` comprobado con grupo inactivo en SQL/API | El trigger oficial conserva su regla y eventos; se traduce su error a 409 |
| C05 FK en desplegables API | Verificado | Todos los campos referencia de `RecursosInvestigacion.cs` tienen selectores en `investigacion.py`; nombres reales, vacíos y fallos de carga | El padre del detalle se asocia por contexto y no aparece como selector repetido |
| C06 FK opcionales admiten null | Bloqueado | La comparación SQL prueba que **ninguna FK del modelo confirmado es nullable**. Texto y fechas opcionales sí persisten null | Falta que el profesor confirme no aplicabilidad o aporte un modelo con FK opcional; no se inventa una |
| C07 Integridad referencial 409 en español | Verificado | `InvalidReferenceAndInactiveParentTrigger_ReturnConflictWithoutInternals`, HTTP real y formularios de puentes | No se devuelven SQLSTATE, SQL o cadenas de conexión |
| C08 Puentes: asignar, retirar y conflicto | Verificado | Cinco puentes; .NET y cinco casos HTTP: alta, consulta, duplicado 409 y retiro; Edge demuestra grupo–línea | Sin edición de PK compuesta |
| C09 Maestro y renglones en una pantalla | Verificado | `grupo.html`, `investigacion.js`, `GuardarFicha` y `sp_guardar_v2_ficha_grupo`; dos detalles, edición y retiro probados | Es grupo→semilleros, tal como existe en el SQL; no universidad→grupo ni línea→docente |
| C10 Borrado lógico filtra retirados | Verificado | Recursos con activo/activa se retiran y filtran; fichas conservan semilleros retirados en DB | Docentes/tutores carecen de columna de estado: su eliminación es física, con CASCADE donde lo define el SQL |
| C11 Lenguaje comprensible | Verificado | Campos, errores, vacíos y confirmaciones en español; 14 comprobaciones Edge y 72 combinaciones página/ancho | No es auditoría exhaustiva de accesibilidad |
| C12 Regresión V1 completa | Verificado | 47 .NET, 63 Flask/arquitectura y 46 HTTP sobre PostgreSQL 16.15, cuatro SQL; navegador sobre plataforma 17.9 | Docker y producción sin verificar |

**Once criterios verificados y uno bloqueado por discrepancia entre rúbrica y nulabilidad del modelo.** No se declara certificada la aceptación completa de V2. Faltan además `VERSION_2.md` y los nombres oficiales de nueve documentos, aunque el esquema físico ya está confirmado e implementado.
