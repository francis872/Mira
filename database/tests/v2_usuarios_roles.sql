-- Ejecutar en una transacción de prueba; no necesita credenciales reales.
BEGIN;
DO $$
DECLARE v JSONB; id BIGINT;
BEGIN
 v:=fn_usuario_crear('v2-test@example.invalid',repeat('x',60),'[1]'::jsonb);
 id:=(v->>'id')::bigint;
 IF (v->'roles'->0->>'nombre') IS DISTINCT FROM 'Administrador' THEN
  RAISE EXCEPTION 'No se insertaron los detalles';
 END IF;
 v:=fn_usuario_actualizar(id,'v2-cambio@example.invalid','[2,3]'::jsonb);
 IF jsonb_array_length(v->'roles')<>2 THEN RAISE EXCEPTION 'Update detalle fallido'; END IF;
 IF fn_usuario_consultar(id) IS NULL THEN RAISE EXCEPTION 'Consulta fallida'; END IF;
 IF NOT fn_usuario_inactivar(id) THEN RAISE EXCEPTION 'Inactivacion fallida'; END IF;
END $$;
ROLLBACK;
