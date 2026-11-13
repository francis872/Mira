# Recorrido actual de diez minutos — Fase 5

| Minutos | Demostración y respaldo |
|---|---|
| 0–1 | MIRA es el origen académico; raíz/rama/HEAD y arquitectura HTTP→servicio→repositorio. Separar cuentas V3 de personas académicas. |
| 1–2 | V1: español/inglés obligatorio y selector ODS de tres categorías. Intentar envío inválido y mostrar rechazo API/SQL, no solo required HTML. |
| 2–3 | V2: sede/código, línea con disciplinas/ODS mínimos y 17 opciones. Mostrar rollback completo ante referencia inválida. |
| 3–4 | Asociación múltiple, confirmación accesible y resumen histórico. Retirar/reactivar línea como Administrador sin borrar puentes. |
| 4–5 | Reporte Sede/Línea, integrantes y descarga XLSX/PDF; comparar filas reales. Declarar Facultad pendiente. |
| 5–7 | Seis escenarios V3: login válido 200, contraseña incorrecta 401, sin token 401, expirado 401, revocado 401, autorización denegada 403. Usar los tests JwtBearer del recorrido anterior, sin mostrar tokens. |
| 7–8 | bcrypt12/JWT/jti/roles/CSRF y D01 preservados; enseñar hash de workflow y prueba de orígenes separados. |
| 8–9 | 136 .NET, 120 Flask, 51 HTTP, 10 SQL y repetición Docker; nueva evidencia de accesibilidad y recuperación de copia V3. |
| 9–10 | Dictámenes separados y pendientes: C06, periodos, perfil propio, Facultad, aceptación de roles y verificación cloud/manual. No presentar V2 como cerrada. |

Preguntas técnicas nuevas:

- ¿Por qué NOT VALID? Conserva filas heredadas sin inventar códigos/traducciones; restringe nuevas escrituras. Falta completar datos reales y VALIDATE, por eso no se presenta una migración de producción validada.
- ¿Cómo se impide una línea vacía? Servicio valida arrays, función guarda línea y puentes atómicamente y triggers diferibles verifican mínimos al commit. Bloqueos de padre serializan retiradas; V2MinimosLineaTests prueba la carrera.
- ¿Qué impide fórmulas en Excel? reportes.excel fuerza cada celda a tipo texto; prueba con nombre =1+1 y lectura OpenpyXL. PDF escapa contenido y usa fuente Unicode.
- ¿Por qué no hay Facultad? No existe entidad/relación en SQL confirmado; faltan fuente y decisión. Rechazar filtro desconocido evita entregar resultados incorrectos con apariencia de filtro aplicado.
- ¿Cambian permisos con las rutas nuevas? No: lectura/exportación autenticada, escritura Datos y reactivación Administrador; V2EstadoLineaTests verifica los tres roles con JWT.
- ¿Los sp_* son procedimientos? Son funciones RETURNS jsonb llamadas con SELECT; se distinguen de procedimientos CALL.
- ¿C06 ya cumple? No hay FK opcional: 22 obligatorias. Es una condición posiblemente no aplicable, pendiente de aceptación escrita.

El recorrido anterior se conserva abajo como antecedente; sus cifras y pendientes anteriores se sustituyen por el corte Fase 5.

---

## Corte previo conservado como antecedente

# Sustentación integral de diez minutos

Preparar la instalación aislada del manual 07, dos ventanas (navegador y código), reportes de este directorio y cuentas ficticias Administrador/Investigador/Coordinador. No usar datos institucionales ni mostrar variables, contraseñas, JWT completos o hashes. Si falla la red, usar los reportes fechados y decir que son evidencia previa de este corte, sin fingir una ejecución en vivo.

