-- Solo en base exclusiva de pruebas. Las escrituras y cambios de trigger se revierten.
BEGIN;
DO $$
DECLARE
    total integer;
    row_data public.area_aplicacion;
BEGIN
    SELECT count(*) INTO total FROM pg_proc p JOIN pg_namespace n ON n.oid = p.pronamespace
    WHERE n.nspname = 'public' AND p.proname ~ '^sp_(listar|consultar|crear|actualizar|eliminar)_(area_conocimiento|objetivo_desarrollo_sostenible|area_aplicacion|termino_clave|universidad|linea_investigacion)$';
    IF total <> 30 THEN RAISE EXCEPTION 'Se esperaban 30 funciones CRUD, hay %', total; END IF;

    SELECT * INTO STRICT row_data FROM public.sp_crear_area_aplicacion(E' \ttrigger-prueba\n ');
    IF row_data.nombre <> 'trigger-prueba' THEN RAISE EXCEPTION 'El trigger no normalizo el INSERT'; END IF;
    PERFORM public.sp_actualizar_area_aplicacion(row_data.id, '  trigger-editado  ');
    SELECT * INTO STRICT row_data FROM public.sp_consultar_area_aplicacion(row_data.id);
    IF row_data.nombre <> 'trigger-editado' THEN RAISE EXCEPTION 'El trigger no normalizo el UPDATE'; END IF;
    IF NOT public.sp_eliminar_area_aplicacion(row_data.id) THEN RAISE EXCEPTION 'No se inactivo'; END IF;
    IF EXISTS (SELECT 1 FROM public.sp_consultar_area_aplicacion(row_data.id)) THEN RAISE EXCEPTION 'Se devolvio un inactivo'; END IF;

    ALTER TABLE public.area_aplicacion DISABLE TRIGGER tr_area_aplicacion_normalizar;
    SELECT * INTO STRICT row_data FROM public.sp_crear_area_aplicacion('  sin-trigger  ');
    IF row_data.nombre <> '  sin-trigger  ' THEN RAISE EXCEPTION 'La normalizacion no pertenece exclusivamente al trigger'; END IF;
    ALTER TABLE public.area_aplicacion ENABLE TRIGGER tr_area_aplicacion_normalizar;

    BEGIN
        PERFORM public.sp_crear_area_aplicacion('   ');
        RAISE EXCEPTION 'Se acepto un nombre vacio';
    EXCEPTION WHEN SQLSTATE '22023' THEN NULL;
    END;
END $$;
ROLLBACK;
