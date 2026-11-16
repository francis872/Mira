BEGIN;
DO $$
DECLARE s integer;g integer;l integer;ls integer; result jsonb; n integer;
BEGIN
    SELECT count(*) INTO n FROM pg_constraint WHERE connamespace='public'::regnamespace
       AND conname IN ('sedes_codigo_original_ck','palabras_clave_ingles_original_ck','lineas_descripcion_original_ck');
    IF n<>3 THEN RAISE EXCEPTION 'Faltan restricciones justificadas por las historias'; END IF;
    BEGIN
        PERFORM sp_crear_v2_sedes('{"nombre":"Sin código","ciudad":"Prueba"}');
        RAISE EXCEPTION 'Sede sin código aceptada';
    EXCEPTION WHEN check_violation THEN NULL; END;
    BEGIN
        PERFORM sp_crear_v2_palabras_clave('{"nombre":"Sin inglés"}');
        RAISE EXCEPTION 'Palabra sin inglés aceptada';
    EXCEPTION WHEN check_violation THEN NULL; END;
    BEGIN
        PERFORM sp_crear_v2_lineas_investigacion('{"codigo":"F5-SIN-DESC","nombre":"Sin descripción"}');
        RAISE EXCEPTION 'Línea sin descripción aceptada';
    EXCEPTION WHEN check_violation THEN NULL; END;
    s:=(sp_crear_v2_sedes('{"nombre":"Prueba F5","ciudad":"Prueba","codigo_institucional":"PRUEBA-SQL-F5"}')->>'id')::integer;
    g:=(sp_crear_v2_grupos_investigacion(jsonb_build_object('sede_id',s,'gruptac_url','https://example.invalid/f5'))->>'id')::integer;
    l:=(sp_crear_v2_lineas_investigacion(jsonb_build_object('codigo','F5-L1','nombre','Línea 1','descripcion','Prueba',
        'disciplina_ids',jsonb_build_array((SELECT min(id) FROM areas_conocimiento WHERE activo)),
        'ods_ids',jsonb_build_array((SELECT min(id) FROM ods WHERE activo))))->>'id')::integer;
    ls:=(sp_crear_v2_lineas_investigacion(jsonb_build_object('codigo','F5-L2','nombre','Línea 2','descripcion','Prueba',
        'disciplina_ids',jsonb_build_array((SELECT min(id) FROM areas_conocimiento WHERE activo)),
        'ods_ids',jsonb_build_array((SELECT min(id) FROM ods WHERE activo))))->>'id')::integer;
    BEGIN
        PERFORM sp_asignar_v2_lineas('grupo_lineas',g,jsonb_build_array(l,2147483647));
        RAISE EXCEPTION 'Lote inválido aceptado';
    EXCEPTION WHEN foreign_key_violation THEN NULL; END;
    IF EXISTS(SELECT 1 FROM grupo_lineas WHERE grupo_id=g) THEN RAISE EXCEPTION 'El lote no fue atómico'; END IF;
    result:=sp_asignar_v2_lineas('grupo_lineas',g,jsonb_build_array(l,ls));
    IF jsonb_array_length(result)<>2 THEN RAISE EXCEPTION 'Falta multiselección'; END IF;
    PERFORM sp_eliminar_v2_lineas_investigacion(l);
    IF jsonb_array_length(sp_resumen_v2_lineas('grupo_lineas',g))<>2 THEN RAISE EXCEPTION 'Se perdió historia de líneas'; END IF;
    BEGIN
        INSERT INTO semillero_lineas(semillero_id,linea_id) VALUES(2147483647,l);
        RAISE EXCEPTION 'Línea inactiva aceptada';
    EXCEPTION WHEN foreign_key_violation THEN NULL; END;
    PERFORM sp_eliminar_v2_sedes(s);
    BEGIN
        PERFORM sp_crear_v2_grupos_investigacion(jsonb_build_object('sede_id',s,'gruptac_url','https://example.invalid/nueva'));
        RAISE EXCEPTION 'Nueva sede inactiva aceptada';
    EXCEPTION WHEN foreign_key_violation THEN NULL; END;
    IF NOT EXISTS(SELECT 1 FROM grupos_investigacion WHERE id=g AND sede_id=s) THEN RAISE EXCEPTION 'Se perdió grupo histórico'; END IF;
END; $$;
ROLLBACK;
