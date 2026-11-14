# Prompts del proyecto MIRA

| Documento | Uso |
|---|---|
| [Prompt maestro definitivo MIRA V2](PROMPT_MAESTRO_DEFINITIVO_MIRA_V2.md) | Texto auténtico del nuevo adjunto del usuario. Instrucción vigente: rutinas para todo CRUD, doce criterios y cierre condicionado a evidencias. |
| [Prompt de trabajo](../PROMPT.md) | Contexto breve para tareas concretas sobre el repositorio. Completar el cambio solicitado y sus criterios de aceptación. |
| [Prompt maestro MIRA V2](PROMPT_MAESTRO_MIRA_V2.md) | Auditoría integral, correcciones, verificación y preparación de sustentación, adaptadas al alcance vigente de MIRA. |
| [Prompt original de Backstage V2](referencias/PROMPT_MAESTRO_BACKSTAGE_V2.md) | Copia íntegra del texto adjuntado por el usuario, conservada como referencia histórica. No constituye requisitos ni instrucciones vigentes de MIRA. |

El usuario confirmó aplicar la auditoría a MIRA después de identificar que el adjunto original correspondía a otro producto. La adaptación conserva el método de inspección, implementación, pruebas y trazabilidad y lo aplica a los catálogos y al alcance académico de este repositorio.

Los módulos de Backstage, geoespacial, SOI, CleanCom, multiempresa y autenticación del original no se incorporan al alcance de MIRA por archivar ese documento. Las reglas del proyecto están en [la constitución](../1_constitution.md) y en las especificaciones de [V1](../../versiones/v0.1_api_inicial/2_spec.md) y [V2](../../versiones/v2_profesor/2_spec.md).

Un prompt orienta el trabajo; las especificaciones definen el comportamiento y las pruebas aportan evidencia. Antes de actuar, consultar [la auditoría](../auditoria-v2.md), [la trazabilidad](../trazabilidad-v2.md), [las pruebas ejecutadas](../pruebas-v2.md) y [los pendientes](../pendientes-v2.md). Los resultados históricos no sustituyen una nueva verificación cuando cambian el código o el entorno.

## Procedencia y cambios producidos

- El original de Backstage y el definitivo de MIRA son adjuntos reales del usuario; se conservan íntegros, sin atribuirlos a integrantes concretos.
- `../PROMPT.md` y `PROMPT_MAESTRO_MIRA_V2.md` fueron redactados por el asistente en esta conversación con autorización del usuario. No son prompts iniciales de estudiantes.
- El original de Backstage motivó la auditoría adaptada y las correcciones de validación, duplicados y errores del commit `ffafcd3`.
- El definitivo de MIRA exige la corrección arquitectónica de los repositorios (migración 004), trigger (005), traducción de errores, revisión de interfaz y matriz C01–C12. La exigencia nueva sustituye el criterio anterior que permitía SQL CRUD directo en Repository.
- No se aportaron prompts personales de otros integrantes ni un historial separado de IDE; no se inventan esas evidencias. La implementación inicial quedó local y posteriormente el usuario autorizó nuevos commits/push y mejorar el diseño. Se conserva esa secuencia sin atribuirle autorizaciones al prompt original.
