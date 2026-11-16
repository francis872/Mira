-- Cinco operaciones por catalogo. Funciones invocadas con SELECT, no procedimientos CALL.
-- No se alteran tablas, filas existentes ni las rutinas legadas de la migracion 003.

CREATE OR REPLACE FUNCTION public.sp_listar_area_conocimiento()
RETURNS SETOF public.area_conocimiento LANGUAGE sql STABLE AS $$
    SELECT * FROM public.area_conocimiento WHERE activo = TRUE ORDER BY id;
$$;

CREATE OR REPLACE FUNCTION public.sp_consultar_area_conocimiento(p_id integer)
RETURNS SETOF public.area_conocimiento LANGUAGE sql STABLE AS $$
    SELECT * FROM public.area_conocimiento WHERE id = p_id AND activo = TRUE;
$$;

CREATE OR REPLACE FUNCTION public.sp_crear_area_conocimiento(p_gran_area text, p_area text, p_disciplina text)
RETURNS SETOF public.area_conocimiento LANGUAGE sql VOLATILE AS $$
    INSERT INTO public.area_conocimiento (gran_area, area, disciplina, activo)
    VALUES (p_gran_area, p_area, p_disciplina, TRUE)
    RETURNING *;
$$;

CREATE OR REPLACE FUNCTION public.sp_actualizar_area_conocimiento(p_id integer, p_gran_area text, p_area text, p_disciplina text)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.area_conocimiento SET gran_area = p_gran_area, area = p_area, disciplina = p_disciplina
        WHERE id = p_id AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;

CREATE OR REPLACE FUNCTION public.sp_eliminar_area_conocimiento(p_id integer)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.area_conocimiento SET activo = FALSE
        WHERE id = p_id AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;

CREATE OR REPLACE FUNCTION public.sp_listar_objetivo_desarrollo_sostenible()
RETURNS SETOF public.objetivo_desarrollo_sostenible LANGUAGE sql STABLE AS $$
    SELECT * FROM public.objetivo_desarrollo_sostenible WHERE activo = TRUE ORDER BY id;
$$;

CREATE OR REPLACE FUNCTION public.sp_consultar_objetivo_desarrollo_sostenible(p_id integer)
RETURNS SETOF public.objetivo_desarrollo_sostenible LANGUAGE sql STABLE AS $$
    SELECT * FROM public.objetivo_desarrollo_sostenible WHERE id = p_id AND activo = TRUE;
$$;

CREATE OR REPLACE FUNCTION public.sp_crear_objetivo_desarrollo_sostenible(p_nombre text, p_categoria text)
RETURNS SETOF public.objetivo_desarrollo_sostenible LANGUAGE sql VOLATILE AS $$
    INSERT INTO public.objetivo_desarrollo_sostenible (nombre, categoria, activo)
    VALUES (p_nombre, p_categoria, TRUE)
    RETURNING *;
$$;

CREATE OR REPLACE FUNCTION public.sp_actualizar_objetivo_desarrollo_sostenible(p_id integer, p_nombre text, p_categoria text)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.objetivo_desarrollo_sostenible SET nombre = p_nombre, categoria = p_categoria
        WHERE id = p_id AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;

CREATE OR REPLACE FUNCTION public.sp_eliminar_objetivo_desarrollo_sostenible(p_id integer)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.objetivo_desarrollo_sostenible SET activo = FALSE
        WHERE id = p_id AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;

CREATE OR REPLACE FUNCTION public.sp_listar_area_aplicacion()
RETURNS SETOF public.area_aplicacion LANGUAGE sql STABLE AS $$
    SELECT * FROM public.area_aplicacion WHERE activo = TRUE ORDER BY id;
$$;

CREATE OR REPLACE FUNCTION public.sp_consultar_area_aplicacion(p_id integer)
RETURNS SETOF public.area_aplicacion LANGUAGE sql STABLE AS $$
    SELECT * FROM public.area_aplicacion WHERE id = p_id AND activo = TRUE;
$$;

CREATE OR REPLACE FUNCTION public.sp_crear_area_aplicacion(p_nombre text)
RETURNS SETOF public.area_aplicacion LANGUAGE sql VOLATILE AS $$
    INSERT INTO public.area_aplicacion (nombre, activo)
    VALUES (p_nombre, TRUE)
    RETURNING *;
$$;

CREATE OR REPLACE FUNCTION public.sp_actualizar_area_aplicacion(p_id integer, p_nombre text)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.area_aplicacion SET nombre = p_nombre
        WHERE id = p_id AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;

