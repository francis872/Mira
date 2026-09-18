# Research MIRA V1

## D1 - PostgreSQL

**Contexto:** el script real usa PostgreSQL. **Decision:** conservarlo. **Alternativas:** SQL Server, SQLite. **Justificacion:** coincide con la base entregada. **Consecuencias:** Compose usa la imagen oficial y SQL PostgreSQL.

## D2 - ASP.NET Core

**Contexto:** el backend existente es C#. **Decision:** ASP.NET Core Web API sobre .NET 10. **Alternativas:** otro framework. **Justificacion:** continuidad y Swagger integrado. **Consecuencias:** requiere SDK/runtime .NET 10 en validacion.

## D3 - SQL explicito y parametrizado

**Contexto:** se exige SQL a la vista. **Decision:** Npgsql con consultas parametrizadas; Dapper puede incorporarse solo si reduce complejidad. **Alternativas:** Entity Framework. **Justificacion:** transparencia y control. **Consecuencias:** repositories contienen SQL revisable.

## D4 - Controller-Service-Repository

**Contexto:** separar responsabilidades. **Decision:** tres capas. **Alternativas:** controller monolitico. **Justificacion:** SOLID y pruebas aisladas. **Consecuencias:** interfaces y DI.

## D5 - Repository como frontera

**Contexto:** evitar acceso disperso a BD. **Decision:** solo repositories abren conexiones y ejecutan SQL. **Alternativas:** acceso desde services. **Justificacion:** responsabilidad unica. **Consecuencias:** services dependen de interfaces.

## D6 - Tablas sin FK como V1

**Contexto:** alcance de la primera version. **Decision:** las siete tablas sin FK saliente del script real. **Alternativas:** implementar todo el modelo futuro. **Justificacion:** YAGNI. **Consecuencias:** entidades relacionadas quedan fuera.

## D7 - Catalogos como agrupacion funcional

**Contexto:** CRUD equivalente. **Decision:** agrupacion funcional sin controller gigante. **Alternativas:** un endpoint universal. **Justificacion:** cada tabla conserva responsabilidad.

## D8 - Borrado logico

**Contexto:** no perder registros. **Decision:** `activo`/`activa = false`. **Alternativas:** DELETE SQL fisico. **Justificacion:** auditoria y regla constitucional. **Consecuencias:** DELETE HTTP ejecuta UPDATE.

## D9 - Docker Compose

**Contexto:** entorno reproducible. **Decision:** API, PostgreSQL y frontend en Compose. **Alternativas:** instalaciones manuales. **Justificacion:** un comando.

## D10 - Swagger

**Contexto:** probar contratos. **Decision:** OpenAPI/Swagger. **Alternativas:** coleccion obligatoria de Postman. **Justificacion:** feedback rapido.

## D11 - Frontend por version

**Contexto:** demostrar comportamiento real. **Decision:** frontend local consume la API V1. **Alternativas:** pantalla estatica. **Justificacion:** valida integracion.

## D12 - JWT y bcrypt diferidos

**Contexto:** no pertenecen a V1. **Decision:** excluirlos del alcance activo. **Alternativas:** mantener login en esta version. **Justificacion:** SDD y YAGNI. **Consecuencias:** endpoints V1 no requieren autenticacion.

## D13 - Identificadores enteros para la base viva

**Contexto:** el modelo histórico v0.1 describe `usuarios.id` como UUID, pero la base local existente usa `SERIAL`/`INTEGER`. **Decision:** conservar `INTEGER` y no alterar PK existentes durante V1. **Justificacion:** compatibilidad con datos, FK y contratos actuales. **Consecuencias:** el módulo de investigación futuro deberá usar integer o presentar una migración de identidad formal antes de implementarse.
