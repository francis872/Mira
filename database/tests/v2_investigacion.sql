-- Only a dedicated audit database. Everything, including fixture people, is rolled back.
BEGIN;
DO $$
DECLARE v_sede integer; v_docente integer; v_user integer; v_group integer; v_line integer;
        v_sheet jsonb; v_count integer; v_semillero integer; v_other integer;
BEGIN
    -- The confirmed model has no optional foreign key: preserve that fact.
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu USING (constraint_catalog,constraint_schema,constraint_name)
        JOIN information_schema.columns c ON c.table_schema=kcu.table_schema AND c.table_name=kcu.table_name AND c.column_name=kcu.column_name
        WHERE tc.constraint_type='FOREIGN KEY' AND tc.table_schema='public'
        AND tc.table_name IN ('docentes','grupos_investigacion','grupo_docentes','semilleros','semillero_tutores','grupo_lineas','semillero_lineas','linea_disciplinas','linea_ods','linea_aplicacion')
        AND c.is_nullable='YES'
    ) THEN RAISE EXCEPTION 'Unexpected optional FK: inspect the source model'; END IF;

    v_sede := (sp_crear_v2_sedes('{"nombre":"Sede prueba SQL","ciudad":"Medellín"}')->>'id')::integer;
    INSERT INTO usuarios(nombre,correo,password_hash) VALUES ('Persona prueba SQL','sql-test@invalid.test','NO_ES_UNA_CREDENCIAL') RETURNING id INTO v_user;
    v_docente := (sp_crear_v2_docentes(jsonb_build_object('usuario_id',v_user))->>'id')::integer;
    IF (sp_consultar_v2_docentes(v_docente)->'cvlac_url') IS DISTINCT FROM 'null'::jsonb THEN RAISE EXCEPTION 'Optional text must remain null'; END IF;
    v_sheet := sp_guardar_v2_ficha_grupo(NULL,jsonb_build_object(
        'grupo',jsonb_build_object('sede_id',v_sede,'gruptac_url','https://example.org/grupo'),
        'semilleros',jsonb_build_array(jsonb_build_object('nombre','Primero'),jsonb_build_object('nombre','Segundo'))));
    v_group := (v_sheet->'grupo'->>'id')::integer;
    v_semillero := (v_sheet->'semilleros'->0->>'id')::integer;
    IF jsonb_array_length(v_sheet->'semilleros')<>2 THEN RAISE EXCEPTION 'Missing detail rows'; END IF;
    v_line := (sp_crear_v2_lineas_investigacion('{"codigo":"TEST-SQL-V2","nombre":"Línea prueba"}')->>'id')::integer;
    PERFORM sp_crear_v2_grupo_lineas(jsonb_build_object('grupo_id',v_group,'linea_id',v_line));
    BEGIN
        PERFORM sp_crear_v2_grupo_lineas(jsonb_build_object('grupo_id',v_group,'linea_id',v_line));
        RAISE EXCEPTION 'Duplicate association accepted';
    EXCEPTION WHEN unique_violation THEN NULL; END;
    BEGIN
        PERFORM sp_crear_v2_grupo_lineas(jsonb_build_object('grupo_id',v_group,'linea_id',2147483647));
        RAISE EXCEPTION 'Invalid FK accepted';
    EXCEPTION WHEN foreign_key_violation THEN NULL; END;
    IF NOT sp_eliminar_v2_grupo_lineas(v_group,v_line) THEN RAISE EXCEPTION 'Association not removed'; END IF;

    SELECT count(*) INTO v_count FROM grupos_investigacion;
    BEGIN
        PERFORM sp_guardar_v2_ficha_grupo(NULL,jsonb_build_object(
            'grupo',jsonb_build_object('sede_id',v_sede,'gruptac_url','https://example.org/rollback'),
            'semilleros',jsonb_build_array(jsonb_build_object('nombre','Must roll back'),jsonb_build_object('descripcion','Missing name'))));
        RAISE EXCEPTION 'Invalid third write accepted';
    EXCEPTION WHEN not_null_violation THEN NULL; END;
    IF (SELECT count(*) FROM grupos_investigacion)<>v_count OR EXISTS(SELECT 1 FROM semilleros WHERE nombre='Must roll back') THEN
        RAISE EXCEPTION 'Atomic create failed';
    END IF;
    BEGIN
        PERFORM sp_guardar_v2_ficha_grupo(v_group,jsonb_build_object(
            'grupo',jsonb_build_object('sede_id',v_sede,'gruptac_url','https://example.org/changed'),
            'semilleros',jsonb_build_array(jsonb_build_object('id',v_semillero,'nombre','Should roll back'),jsonb_build_object('id',2147483647,'nombre','Wrong owner'))));
        RAISE EXCEPTION 'Invalid owner accepted';
    EXCEPTION WHEN foreign_key_violation THEN NULL; END;
    IF (SELECT nombre FROM semilleros WHERE id=v_semillero)<>'Primero' OR (SELECT gruptac_url FROM grupos_investigacion WHERE id=v_group)<>'https://example.org/grupo' THEN
        RAISE EXCEPTION 'Atomic update failed';
    END IF;
    PERFORM sp_guardar_v2_ficha_grupo(v_group,jsonb_build_object(
        'grupo',jsonb_build_object('sede_id',v_sede,'gruptac_url','https://example.org/grupo'),
        'semilleros',jsonb_build_array(jsonb_build_object('id',v_semillero,'nombre','Editado'))));
    IF (SELECT count(*) FROM semilleros WHERE grupo_id=v_group AND activo)<>1 OR (SELECT count(*) FROM semilleros WHERE grupo_id=v_group)<>2 THEN
        RAISE EXCEPTION 'Missing logical detail removal';
    END IF;
    PERFORM sp_eliminar_v2_grupos_investigacion(v_group);
    BEGIN
        PERFORM sp_crear_v2_semilleros(jsonb_build_object('grupo_id',v_group,'nombre','Inactive parent'));
        RAISE EXCEPTION 'Trigger did not reject inactive group';
    EXCEPTION WHEN foreign_key_violation THEN NULL; END;
    IF sp_consultar_v2_grupos_investigacion(v_group) IS NOT NULL OR NOT EXISTS(SELECT 1 FROM grupos_investigacion WHERE id=v_group AND NOT activo) THEN
        RAISE EXCEPTION 'Logical removal not preserved';
    END IF;
END; $$;
ROLLBACK;
