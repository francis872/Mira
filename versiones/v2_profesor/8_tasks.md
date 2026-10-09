# Tareas verificables V2

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
- [x] Pruebas actualizadas sin JWT ni credenciales (.NET, Flask, E2E).
- [x] Regresión automatizada de los 6 catálogos V1 contra PostgreSQL real.
- [x] Medidas generales de seguridad: validación, SQL parametrizado, CSRF en formularios.
- [x] Migración a .NET 10 LTS.
- [ ] Revisión y fusión del PR (no fusionar hasta superar las pruebas y recibir el modelo oficial).
