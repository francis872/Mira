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

## Estado de ejecución del plan
| Paso | Estado |
|---|---|
| 1. V1 recuperada y congelada | Hecho (`main` conserva `a921770`; PR #4 parte de ese historial) |
| 2. Modelo oficial vs repositorio | **Bloqueado**: falta el modelo relacional oficial de la Entrega 2 |
| 3–4. Contratos JSONB, rutinas y pruebas con rollback | Hecho para usuario→roles; pendiente para el dominio académico |
| 5. Triggers y vistas | Pendiente (sin reglas derivadas identificadas) |
| 6–7. Repositorios C# y selects de FK | Hecho para usuario→roles |
| 8. Login bcrypt y roles 401/403 | Hecho |
| 9. Pruebas V1+V2 | 47 pruebas .NET (26 requieren PostgreSQL) + 12 pruebas Flask |
| Migración a .NET LTS | Hecho (.NET 10) |

## Política de cambios
Nunca añadir credenciales al repositorio. Scripts idempotentes cuando sea posible. Los cambios van por pull request; no modificar main directamente durante desarrollo.
