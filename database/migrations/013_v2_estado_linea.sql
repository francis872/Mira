-- HU07-CA02: estados Activa/Inactiva. No borra asociaciones ni cambia permisos V3.
CREATE OR REPLACE FUNCTION public.sp_listar_v2_lineas_inactivas() RETURNS jsonb LANGUAGE sql STABLE AS $$
SELECT coalesce(jsonb_agg(to_jsonb(l) ORDER BY l.codigo),'[]'::jsonb) FROM public.v2_lineas_investigacion l WHERE NOT l.activo;
$$;
CREATE OR REPLACE FUNCTION public.sp_estado_v2_linea(p_id integer,p_activo boolean) RETURNS jsonb LANGUAGE plpgsql AS $$
BEGIN
    UPDATE public.lineas_investigacion SET activo=p_activo,deleted_at=CASE WHEN p_activo THEN NULL ELSE CURRENT_TIMESTAMP END WHERE id=p_id;
    IF NOT FOUND THEN RETURN NULL; END IF;
    RETURN (SELECT to_jsonb(l) FROM public.v2_lineas_investigacion l WHERE id=p_id);
END; $$;
