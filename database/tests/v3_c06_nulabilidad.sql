-- C06, fuente: PROMPT_MAESTRO_DEFINITIVO_MIRA_V2.md:406 y :707.
-- Verifica el supuesto del esquema confirmado; no inventa una FK opcional.
BEGIN;
SELECT count(*) AS columnas_fk, count(*) FILTER (WHERE NOT a.attnotnull) AS columnas_fk_opcionales
FROM pg_constraint c
JOIN pg_namespace n ON n.oid=c.connamespace
CROSS JOIN LATERAL unnest(c.conkey) key(attnum)
JOIN pg_attribute a ON a.attrelid=c.conrelid AND a.attnum=key.attnum
WHERE n.nspname='public' AND c.contype='f';
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM pg_constraint c JOIN pg_namespace n ON n.oid=c.connamespace
        CROSS JOIN LATERAL unnest(c.conkey) key(attnum)
        JOIN pg_attribute a ON a.attrelid=c.conrelid AND a.attnum=key.attnum
        WHERE n.nspname='public' AND c.contype='f' AND NOT a.attnotnull
    ) THEN
        RAISE EXCEPTION 'Cambió el supuesto C06 del modelo confirmado: revisar la fuente y las pruebas de FK opcionales';
    END IF;
END $$;
ROLLBACK;
