-- Build the unmodified confirmed source in an isolated, rolled-back schema and compare metadata.
BEGIN;
CREATE SCHEMA mira_source_model;
SET LOCAL search_path = mira_source_model;
\ir ../../docs/fuentes/sql/init/01-init.sql
SET LOCAL search_path = public;
DO $$
DECLARE v_count integer;
BEGIN
    SELECT count(*) INTO v_count FROM information_schema.tables WHERE table_schema='mira_source_model' AND table_type='BASE TABLE';
    IF v_count<>19 THEN RAISE EXCEPTION 'Expected 19 source tables, got %',v_count; END IF;
    WITH expected AS (
        SELECT table_name,column_name,ordinal_position,udt_name,character_maximum_length,is_nullable,datetime_precision
        FROM information_schema.columns WHERE table_schema='mira_source_model'
    ), actual AS (
        SELECT table_name,column_name,ordinal_position,udt_name,character_maximum_length,is_nullable,datetime_precision
        FROM information_schema.columns WHERE table_schema='public' AND table_name IN (SELECT table_name FROM expected)
    ), differences AS ((SELECT * FROM expected EXCEPT SELECT * FROM actual) UNION ALL (SELECT * FROM actual EXCEPT SELECT * FROM expected))
    SELECT count(*) INTO v_count FROM differences;
    IF v_count<>0 THEN RAISE EXCEPTION 'Official column mismatch (% differences)',v_count; END IF;

    WITH definitions AS (
        SELECT n.nspname AS schema_name,t.relname,c.contype,
            replace(replace(pg_get_constraintdef(c.oid),'mira_source_model.',''),'public.','') AS definition
        FROM pg_constraint c JOIN pg_class t ON t.oid=c.conrelid JOIN pg_namespace n ON n.oid=t.relnamespace
        WHERE n.nspname IN ('public','mira_source_model')
    ), expected AS (SELECT relname,contype,definition FROM definitions WHERE schema_name='mira_source_model'),
    actual AS (SELECT relname,contype,definition FROM definitions WHERE schema_name='public' AND relname IN (SELECT relname FROM expected)),
    differences AS ((SELECT * FROM expected EXCEPT SELECT * FROM actual) UNION ALL (SELECT * FROM actual EXCEPT SELECT * FROM expected))
    SELECT count(*) INTO v_count FROM differences;
    IF v_count<>0 THEN RAISE EXCEPTION 'Official PK/FK/unique/check mismatch (% differences)',v_count; END IF;

    WITH definitions AS (
        SELECT n.nspname AS schema_name,t.relname,
            replace(replace(pg_get_indexdef(i.indexrelid),'mira_source_model.',''),'public.','') AS definition
        FROM pg_index i JOIN pg_class t ON t.oid=i.indrelid JOIN pg_namespace n ON n.oid=t.relnamespace
        WHERE n.nspname IN ('public','mira_source_model')
    ), expected AS (SELECT relname,definition FROM definitions WHERE schema_name='mira_source_model'),
    actual AS (SELECT relname,definition FROM definitions WHERE schema_name='public' AND relname IN (SELECT relname FROM expected)),
    differences AS ((SELECT * FROM expected EXCEPT SELECT * FROM actual) UNION ALL (SELECT * FROM actual EXCEPT SELECT * FROM expected))
    SELECT count(*) INTO v_count FROM differences;
    IF v_count<>0 THEN RAISE EXCEPTION 'Official index mismatch (% differences)',v_count; END IF;
END; $$;
ROLLBACK;
