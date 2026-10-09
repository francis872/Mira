# Tareas verificables V2

- [x] Migrar historial Git de V1 al repositorio de destino sin alterar autoría.
- [x] Crear rama aislada V2.
- [x] Registrar los requisitos de clase en Spec Kit V2.
- [ ] Confirmar modelo oficial completo y relaciones maestro-detalle.
- [ ] Definir semántica de borrado por entidad (lógico/físico según guía).
- [ ] Implementar migraciones PostgreSQL para FK y entidades nuevas.
- [ ] Implementar procedimientos/funciones CRUD maestro-detalle.
- [ ] Incorporar JSONB en entrada/salida y garantizar atomicidad.
- [ ] Programar triggers derivados de reglas efectivas y vistas necesarias.
- [ ] Adaptar repositorios/servicios/controladores de API.
- [ ] Reemplazar toda captura manual de FK por selects cargados desde API.
- [ ] Implementar bcrypt, login y autorización por roles con 401/403.
- [ ] Pruebas de regresión, transacciones, permisos y validación SQL.
- [ ] Documentar evidencias, revisar y fusionar PR.

La constitución (archivo 1) está en docs/1_constitution.md y se hereda de V1; no se duplica.
