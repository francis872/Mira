\set ON_ERROR_STOP on
BEGIN;
DO $$
BEGIN
 IF (SELECT count(DISTINCT substring(nombre FROM '^ODS ([0-9]+) - ')::integer) FROM public.ods
     WHERE substring(nombre FROM '^ODS ([0-9]+) - ')::integer BETWEEN 1 AND 17) <> 17 THEN
   RAISE EXCEPTION 'Falta al menos uno de los 17 ODS originales';
 END IF;
 IF NOT EXISTS(SELECT 1 FROM public.ods WHERE nombre='ODS 4 - Educacion de Calidad')
 OR NOT EXISTS(SELECT 1 FROM public.ods WHERE nombre='ODS 9 - Industria, Innovacion e Infraestructura') THEN
   RAISE EXCEPTION 'Se cambiaron los rótulos históricos';
 END IF;
END; $$;
ROLLBACK;
