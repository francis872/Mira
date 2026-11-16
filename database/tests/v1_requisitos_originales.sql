-- Base exclusiva de pruebas; verifica persistencia sin depender de controles HTML/API.
BEGIN;
DO $$
DECLARE c text;
BEGIN
    FOREACH c IN ARRAY ARRAY['Social','Económica','Ambiental'] LOOP
        PERFORM public.sp_crear_objetivo_desarrollo_sostenible('fase5-ods',c);
    END LOOP;
    BEGIN
        PERFORM public.sp_crear_objetivo_desarrollo_sostenible('fase5-invalido','Económico');
        RAISE EXCEPTION 'Se aceptó una categoría fuera de la especificación';
    EXCEPTION WHEN check_violation THEN NULL;
    END;
    BEGIN
        PERFORM public.sp_crear_termino_clave('fase5-sin-ingles',NULL);
        RAISE EXCEPTION 'Se aceptó un término sin inglés';
    EXCEPTION WHEN check_violation THEN NULL;
    END;
    BEGIN
        PERFORM public.sp_crear_termino_clave('fase5-ingles-vacio','   ');
        RAISE EXCEPTION 'Se aceptó un término con inglés vacío';
    EXCEPTION WHEN check_violation THEN NULL;
    END;
    PERFORM public.sp_crear_termino_clave('fase5-bilingue','bilingual');
    BEGIN
        PERFORM public.sp_actualizar_termino_clave('fase5-bilingue','');
        RAISE EXCEPTION 'Se quitó el inglés al editar';
    EXCEPTION WHEN check_violation THEN NULL;
    END;
    IF (SELECT termino_ingles FROM public.termino_clave WHERE termino='fase5-bilingue')<>'bilingual' THEN
        RAISE EXCEPTION 'Se alteró el término después del fallo';
    END IF;
END;
$$;
ROLLBACK;
