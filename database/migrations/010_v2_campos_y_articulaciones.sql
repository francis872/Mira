-- Historias originales HU02, HU04, HU06, HU07 y HU08. No modifica 001-009.
-- Las filas anteriores sin código/inglés/descripción se conservan para completarlas,
-- sin inventar códigos institucionales, traducciones o descripciones.
ALTER TABLE public.sedes ADD COLUMN codigo_institucional text;
ALTER TABLE public.palabras_clave ADD COLUMN termino_ingles varchar(150);
ALTER TABLE public.sedes ADD CONSTRAINT sedes_codigo_original_ck
    CHECK (codigo_institucional IS NOT NULL AND btrim(codigo_institucional)<>'') NOT VALID;
ALTER TABLE public.palabras_clave ADD CONSTRAINT palabras_clave_ingles_original_ck
    CHECK (termino_ingles IS NOT NULL AND btrim(termino_ingles)<>'') NOT VALID;
ALTER TABLE public.lineas_investigacion ADD CONSTRAINT lineas_descripcion_original_ck
    CHECK (descripcion IS NOT NULL AND btrim(descripcion)<>'') NOT VALID;

CREATE OR REPLACE VIEW public.v2_sedes AS SELECT t.* FROM public.sedes t;
CREATE OR REPLACE VIEW public.v2_palabras_clave AS SELECT t.* FROM public.palabras_clave t;

CREATE OR REPLACE FUNCTION public.sp_crear_v2_sedes(p_datos jsonb) RETURNS jsonb LANGUAGE plpgsql AS $$
DECLARE v_id integer;
BEGIN
    INSERT INTO public.sedes(nombre,ciudad,codigo_institucional)
    VALUES (nullif(p_datos->>'nombre',''),nullif(p_datos->>'ciudad',''),nullif(btrim(p_datos->>'codigo_institucional'),'')) RETURNING id INTO v_id;
    RETURN public.sp_consultar_v2_sedes(v_id);
END; $$;
CREATE OR REPLACE FUNCTION public.sp_actualizar_v2_sedes(p_id integer,p_datos jsonb) RETURNS jsonb LANGUAGE plpgsql AS $$
BEGIN
    UPDATE public.sedes SET nombre=nullif(p_datos->>'nombre',''),ciudad=nullif(p_datos->>'ciudad',''),
        codigo_institucional=nullif(btrim(p_datos->>'codigo_institucional'),'') WHERE id=p_id AND activa;
    IF NOT FOUND THEN RETURN NULL; END IF;
    RETURN public.sp_consultar_v2_sedes(p_id);
END; $$;
CREATE OR REPLACE FUNCTION public.sp_crear_v2_palabras_clave(p_datos jsonb) RETURNS jsonb LANGUAGE plpgsql AS $$
DECLARE v_id integer;
BEGIN
    INSERT INTO public.palabras_clave(nombre,termino_ingles)
    VALUES (nullif(p_datos->>'nombre',''),nullif(btrim(p_datos->>'termino_ingles'),'')) RETURNING id INTO v_id;
    RETURN public.sp_consultar_v2_palabras_clave(v_id);
END; $$;
CREATE OR REPLACE FUNCTION public.sp_actualizar_v2_palabras_clave(p_id integer,p_datos jsonb) RETURNS jsonb LANGUAGE plpgsql AS $$
BEGIN
    UPDATE public.palabras_clave SET nombre=nullif(p_datos->>'nombre',''),termino_ingles=nullif(btrim(p_datos->>'termino_ingles'),'') WHERE id=p_id AND activo;
    IF NOT FOUND THEN RETURN NULL; END IF;
    RETURN public.sp_consultar_v2_palabras_clave(p_id);
END; $$;

CREATE OR REPLACE FUNCTION public.mira_fase5_sede_activa() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF TG_OP='UPDATE' AND NEW.sede_id=OLD.sede_id THEN RETURN NEW; END IF;
    IF NOT EXISTS(SELECT 1 FROM public.sedes WHERE id=NEW.sede_id AND activa FOR SHARE) THEN
        RAISE EXCEPTION 'Seleccione una sede activa para una nueva asignación' USING ERRCODE='23503';
    END IF;
    RETURN NEW;
END; $$;
CREATE TRIGGER grupos_sede_activa_original_trigger BEFORE INSERT OR UPDATE OF sede_id ON public.grupos_investigacion
    FOR EACH ROW EXECUTE FUNCTION public.mira_fase5_sede_activa();

CREATE OR REPLACE FUNCTION public.mira_fase5_linea_activa() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF TG_OP='UPDATE' AND NEW.linea_id=OLD.linea_id THEN RETURN NEW; END IF;
    IF NOT EXISTS(SELECT 1 FROM public.lineas_investigacion WHERE id=NEW.linea_id AND activo AND deleted_at IS NULL FOR SHARE) THEN
        RAISE EXCEPTION 'Seleccione una línea activa para una nueva articulación' USING ERRCODE='23503';
    END IF;
    RETURN NEW;
