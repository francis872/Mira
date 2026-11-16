-- HU05-CA04 y HU10: correspondencias de las relaciones existentes, sin métricas ni Facultad inventada.
CREATE OR REPLACE FUNCTION public.sp_opciones_v2_reportes() RETURNS jsonb LANGUAGE sql STABLE AS $$
SELECT jsonb_build_object(
    'sedes',(SELECT coalesce(jsonb_agg(jsonb_build_object('id',id,'nombre',nombre,'activo',activa) ORDER BY nombre,id),'[]'::jsonb) FROM public.sedes),
    'lineas',(SELECT coalesce(jsonb_agg(jsonb_build_object('id',id,'nombre',codigo||' · '||nombre,'activo',activo) ORDER BY codigo,id),'[]'::jsonb) FROM public.lineas_investigacion),
    'grupos',(SELECT coalesce(jsonb_agg(jsonb_build_object('id',id,'nombre',etiqueta,'activo',activo) ORDER BY id),'[]'::jsonb) FROM public.v2_grupos_investigacion));
$$;

CREATE OR REPLACE FUNCTION public.sp_reporte_v2_integrantes(p_grupo integer) RETURNS jsonb LANGUAGE sql STABLE AS $$
SELECT jsonb_build_object('grupo',jsonb_build_object('id',g.id,'nombre',g.etiqueta),'integrantes',
    (SELECT coalesce(jsonb_agg(jsonb_build_object('docente_id',d.id,'docente',u.nombre,'rol',m.rol,
        'fecha_inicio',m.fecha_inicio,'fecha_fin',m.fecha_fin,'activo',m.activo) ORDER BY u.nombre,m.fecha_inicio,m.id),'[]'::jsonb)
     FROM public.grupo_docentes m JOIN public.docentes d ON d.id=m.docente_id JOIN public.usuarios u ON u.id=d.usuario_id WHERE m.grupo_id=g.id))
FROM public.v2_grupos_investigacion g WHERE g.id=p_grupo;
$$;

CREATE OR REPLACE FUNCTION public.sp_reporte_v2_articulacion(p_sede integer,p_linea integer) RETURNS jsonb LANGUAGE sql STABLE AS $$
WITH organizaciones AS (
    SELECT g.id AS grupo_id,NULL::integer AS semillero_id,g.sede_id,g.activo AS grupo_activo,NULL::boolean AS semillero_activo,NULL::text AS semillero
    FROM public.grupos_investigacion g
    UNION ALL
    SELECT g.id,s.id,g.sede_id,g.activo,s.activo,s.nombre FROM public.semilleros s JOIN public.grupos_investigacion g ON g.id=s.grupo_id
), articulaciones AS (
    SELECT a.grupo_id,NULL::integer AS semillero_id,a.linea_id FROM public.grupo_lineas a
    UNION ALL
    SELECT s.grupo_id,a.semillero_id,a.linea_id FROM public.semillero_lineas a JOIN public.semilleros s ON s.id=a.semillero_id
), integrantes AS (
    SELECT m.grupo_id,NULL::integer AS semillero_id,m.docente_id,m.rol,m.fecha_inicio,m.fecha_fin,m.activo FROM public.grupo_docentes m
    UNION ALL
    SELECT s.grupo_id,m.semillero_id,m.docente_id,m.rol,m.fecha_inicio,m.fecha_fin,TRUE FROM public.semillero_tutores m JOIN public.semilleros s ON s.id=m.semillero_id
), filas AS (
    SELECT o.grupo_id,o.semillero_id,se.nombre AS sede,g.etiqueta AS grupo,o.semillero,
        u.nombre AS docente,i.rol,l.codigo||' · '||l.nombre AS linea,od.nombre AS ods,
        i.fecha_inicio,i.fecha_fin,o.grupo_activo,o.semillero_activo,l.activo AS linea_activa,i.activo AS vinculo_activo,
        i.docente_id,l.id AS linea_id,od.id AS ods_id
    FROM organizaciones o JOIN public.sedes se ON se.id=o.sede_id JOIN public.v2_grupos_investigacion g ON g.id=o.grupo_id
    LEFT JOIN integrantes i ON i.grupo_id=o.grupo_id AND i.semillero_id IS NOT DISTINCT FROM o.semillero_id
    LEFT JOIN public.docentes d ON d.id=i.docente_id LEFT JOIN public.usuarios u ON u.id=d.usuario_id
    LEFT JOIN articulaciones a ON a.grupo_id=o.grupo_id AND a.semillero_id IS NOT DISTINCT FROM o.semillero_id
    LEFT JOIN public.lineas_investigacion l ON l.id=a.linea_id LEFT JOIN public.linea_ods lo ON lo.linea_id=l.id LEFT JOIN public.ods od ON od.id=lo.ods_id
    WHERE (p_sede IS NULL OR o.sede_id=p_sede) AND (p_linea IS NULL OR l.id=p_linea)
)
SELECT coalesce(jsonb_agg(to_jsonb(f) ORDER BY f.grupo_id,f.semillero_id NULLS FIRST,f.docente_id,f.fecha_inicio,f.linea_id,f.ods_id),'[]'::jsonb) FROM filas f;
$$;