| Tiempo | Recorrido y fundamento |
|---|---|
| 0:00–1:00 | Identidad SAPIENS, base MIRA V1–V3 y límites; diagrama Controller→Service→Repository; un sistema y dos dominios usuario/usuarios |
| 1:00–2:30 | V1: crear área de aplicación con espacios, consultar desde cliente nuevo, editar/inactivar; señalar normalizador y validaciones; demostrar datos persistidos |
| 2:30–4:30 | V2: asociación grupo–línea y duplicado409; ficha con dos semilleros; retirar uno; mostrar test rollback de detalle ajeno y función RETURNS jsonb; trigger de grupo activo |
| 4:30–7:00 | V3: login válido/incorrecto, sin token, token expirado, JWT revocado y operación403; localizar bcrypt/JWT/roles; pruebas rápidas deterministas |
| 7:00–8:00 | D01: evidencia de cuota429 por origen y otro cliente302; falsificación rechazada; explicar logout con API caída y JWT aún válido |
| 8:00–9:00 | Móvil/teclado/zoom; mostrar evidencia 50/18/10/27 y advertir que no certifica lector ni dispositivos físicos |
| 9:00–10:00 | Matriz de 104 registros, C06, diferencias históricas, CI bloqueado y tres dictámenes; no afirmar producción ni comenzar V4 |

## Escenarios reproducibles

Todos los IDs se obtienen de la base ficticia; no copiar IDs de producción. “Caso” nombra pruebas incluidas en evidencias/casos.json. Para seguridad usar el filtro SeguridadTests del manual 10: mantiene JwtBearer real con repositorio de pruebas; la suite SeguridadDatabaseTests complementa PostgreSQL real.

| Caso | Preparación y datos ficticios | Acción | Esperado | Evidencia y recuperación |
|---|---|---|---|---|
| V1 CRUD/persistencia | Admin de prueba; nombre `demo-area-fecha` | Crear área, consultar desde cliente nuevo, editar y retirar | 201/200; fila persiste inactiva y desaparece de activos | CatalogCrudRegressionTests.FullLifecycle_CreateReadListUpdateLogicalDelete; E2E test_catalog_create_update_inactivate_persist. Si falla referencia/servicio, revisar base/puertos y repetir con otro marcador |
| V1 validación/trigger | Nombre con espacios y mayúsculas; otro body vacío | POST área y POST inválido | Nombre normalizado guardado; inválido400 sin alta | AreaName_IsNormalizedByDatabaseTrigger_AndReturnedPersisted; EmptyPayload_ReturnsBadRequest. No corregir manualmente la fila para simular el trigger |
| V2 asociaciones | Grupo/línea activos de prueba | Asignar pareja, repetir, consultar y retirar | 201,409,200; retirada deja404 | AllFiveBridges_AssignQueryDuplicateConflictAndRemove. Conservar IDs y retirar solo puentes propios |
| V2 maestro–detalle | Sede activa de prueba; dos nombres de semillero | POST ficha; PUT modifica/añade/omite detalle | Una transacción, detalles creados/editados y omitido inactivo | test_group_sheet_http_creates_edits_and_retires_multiple_details. Si falla, comprobar referencias y conservar datos del formulario |
| V2 rollback | Dos grupos propios, detalle del segundo | Actualizar primer grupo con detalle ajeno al final | 409 y estado completo anterior conservado | SheetCreatesManyRowsThenRollsBackAllChangesWhenLastRowBelongsElsewhere. Recargar y comparar; no recrear manualmente cambios revertidos |
| V2 trigger | Grupo propio inactivo | Intentar crear semillero asociado | 409 comprensible sin SQL interno | InvalidReferenceAndInactiveParentTrigger_ReturnConflictWithoutInternals. Recuperar seleccionando otro padre activo |
| Login correcto | Cuenta ficticia activa, contraseña privada | POST /api/auth/login; GET catálogo con Bearer | 200 token/expiraEn/usuario y lectura200 | Login_Read_ForbiddenWrite_Logout_Revoked; RealDatabase_AccountsBcryptRolesRevocationAndPasswordMigration. Si401, comprobar cuenta activa y reset autorizado en base de prueba |
| Contraseña incorrecta | Mismo correo; password incorrecta no vacía | POST login | 401 uniforme, no400 ni200 | BadLoginIs401. Evitar agotar cuota durante demo; no aumentar límites para recuperar acceso |
| Sin token | Cliente API nuevo | GET /api/universidad sin Authorization | 401 | MissingOrInvalidTokenIs401. Recuperación: login correcto y reenviar Bearer solo en cliente privado |
| Token expirado | Token firmado de Testing con exp pasado | GET /api/auth/me | 401 | InvalidJwtClaimsAre401(kind: expired). Usar test, nunca clave real o cambio del reloj; recuperación por login |
| Sesión revocada | Login, guardar JWT solo en memoria del test | GET200, POST logout200, repetir exactamente JWT | 401 tras revocación persistente | Login_Read_ForbiddenWrite_Logout_Revoked y prueba DB. Recuperación: nueva sesión; no reutilizar token revocado |
| Autorización denegada | Investigador autenticado | GET /api/auth/usuarios o intentar escritura | 403; lectura común sigue200 | RoleMatrix y EveryAdministrativeActionRequiresAdministrator. No cambiar rol para maquillar resultado; volver a Admin para administración |
| D01 | Clientes Flask sintéticos con dos REMOTE_ADDR y API/PG aislados | Agotar origen atacante, probar otro y cabeceras falsas | Atacante429; distinto302; falsa cabecera no reinicia cuota | LoginProtectionTests; test_login_protection.py; d01-sesiones.json. Esperar ventana o reiniciar solo instancia de prueba; no producción |
| Logout con API caída | Instancia exclusiva y JWT de prueba | API caída, logout Flask; restaurar API y consultar con mismo JWT | Sesión local borrada/advertencia, token aún200; logout remoto200 y luego401 | d01-sesiones.json. Recuperación: revocar cuando vuelva API; no declarar revocación mientras no se confirme |

