BEGIN;
DO $$
DECLARE s integer;g integer;l integer;o integer;p integer;d integer;r jsonb;
BEGIN
    s:=(sp_crear_v2_sedes('{"nombre":"Sede reportes SQL","ciudad":"Prueba","codigo_institucional":"PRUEBA-REPORTES"}')->>'id')::integer;
    g:=(sp_crear_v2_grupos_investigacion(jsonb_build_object('sede_id',s,'gruptac_url','https://example.invalid/reportes'))->>'id')::integer;
    o:=(sp_crear_v2_ods('{"nombre":"ODS reportes"}')->>'id')::integer;
    l:=(sp_crear_v2_lineas_investigacion(jsonb_build_object('codigo','REP-F5','nombre','Línea reportes','descripcion','Descripción',
        'disciplina_ids',jsonb_build_array((SELECT min(id) FROM areas_conocimiento WHERE activo)),'ods_ids',jsonb_build_array(o)))->>'id')::integer;
    PERFORM sp_crear_v2_grupo_lineas(jsonb_build_object('grupo_id',g,'linea_id',l));
    INSERT INTO usuarios(nombre,correo,password_hash) VALUES('Persona reporte ficticio','reportes-sql@invalid.test','NO_ES_UNA_CREDENCIAL') RETURNING id INTO p;
    d:=(sp_crear_v2_docentes(jsonb_build_object('usuario_id',p))->>'id')::integer;
    PERFORM sp_crear_v2_grupo_docentes(jsonb_build_object('grupo_id',g,'docente_id',d,'rol','Colaborador','fecha_inicio','2026-01-01'));
    r:=sp_reporte_v2_articulacion(s,l);
    IF jsonb_array_length(r)<>1 OR r->0->>'docente'<>'Persona reporte ficticio' THEN RAISE EXCEPTION 'Correspondencia de reporte incorrecta'; END IF;
    IF r::text LIKE '%password%' OR r::text LIKE '%correo%' THEN RAISE EXCEPTION 'El reporte expone credenciales'; END IF;
    IF jsonb_array_length(sp_reporte_v2_articulacion(s,2147483647))<>0 THEN RAISE EXCEPTION 'Se ignoró filtro de línea'; END IF;
    IF jsonb_array_length(sp_reporte_v2_integrantes(g)->'integrantes')<>1 THEN RAISE EXCEPTION 'Falta integrante'; END IF;
    IF sp_reporte_v2_integrantes(2147483647) IS NOT NULL THEN RAISE EXCEPTION 'Grupo inexistente no debe inventarse'; END IF;
END; $$;
ROLLBACK;
