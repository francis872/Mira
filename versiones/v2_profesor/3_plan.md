# Plan técnico V2

1. Congelar y comprobar V1 recuperada en main.
2. Comparar modelo de datos oficial con tablas del repositorio e identificar pares maestro-detalle, PK, FK, cardinalidades y borrado lógico.
3. Definir contrato JSONB de entrada y salida por par.
4. Implementar migraciones SQL, rutinas transaccionales CRUD y pruebas con rollback.
5. Implementar triggers y vistas solamente para reglas identificadas.
6. Adaptar repositorios C# para llamar las rutinas con parámetros; mantener servicios y controladores.
7. Conectar selects de FK con catálogos válidos y filtros de permisos.
8. Implementar login bcrypt y autorización por roles; verificar 401/403.
9. Construir pruebas automatizadas y documentar evidencias de V1+V2.

## Política de cambios
Nunca añadir credenciales al repositorio. Scripts idempotentes cuando sea posible. Los cambios van por pull request; no modificar main directamente durante desarrollo.
