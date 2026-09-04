# MIRA V1 - Catalogos

## Proposito

Implementar el CRUD de las tablas sin FK saliente identificadas en el modelo real.

## Alcance

1. `usuarios`
2. `roles`
3. `sedes`
4. `areas_conocimiento`
5. `ods`
6. `areas_aplicacion`
7. `palabras_clave`

`usuario_roles` queda fuera porque tiene dos claves foraneas.

## Incluye

- Listado de registros activos.
- Consulta por ID.
- Creacion.
- Actualizacion completa mediante PUT.
- Actualizacion parcial mediante PATCH.
- Desactivacion logica mediante DELETE HTTP.
- Frontend local de catalogos.
- Swagger/OpenAPI.

Cada tabla conserva su propio controller, service y repository.

## No incluye

- Tablas con FK.
- Grupos, docentes, semilleros y lineas.
- Dashboards y reportes avanzados.
- Integraciones externas o machine learning.
- Login, JWT, bcrypt, Argon2, autenticacion y autorizacion final.

## Requisitos funcionales

Para cada una de las siete tablas se implementan RF-XX-01 listar activos, RF-XX-02 consultar por ID, RF-XX-03 crear, RF-XX-04 actualizar, RF-XX-05 actualizar parcialmente y RF-XX-06 desactivar logicamente.
