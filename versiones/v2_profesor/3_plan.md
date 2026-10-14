# Plan técnico V2

1. Congelar y comprobar V1 recuperada en main.
2. Comparar modelo de datos oficial con tablas del repositorio e identificar pares maestro-detalle, PK, FK, cardinalidades y borrado lógico.
3. Definir contrato JSONB de entrada y salida por par.
4. Implementar migraciones SQL, rutinas transaccionales CRUD y pruebas con rollback.
5. Implementar triggers y vistas solamente para reglas identificadas.
6. Adaptar repositorios C# para llamar las rutinas con parámetros; mantener servicios y controladores.
7. Conectar selects de FK con catálogos válidos.
8. Construir pruebas automatizadas y documentar evidencias de V1+V2.

Autenticación y autorización no forman parte del plan (ver 2_spec.md, "Corrección de alcance").

## Estado de ejecución del plan
| Paso | Estado |
|---|---|
| 1. V1 recuperada y congelada | Hecho (`main` conserva `a921770`; PR #4 parte de ese historial) |
| 2. Modelo oficial vs repositorio | **Bloqueado**: falta el modelo relacional oficial de la Entrega 2 |
| 3–4. Contratos JSONB, rutinas y pruebas con rollback | Hecho en PostgreSQL para usuario→roles; pendiente para el dominio académico |
| 5. Triggers y vistas | Pendiente (sin reglas derivadas identificadas) |
| 6–7. Repositorios C# y selects de FK | Pendiente del modelo oficial |
| 8. Pruebas V1+V2 | .NET: 9 sin base de datos (5 ejecutan, 4 requieren PostgreSQL) y 24 con `MIRA_TEST_DB`; Flask: 23; E2E contra la plataforma real: 28 (ver 7_quickstart.md) |
| Migración a .NET LTS | Hecho (.NET 10) |
| Plataforma localhost sin login | Hecho: frontend :5000, API :8081, PostgreSQL :5544 |

## Política de cambios
Nunca añadir credenciales al repositorio. Scripts idempotentes. No borrar datos existentes. Los cambios van por pull request; no fusionar `main` hasta superar las pruebas.
