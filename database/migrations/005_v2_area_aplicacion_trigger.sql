-- Centraliza la normalizacion de espacios extremos ya realizada por la aplicacion.
-- No modifica columnas ni normaliza retroactivamente datos existentes.
CREATE OR REPLACE FUNCTION public.fn_area_aplicacion_normalizar()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    NEW.nombre := regexp_replace(NEW.nombre, '^[[:space:]]+|[[:space:]]+$', '', 'g');
    IF NEW.nombre IS NULL OR NEW.nombre = '' THEN
        RAISE EXCEPTION 'El nombre del area de aplicacion es obligatorio.' USING ERRCODE = '22023';
    END IF;
    RETURN NEW;
END;
$$;

CREATE OR REPLACE TRIGGER tr_area_aplicacion_normalizar
BEFORE INSERT OR UPDATE OF nombre ON public.area_aplicacion
FOR EACH ROW EXECUTE FUNCTION public.fn_area_aplicacion_normalizar();
