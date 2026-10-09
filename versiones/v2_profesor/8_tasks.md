# Tareas verificables V2

- [x] Migrar historial Git de V1 al repositorio de destino sin alterar autoría.
- [x] Crear rama aislada V2.
- [x] Registrar los requisitos de clase en Spec Kit V2.
- [ ] Confirmar modelo oficial completo y relaciones maestro-detalle. **BLOQUEADO: falta el modelo oficial de la Entrega 2.**
- [x] Definir semántica de borrado para usuario→roles (lógico). Pendiente por entidad académica.
- [x] Migraciones PostgreSQL para usuario/rol/usuario_rol (`002-v2-usuarios-roles.sql`, idempotente). Dominio académico pendiente.
- [x] Procedimientos CRUD maestro-detalle para usuario→roles. Dominio académico pendiente.
- [x] JSONB en entrada/salida y atomicidad probada (rollback verificado) para usuario→roles.
- [ ] Triggers derivados de reglas efectivas. Vista `vw_usuarios_roles` hecha; triggers pendientes del modelo oficial.
- [x] Adaptar repositorios/servicios/controladores de API (`AuthService`, catálogos protegidos).
- [x] Selects de FK cargados desde API (roles). No existen otros FK.
- [x] bcrypt, login y autorización por roles con 401/403 (API y Flask).
- [x] Pruebas de regresión, transacciones, permisos y SQL: 28 .NET + 13 Flask + smoke E2E.
- [x] Aprovisionamiento seguro del primer administrador (`--bootstrap-admin`).
- [x] Migración a .NET 10 LTS.
- [ ] Token CSRF por formulario en Flask.
- [ ] Regresión manual en navegador de los 6 formularios V1 con sesión.
- [ ] Revisión y fusión del PR (no fusionar hasta cerrar los pendientes y recibir el modelo oficial).

La constitución (archivo 1) está en docs/1_constitution.md y se hereda de V1; no se duplica.
