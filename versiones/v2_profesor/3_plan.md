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
| 5. Triggers y vistas | Trigger de normalización de área implementado en 005; vista legada existente; reglas derivadas académicas pendientes del modelo |
| 6. Repositorios C# de V1 | Seis repositorios adaptados a 30 funciones PostgreSQL de la migración 004 |
| 7. Repositorios y selects de FK académicos | Bloqueados por el modelo oficial |
| 8. Pruebas V1+V2 | Final: 41 .NET con PostgreSQL real, 42 Flask/arquitectura, 31 E2E HTTP, SQL con rollback y 7 comprobaciones Edge. Evidencias en `docs/pruebas-v2.md`. |
| Paridad de motor 16 | Verificada: PostgreSQL 16.15 con API/Flask propios, 41/42/31 pruebas y ambos SQL aprobados en dos ejecuciones; `scripts/verify-postgres16.ps1` |
| Recuperación documental | Plan, historias y cronograma restaurados desde Git; esquema oficial V2 aún ausente. Usuario exige usar únicamente el modelo oficial. |
| Migración a .NET LTS | Hecho (.NET 10) |
| Plataforma localhost sin login | Hecho: frontend :5000, API :8081, PostgreSQL :5544 |

## Política de cambios

La nueva instrucción está conservada en `docs/prompts/PROMPT_MAESTRO_DEFINITIVO_MIRA_V2.md`. Las relaciones universidad→grupo, grupo→semillero y línea→docente son referencias por contrastar, no autorización para inventar columnas. Los doce criterios y sus bloqueos se registran en `docs/criterios-v2.md`.
Nunca añadir credenciales al repositorio. Scripts idempotentes. No borrar datos existentes. Los cambios van por pull request; no fusionar `main` hasta superar las pruebas.
