-- HU09-CA04: una línea guardada conserva al menos una disciplina y un ODS.
-- Las filas anteriores no se completan con relaciones ficticias. La regla se comprueba al modificarlas.
CREATE OR REPLACE VIEW public.v2_lineas_investigacion AS
SELECT l.*,
    coalesce((SELECT jsonb_agg(d.disciplina_id ORDER BY d.disciplina_id) FROM public.linea_disciplinas d WHERE d.linea_id=l.id),'[]'::jsonb) AS disciplina_ids,
    coalesce((SELECT jsonb_agg(o.ods_id ORDER BY o.ods_id) FROM public.linea_ods o WHERE o.linea_id=l.id),'[]'::jsonb) AS ods_ids
FROM public.lineas_investigacion l;

CREATE OR REPLACE FUNCTION public.mira_fase5_guardar_minimos_linea(p_id integer,p_datos jsonb) RETURNS void LANGUAGE plpgsql AS $$
DECLARE v_campo text;
BEGIN
    FOREACH v_campo IN ARRAY ARRAY['disciplina_ids','ods_ids'] LOOP
        IF p_datos->v_campo IS NULL OR jsonb_typeof(p_datos->v_campo)<>'array' OR jsonb_array_length(p_datos->v_campo) NOT BETWEEN 1 AND 200 THEN
            RAISE EXCEPTION 'Seleccione al menos una disciplina y un ODS para guardar la línea' USING ERRCODE='23514';
        END IF;
    END LOOP;
    PERFORM 1 FROM public.lineas_investigacion WHERE id=p_id FOR UPDATE;
    DELETE FROM public.linea_disciplinas WHERE linea_id=p_id AND disciplina_id NOT IN (SELECT value::integer FROM jsonb_array_elements_text(p_datos->'disciplina_ids'));
    DELETE FROM public.linea_ods WHERE linea_id=p_id AND ods_id NOT IN (SELECT value::integer FROM jsonb_array_elements_text(p_datos->'ods_ids'));
    INSERT INTO public.linea_disciplinas(linea_id,disciplina_id)
        SELECT p_id,value::integer FROM jsonb_array_elements_text(p_datos->'disciplina_ids') ORDER BY value::integer ON CONFLICT DO NOTHING;
    INSERT INTO public.linea_ods(linea_id,ods_id)
        SELECT p_id,value::integer FROM jsonb_array_elements_text(p_datos->'ods_ids') ORDER BY value::integer ON CONFLICT DO NOTHING;
END; $$;

CREATE OR REPLACE FUNCTION public.sp_crear_v2_lineas_investigacion(p_datos jsonb) RETURNS jsonb LANGUAGE plpgsql AS $$
DECLARE v_id integer;
BEGIN
    INSERT INTO public.lineas_investigacion(codigo,nombre,descripcion)
        VALUES(nullif(p_datos->>'codigo',''),nullif(p_datos->>'nombre',''),nullif(p_datos->>'descripcion','')) RETURNING id INTO v_id;
    PERFORM public.mira_fase5_guardar_minimos_linea(v_id,p_datos);
    RETURN public.sp_consultar_v2_lineas_investigacion(v_id);
END; $$;
CREATE OR REPLACE FUNCTION public.sp_actualizar_v2_lineas_investigacion(p_id integer,p_datos jsonb) RETURNS jsonb LANGUAGE plpgsql AS $$
BEGIN
    UPDATE public.lineas_investigacion SET codigo=nullif(p_datos->>'codigo',''),nombre=nullif(p_datos->>'nombre',''),descripcion=nullif(p_datos->>'descripcion','') WHERE id=p_id AND activo;
    IF NOT FOUND THEN RETURN NULL; END IF;
    PERFORM public.mira_fase5_guardar_minimos_linea(p_id,p_datos);
    RETURN public.sp_consultar_v2_lineas_investigacion(p_id);
END; $$;

-- Serializar cambios de puentes en la fila padre evita carreras al retirar el último vínculo.
CREATE OR REPLACE FUNCTION public.mira_fase5_bloquear_linea() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF TG_OP='DELETE' THEN
        PERFORM 1 FROM public.lineas_investigacion WHERE id=OLD.linea_id FOR UPDATE;
        RETURN OLD;
    ELSIF TG_OP='UPDATE' THEN
        PERFORM 1 FROM public.lineas_investigacion WHERE id IN (OLD.linea_id,NEW.linea_id) ORDER BY id FOR UPDATE;
    ELSE
        PERFORM 1 FROM public.lineas_investigacion WHERE id=NEW.linea_id FOR UPDATE;
    END IF;
    RETURN NEW;
END; $$;
CREATE TRIGGER linea_disciplinas_bloqueo_original BEFORE INSERT OR UPDATE OR DELETE ON public.linea_disciplinas FOR EACH ROW EXECUTE FUNCTION public.mira_fase5_bloquear_linea();
CREATE TRIGGER linea_ods_bloqueo_original BEFORE INSERT OR UPDATE OR DELETE ON public.linea_ods FOR EACH ROW EXECUTE FUNCTION public.mira_fase5_bloquear_linea();

CREATE OR REPLACE FUNCTION public.mira_fase5_comprobar_minimos(p_id integer) RETURNS void LANGUAGE plpgsql AS $$
BEGIN
    IF EXISTS(SELECT 1 FROM public.lineas_investigacion WHERE id=p_id)
       AND (NOT EXISTS(SELECT 1 FROM public.linea_disciplinas WHERE linea_id=p_id) OR NOT EXISTS(SELECT 1 FROM public.linea_ods WHERE linea_id=p_id)) THEN
        RAISE EXCEPTION 'La línea debe conservar al menos una disciplina y un ODS' USING ERRCODE='23503';
    END IF;
END; $$;
CREATE OR REPLACE FUNCTION public.mira_fase5_linea_completa() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE v_id integer;
BEGIN
    IF TG_TABLE_NAME='lineas_investigacion' THEN
        PERFORM public.mira_fase5_comprobar_minimos(NEW.id);
    ELSE
        IF TG_OP IN ('DELETE','UPDATE') THEN PERFORM public.mira_fase5_comprobar_minimos(OLD.linea_id); END IF;
        IF TG_OP IN ('INSERT','UPDATE') THEN PERFORM public.mira_fase5_comprobar_minimos(NEW.linea_id); END IF;
    END IF;
    RETURN NULL;
END; $$;
CREATE CONSTRAINT TRIGGER linea_minimos_original_ct AFTER INSERT OR UPDATE ON public.lineas_investigacion
    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION public.mira_fase5_linea_completa();
CREATE CONSTRAINT TRIGGER linea_disciplinas_minimos_original_ct AFTER INSERT OR UPDATE OR DELETE ON public.linea_disciplinas
    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION public.mira_fase5_linea_completa();
CREATE CONSTRAINT TRIGGER linea_ods_minimos_original_ct AFTER INSERT OR UPDATE OR DELETE ON public.linea_ods
    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION public.mira_fase5_linea_completa();
