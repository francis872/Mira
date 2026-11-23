BEGIN;
DO $$
DECLARE l integer;d integer;o integer;
BEGIN
    SELECT min(id) INTO d FROM areas_conocimiento WHERE activo;
    SELECT min(id) INTO o FROM ods WHERE activo;
    BEGIN
        PERFORM sp_crear_v2_lineas_investigacion('{"codigo":"F5-MIN-INVALIDA","nombre":"No guardar","descripcion":"Prueba"}');
        RAISE EXCEPTION 'Se guardó línea sin vínculos mínimos';
    EXCEPTION WHEN check_violation THEN NULL; END;
    IF EXISTS(SELECT 1 FROM lineas_investigacion WHERE codigo='F5-MIN-INVALIDA') THEN RAISE EXCEPTION 'Se dejó una línea parcial'; END IF;
    l:=(sp_crear_v2_lineas_investigacion(jsonb_build_object('codigo','F5-MIN-VALIDA','nombre','Completa','descripcion','Prueba','disciplina_ids',jsonb_build_array(d),'ods_ids',jsonb_build_array(o)))->>'id')::integer;
    SET CONSTRAINTS ALL IMMEDIATE;
    IF (sp_estado_v2_linea(l,FALSE)->>'activo')::boolean THEN RAISE EXCEPTION 'No se inactivó la línea'; END IF;
    IF NOT (sp_estado_v2_linea(l,TRUE)->>'activo')::boolean THEN RAISE EXCEPTION 'No se reactivó la línea'; END IF;
    BEGIN
        DELETE FROM linea_ods WHERE linea_id=l;
        RAISE EXCEPTION 'Se eliminó último ODS';
    EXCEPTION WHEN foreign_key_violation THEN NULL; END;
    BEGIN
        DELETE FROM linea_disciplinas WHERE linea_id=l;
        RAISE EXCEPTION 'Se eliminó última disciplina';
    EXCEPTION WHEN foreign_key_violation THEN NULL; END;
    BEGIN
        INSERT INTO lineas_investigacion(codigo,nombre,descripcion) VALUES('F5-MIN-DIRECTA','No guardar','Prueba');
        RAISE EXCEPTION 'Un INSERT directo evitó el mínimo';
    EXCEPTION WHEN foreign_key_violation THEN NULL; END;
    IF (SELECT count(*) FROM pg_constraint WHERE connamespace='public'::regnamespace AND conname IN ('linea_minimos_original_ct','linea_disciplinas_minimos_original_ct','linea_ods_minimos_original_ct') AND condeferrable AND condeferred)<>3 THEN
        RAISE EXCEPTION 'Faltan garantías diferibles de atomicidad';
    END IF;
END; $$;
ROLLBACK;
