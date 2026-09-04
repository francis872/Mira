# Plan MIRA V1 - Catalogos

## Arquitectura

Frontend -> Controller -> Service -> Repository -> PostgreSQL

El Repository es la unica capa con acceso directo a PostgreSQL. Se usara C#, ASP.NET Core Web API, SQL parametrizado visible, Npgsql, Swagger y Docker Compose. Dapper queda reservado si se incorpora un micro ORM en una iteracion posterior; el modelo actual ya usa SQL explicito con Npgsql.

## Estructura

Para cada tabla se mantienen Modelos, Peticiones, Repositorios, Servicios y Controllers separados. La inyeccion de dependencias se registra en `MiraServiceCollectionExtensions`.

## Archivos

- Modificar `database/init/01-init.sql` solo si una validacion demuestra que falta la columna de estado real.
- Modificar los repositories y services existentes para completar CRUD.
- Crear interfaces y clases por catalogo.
- Modificar controllers para GET por ID, PUT, PATCH y DELETE logico.
- Mantener frontend consumiendo HTTP, nunca PostgreSQL.

## Validacion por fase

1. Validar documentos y lista real de tablas.
2. Compilar modelos y repositories.
3. Compilar services y controllers.
4. Levantar API y Swagger.
5. Ejecutar Compose y probar CRUD completo.
6. Comparar Spec Kit con codigo.
