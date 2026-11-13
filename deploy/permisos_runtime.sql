-- Ejecutar como operador de migraciones en la base autorizada, después de 001–015.
-- Los roles LOGIN mira_api y mira_web se crean por canal privado, sin claves aquí.
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
REVOKE ALL ON SCHEMA mira_web FROM PUBLIC;
REVOKE ALL ON ALL ROUTINES IN SCHEMA public FROM PUBLIC;

GRANT USAGE ON SCHEMA public TO mira_api;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO mira_api;
REVOKE ALL ON public.schema_migrations FROM mira_api;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO mira_api;
GRANT EXECUTE ON ALL ROUTINES IN SCHEMA public TO mira_api;

GRANT USAGE ON SCHEMA mira_web TO mira_web;
GRANT SELECT, INSERT, UPDATE, DELETE ON mira_web.sesion TO mira_web;
REVOKE ALL ON ALL TABLES IN SCHEMA public FROM mira_web;
REVOKE ALL ON ALL SEQUENCES IN SCHEMA public FROM mira_web;
REVOKE ALL ON ALL ROUTINES IN SCHEMA public FROM mira_web;

-- No se conceden DDL, propiedad, TRUNCATE, roles ni privilegios de migrador.
-- Cada nueva migración revisa los GRANT de sus objetos antes de desplegar.