END; $$;
CREATE TRIGGER grupo_lineas_activa_original_trigger BEFORE INSERT OR UPDATE OF linea_id ON public.grupo_lineas
    FOR EACH ROW EXECUTE FUNCTION public.mira_fase5_linea_activa();
CREATE TRIGGER semillero_lineas_activa_original_trigger BEFORE INSERT OR UPDATE OF linea_id ON public.semillero_lineas
    FOR EACH ROW EXECUTE FUNCTION public.mira_fase5_linea_activa();

CREATE OR REPLACE FUNCTION public.sp_asignar_v2_lineas(p_recurso text,p_padre integer,p_lineas jsonb) RETURNS jsonb LANGUAGE plpgsql AS $$
DECLARE v_linea integer; v_resultado jsonb='[]'::jsonb; v_fila jsonb;
BEGIN
    IF p_lineas IS NULL OR jsonb_typeof(p_lineas)<>'array' OR jsonb_array_length(p_lineas) NOT BETWEEN 1 AND 200 THEN
        RAISE EXCEPTION 'Seleccione entre una y doscientas líneas' USING ERRCODE='23514';
    END IF;
    IF p_recurso='grupo_lineas' THEN
        PERFORM 1 FROM public.grupos_investigacion WHERE id=p_padre AND activo FOR SHARE;
    ELSIF p_recurso='semillero_lineas' THEN
        PERFORM 1 FROM public.semilleros WHERE id=p_padre AND activo FOR SHARE;
    ELSE
        RAISE EXCEPTION 'Asociación no permitida' USING ERRCODE='23514';
    END IF;
    IF NOT FOUND THEN RAISE EXCEPTION 'Seleccione un registro activo' USING ERRCODE='23503'; END IF;
    -- Orden estable y una transacción: un duplicado o referencia inválida revierte el lote completo.
    FOR v_linea IN SELECT value::integer FROM jsonb_array_elements_text(p_lineas) ORDER BY value::integer LOOP
        IF p_recurso='grupo_lineas' THEN
            v_fila:=public.sp_crear_v2_grupo_lineas(jsonb_build_object('grupo_id',p_padre,'linea_id',v_linea));
        ELSE
            v_fila:=public.sp_crear_v2_semillero_lineas(jsonb_build_object('semillero_id',p_padre,'linea_id',v_linea));
        END IF;
        v_resultado:=v_resultado||jsonb_build_array(v_fila);
    END LOOP;
    RETURN v_resultado;
END; $$;

CREATE OR REPLACE FUNCTION public.sp_resumen_v2_lineas(p_recurso text,p_padre integer) RETURNS jsonb LANGUAGE plpgsql STABLE AS $$
BEGIN
    IF p_recurso='grupo_lineas' THEN
        IF NOT EXISTS(SELECT 1 FROM public.grupos_investigacion WHERE id=p_padre AND activo) THEN RETURN NULL; END IF;
        RETURN (SELECT coalesce(jsonb_agg(jsonb_build_object('id',l.id,'codigo',l.codigo,'nombre',l.nombre,'activo',l.activo) ORDER BY l.codigo),'[]'::jsonb)
                FROM public.grupo_lineas a JOIN public.lineas_investigacion l ON l.id=a.linea_id WHERE a.grupo_id=p_padre);
    ELSIF p_recurso='semillero_lineas' THEN
        IF NOT EXISTS(SELECT 1 FROM public.semilleros WHERE id=p_padre AND activo) THEN RETURN NULL; END IF;
        RETURN (SELECT coalesce(jsonb_agg(jsonb_build_object('id',l.id,'codigo',l.codigo,'nombre',l.nombre,'activo',l.activo) ORDER BY l.codigo),'[]'::jsonb)
                FROM public.semillero_lineas a JOIN public.lineas_investigacion l ON l.id=a.linea_id WHERE a.semillero_id=p_padre);
    END IF;
    RETURN NULL;
END; $$;

CREATE OR REPLACE FUNCTION public.sp_filtrar_v2_semilleros(p_grupo integer,p_docente integer) RETURNS jsonb LANGUAGE sql STABLE AS $$
SELECT coalesce(jsonb_agg(to_jsonb(s) ORDER BY s.id),'[]'::jsonb)
FROM public.v2_semilleros s WHERE s.activo
AND (p_grupo IS NULL OR s.grupo_id=p_grupo)
AND (p_docente IS NULL OR EXISTS(SELECT 1 FROM public.semillero_tutores t WHERE t.semillero_id=s.id AND t.docente_id=p_docente));
$$;