CREATE OR REPLACE FUNCTION public.sp_eliminar_area_aplicacion(p_id integer)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.area_aplicacion SET activo = FALSE
        WHERE id = p_id AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;

CREATE OR REPLACE FUNCTION public.sp_listar_termino_clave()
RETURNS SETOF public.termino_clave LANGUAGE sql STABLE AS $$
    SELECT * FROM public.termino_clave WHERE activo = TRUE ORDER BY termino;
$$;

CREATE OR REPLACE FUNCTION public.sp_consultar_termino_clave(p_termino text)
RETURNS SETOF public.termino_clave LANGUAGE sql STABLE AS $$
    SELECT * FROM public.termino_clave WHERE termino = p_termino AND activo = TRUE;
$$;

CREATE OR REPLACE FUNCTION public.sp_crear_termino_clave(p_termino text, p_termino_ingles text)
RETURNS SETOF public.termino_clave LANGUAGE sql VOLATILE AS $$
    INSERT INTO public.termino_clave (termino, termino_ingles, activo)
    VALUES (p_termino, p_termino_ingles, TRUE)
    RETURNING *;
$$;

CREATE OR REPLACE FUNCTION public.sp_actualizar_termino_clave(p_termino text, p_termino_ingles text)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.termino_clave SET termino_ingles = p_termino_ingles
        WHERE termino = p_termino AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;

CREATE OR REPLACE FUNCTION public.sp_eliminar_termino_clave(p_termino text)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.termino_clave SET activo = FALSE
        WHERE termino = p_termino AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;

CREATE OR REPLACE FUNCTION public.sp_listar_universidad()
RETURNS SETOF public.universidad LANGUAGE sql STABLE AS $$
    SELECT * FROM public.universidad WHERE activo = TRUE ORDER BY id;
$$;

CREATE OR REPLACE FUNCTION public.sp_consultar_universidad(p_id integer)
RETURNS SETOF public.universidad LANGUAGE sql STABLE AS $$
    SELECT * FROM public.universidad WHERE id = p_id AND activo = TRUE;
$$;

CREATE OR REPLACE FUNCTION public.sp_crear_universidad(p_nombre text, p_tipo text, p_ciudad text)
RETURNS SETOF public.universidad LANGUAGE sql VOLATILE AS $$
    INSERT INTO public.universidad (nombre, tipo, ciudad, activo)
    VALUES (p_nombre, p_tipo, p_ciudad, TRUE)
    RETURNING *;
$$;

CREATE OR REPLACE FUNCTION public.sp_actualizar_universidad(p_id integer, p_nombre text, p_tipo text, p_ciudad text)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.universidad SET nombre = p_nombre, tipo = p_tipo, ciudad = p_ciudad
        WHERE id = p_id AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;

CREATE OR REPLACE FUNCTION public.sp_eliminar_universidad(p_id integer)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.universidad SET activo = FALSE
        WHERE id = p_id AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;

CREATE OR REPLACE FUNCTION public.sp_listar_linea_investigacion()
RETURNS SETOF public.linea_investigacion LANGUAGE sql STABLE AS $$
    SELECT * FROM public.linea_investigacion WHERE activo = TRUE ORDER BY id;
$$;

CREATE OR REPLACE FUNCTION public.sp_consultar_linea_investigacion(p_id integer)
RETURNS SETOF public.linea_investigacion LANGUAGE sql STABLE AS $$
    SELECT * FROM public.linea_investigacion WHERE id = p_id AND activo = TRUE;
$$;

CREATE OR REPLACE FUNCTION public.sp_crear_linea_investigacion(p_nombre text, p_descripcion text)
RETURNS SETOF public.linea_investigacion LANGUAGE sql VOLATILE AS $$
    INSERT INTO public.linea_investigacion (nombre, descripcion, activo)
    VALUES (p_nombre, p_descripcion, TRUE)
    RETURNING *;
$$;

CREATE OR REPLACE FUNCTION public.sp_actualizar_linea_investigacion(p_id integer, p_nombre text, p_descripcion text)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.linea_investigacion SET nombre = p_nombre, descripcion = p_descripcion
        WHERE id = p_id AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;

CREATE OR REPLACE FUNCTION public.sp_eliminar_linea_investigacion(p_id integer)
RETURNS boolean LANGUAGE sql VOLATILE AS $$
    WITH changed AS (
        UPDATE public.linea_investigacion SET activo = FALSE
        WHERE id = p_id AND activo = TRUE RETURNING 1
    ) SELECT EXISTS (SELECT 1 FROM changed);
$$;
