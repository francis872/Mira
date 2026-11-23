-- HU02-CA02/CA03 del DOCX original: categorías ODS cerradas y equivalente inglés obligatorio.
-- Ejecutar en una transacción (apply.sh y los verificadores ya lo hacen).
-- Preflight: no inventar traducciones ni recategorizar valores de significado desconocido.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM public.termino_clave WHERE termino_ingles IS NULL OR btrim(termino_ingles)='') THEN
        RAISE EXCEPTION '009: complete el equivalente inglés de los términos existentes antes de migrar. No se modificaron datos.' USING ERRCODE='22023';
    END IF;
    IF EXISTS (SELECT 1 FROM public.objetivo_desarrollo_sostenible
               WHERE categoria NOT IN ('Social','Económica','Ambiental','Económico')) THEN
        RAISE EXCEPTION '009: revise las categorías ODS existentes; solo Social, Económica y Ambiental están permitidas. No se modificaron datos.' USING ERRCODE='22023';
    END IF;
END;
$$;

-- Corrección ortográfica de la categoría económica sembrada por 002; no cambia su clasificación.
UPDATE public.objetivo_desarrollo_sostenible SET categoria='Económica' WHERE categoria='Económico';

ALTER TABLE public.termino_clave ADD CONSTRAINT termino_clave_ingles_obligatorio_ck
    CHECK (termino_ingles IS NOT NULL AND btrim(termino_ingles)<>'');
ALTER TABLE public.objetivo_desarrollo_sostenible ADD CONSTRAINT ods_categoria_original_ck
    CHECK (categoria IN ('Social','Económica','Ambiental'));