## Preguntas técnicas con respuesta sustentada

| Pregunta | Respuesta y ubicación |
|---|---|
| ¿Dónde se hashea y comprueba password? | PasswordService.Hash/Verify en AuthService.cs; bcrypt12, límite 72 bytes; crear/restablecer/bootstrap llaman Hash antes del repositorio |
| ¿JWT cifra los datos? | No: firma HS256 en AuthService.GenerarJwt; JwtSettings.Validation valida firma/algoritmo/issuer/audience/exp. El payload puede leerse |
| ¿Cómo revoca un JWT firmado y válido? | CuentaRepository.Sesion exige jti vivo y cuenta/roles actuales; Revocar elimina jti; OnTokenValidated consulta BD en cada petición |
| ¿Dónde consulta los roles? | CuentaRepository.Columns: usuario_rol JOIN rol; AuthService.ValidarSesion compara conjuntos; política Datos controla verbos |
| ¿Ocultar botones basta? | No: Authorize y política Datos en API dan401/403 incluso con petición directa; Flask complementa la presentación |
| ¿La ficha usa procedimiento? | Su nombre empieza sp_, pero es función RETURNS jsonb invocada con SELECT; pg_proc.prokind=f lo demuestra |
| ¿Por qué es atómica? | Una llamada a función guarda maestro/detalles dentro del statement; excepción tardía revierte; test compara todo antes/después |
| ¿Por qué hay usuario y usuarios? | 003 es identidad, 006 personas académicas confirmadas. No hay mapeo de propietario; no prometer autoservicio docente |
| ¿Por qué C06 no está aprobado? | La fuente condiciona NULL a FK opcional; el esquema real tiene 22 FK obligatorias. Se necesita decisión docente, no inventar una relación |
| ¿Por qué pasan pruebas y faltan requisitos? | Las suites verifican contratos implementados. Los criterios originales no incluidos se contrastaron ahora, con contraejemplos y módulos ausentes trazados |
| ¿D01 permite fingir IP? | Se firma origen validado y cuerpo real con clave independiente; socket por defecto y proxies CIDR explícitos; tests de firma/cabeceras/cuotas |
| ¿Logout siempre revoca? | Solo si API confirma. Con caída se limpia local y advierte; el mismo JWT siguió200 hasta revocación explícita, después401 |
| ¿Está probado Render/Neon? | No. Solo build, integración Docker y arranque Production local; sin TLS/destinos/permiso/disco/redeploy remotos |
| ¿Cumple SOLID/WCAG totalmente? | No se certifican absolutos. Hay separación/DI y pruebas automatizadas de accesibilidad; quedan límites concretos y revisión humana |

Terminar mostrando los tres dictámenes del informe y solicitando resolver alcance histórico/C06 y autorizar el arreglo CI antes de publicar. No presentar SAPIENS V4 como iniciado.
