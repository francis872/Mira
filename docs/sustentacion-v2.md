# Guía de sustentación MIRA V2

Demostrar en `http://localhost:5000/investigacion`, sin login. Fuente: [SQL confirmado](fuentes/sql/README.md). Evidencia: [pruebas](pruebas-v2.md) y [matriz](criterios-v2.md).

| Pregunta | Código y demostración |
|---|---|
| ¿Dónde está GET? | `InvestigacionController.Listar/Obtener` → `InvestigacionService.Listar/Obtener` → `InvestigacionRepository.Listar/Obtener` → `sp_listar_v2_*`/`sp_consultar_v2_*` |
| ¿Dónde está POST? | `InvestigacionController.Crear` → Service.Validar → Repository.Crear → `sp_crear_v2_*`; devuelve fila almacenada y Location |
| ¿Dónde se retira? | Controller.Eliminar → Service.Eliminar → Repository.Eliminar → `sp_eliminar_v2_*`; activo/activa define lógico, ausencia de estado define físico |
| ¿Dónde están las reglas? | `Modelos/RecursosInvestigacion.cs`, `InvestigacionService.Validar`, restricciones de 006 y trigger oficial en 007 |
| ¿Cómo se comprueba FK? | Restricciones PostgreSQL reales; los selectores de `investigacion.py:options_for` consumen API; la integridad vuelve a validarse en DB |
| ¿Dónde están las rutinas? | 004 para V1 y 007 para dominio confirmado; FUNCTIONS invocadas con SELECT, no CALL |
| ¿Cómo las llama Repository? | `Repositorios/Investigacion/InvestigacionRepository.cs`: parámetros Dapper, nombres solo de metadatos registrados |
| ¿Qué maestro-detalle se usa? | Grupo de investigación→semilleros. Abrir una ficha y agregar dos renglones |
| ¿Cómo se editan/retiran? | `grupo.html` e `investigacion.js`; todos los renglones viajan juntos, sin selector de padre por fila |
| ¿Cómo se guarda? | Flask.group_form → Controller.CrearFicha/ActualizarFicha → Service.GuardarFicha → Repository.GuardarFicha → `sp_guardar_v2_ficha_grupo` |
| ¿Cómo se demuestra atomicidad? | `SheetCreatesManyRowsThenRollsBackAllChangesWhenLastRowBelongsElsewhere` y `database/tests/v2_investigacion.sql`: fallo tardío conserva maestro y detalles anteriores |
| ¿Cuál es el trigger? | `semilleros_grupo_activo_trigger`; rechaza alta/cambio de padre a grupo inactivo. La normalización V1 también se demuestra en Edge |
| ¿Por qué 409? | `ApiExceptionMiddleware`: FK 23503 y UNIQUE 23505; prueba HTTP usa referencias inexistentes y duplicados reales |
| ¿Qué hace Flask? | Renderiza formularios, valida CSRF, carga referencias, envía HTTP JSON y muestra errores españoles; no conecta a PostgreSQL |
| ¿Qué documentación guía? | `versiones/v2_profesor/`, especificación de integración previa, contratos y trazabilidad |
| ¿Qué prompts se usaron? | `docs/prompts/README.md`; originales preservados. La confirmación del SQL por el usuario queda registrada en `fuentes/sql/README.md` |
| ¿Cómo probar? | `scripts/verify-postgres16.ps1`; API/base/frontend aislados, cuatro SQL, .NET, Flask y HTTP. `verify-browser.mjs` prueba Edge |

V1 sigue en `app.py`, controladores, servicios y repositorios por catálogo. Su cadena y tests permanecen. No presentar usuario→roles legado como maestro-detalle académico. No existe FK nullable: C06 necesita aclaración; tampoco se han recibido los nueve nombres oficiales del curso. El selector docente vacío en la plataforma habitual significa que faltan personas reales, no una autenticación pendiente.
