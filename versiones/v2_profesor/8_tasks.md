# Tareas verificables V2

- [x] Recuperar plan, historias y cronograma auténticos del historial, con trazabilidad de blobs, en `docs/fuentes/historicos/`.
- [x] Ejecutar PostgreSQL 16.15 con API/frontend independientes: 41 .NET, 42 Flask/arquitectura, 31 HTTP y ambos SQL aprobados; repetibilidad y cierre de procesos comprobados.
- [ ] Implementar el dominio académico únicamente cuando se aporte su esquema oficial. El usuario descartó expresamente diseñar un modelo provisional; la recuperación histórica no resuelve esa diferencia.

- [x] Guardar íntegro el prompt definitivo auténtico y documentar su procedencia, sin atribuirlo a integrantes.
- [x] Migración 004: 30 funciones, cinco por catálogo; seis repositorios C# solo invocan rutinas parametrizadas.
- [x] Migración 005: trigger de normalización de área; pruebas SQL, API y formulario.
- [x] Middleware SQLSTATE con respuestas seguras y validación de contrato en español; logs a consola.
- [x] Búsqueda local, navegación activa, accesibilidad básica y estado de envío, sin ampliar alcance.
- [x] Comprobación real en Edge de búsqueda, modales, alta, edición, retiro y menú móvil; capturas en `docs/evidencias/`.
- [x] Matriz de doce criterios en `docs/criterios-v2.md` y reglas de negocio por entidad.
- [x] Verificación final: 41 .NET, 42 Flask/arquitectura, 31 HTTP, dos SQL con rollback y 7 comprobaciones Edge. Persistencia comprobada tras reiniciar con nuevas rutinas.
- [ ] Localizar modelo académico, VERSION_2 y estructura oficial de nueve documentos. El usuario no recuerda su ubicación.

- [x] Auditar flujos reales y corregir mensajes HTTP 400/409, respuesta JSON inválida, CSRF no ASCII, longitud de textos y colisiones de términos activos/inactivos/concurrentes.
- [x] Primera auditoría (histórico): 31 .NET, 40 Flask, 30 E2E HTTP y SQL con rollback.
- [x] Comprobar persistencia tras reiniciar PostgreSQL, API y frontend.
- [x] Documentar auditoría, arquitectura, trazabilidad, pruebas, sustentación y pendientes en `docs/*-v2.md`.

- [x] Migrar historial Git de V1 al repositorio de destino sin alterar autoría.
- [x] Crear rama aislada V2.
- [x] Registrar los requisitos de clase en Spec Kit V2.
- [ ] Confirmar modelo oficial completo y relaciones maestro-detalle. **BLOQUEADO: falta el modelo oficial de la Entrega 2.**
- [x] Migraciones controladas (`database/migrations`, `apply.sh`, `schema_migrations`) para bases nuevas y existentes.
- [x] Rutinas CRUD maestro-detalle con JSONB y atomicidad probada en PostgreSQL para usuario→roles (sin pantalla; dominio académico pendiente).
- [ ] Triggers derivados de reglas efectivas. Vista `vw_usuarios_roles` hecha; triggers pendientes del modelo oficial.
- [ ] Selects de FK del dominio académico y repositorios C# que llamen las rutinas. Pendiente del modelo oficial.
- [x] **Retirar la autenticación del alcance activo** (rutas, JWT, bcrypt, pantallas, `--bootstrap-admin`, pruebas); historial y etiqueta `auth-futuro-v2-261872f` conservados; tablas `usuario/rol/usuario_rol` intactas.
- [x] Plataforma localhost sin login: dashboard con conteos reales y seis módulos operativos.
- [x] Arranque reproducible en Windows sin Docker (`scripts/start-local.ps1`), PostgreSQL exclusivo, migraciones controladas y parada con conservación de datos (`scripts/stop-local.ps1`). Verificado con 28 pruebas E2E, 24 pruebas .NET con PostgreSQL y 23 pruebas Flask.
- [x] Pruebas actualizadas sin JWT ni credenciales (.NET, Flask, E2E).
- [x] Regresión automatizada de los 6 catálogos V1 contra PostgreSQL real.
- [x] Medidas generales de seguridad: validación, SQL parametrizado, CSRF en formularios.
- [x] Migración a .NET 10 LTS.
- [ ] Revisión y fusión del PR (no fusionar hasta superar las pruebas y recibir el modelo oficial).
