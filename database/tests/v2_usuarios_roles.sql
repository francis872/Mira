-- Ejecutar en una transacción de prueba; no necesita credenciales reales.
BEGIN;
DO $$
DECLARE
 v JSONB;
 v_id BIGINT;
 v_admin_role INTEGER;
 v_investigator_role INTEGER;
 v_coordinator_role INTEGER;
BEGIN
 SELECT id INTO STRICT v_admin_role FROM rol WHERE nombre='Administrador' AND activo;
 SELECT id INTO STRICT v_investigator_role FROM rol WHERE nombre='Investigador' AND activo;
 SELECT id INTO STRICT v_coordinator_role FROM rol WHERE nombre='Coordinador' AND activo;

 v:=fn_usuario_crear('v2-test@example.invalid',repeat('x',60),jsonb_build_array(v_admin_role));
 v_id:=(v->>'id')::bigint;
 IF (v->'roles'->0->>'nombre') IS DISTINCT FROM 'Administrador' THEN
  RAISE EXCEPTION 'No se insertaron los detalles';
 END IF;
 v:=fn_usuario_actualizar(v_id,'v2-cambio@example.invalid',jsonb_build_array(v_investigator_role,v_coordinator_role));
 IF jsonb_array_length(v->'roles')<>2 THEN RAISE EXCEPTION 'Update detalle fallido'; END IF;
 IF fn_usuario_consultar(v_id) IS NULL THEN RAISE EXCEPTION 'Consulta fallida'; END IF;
 IF NOT fn_usuario_inactivar(v_id) THEN RAISE EXCEPTION 'Inactivacion fallida'; END IF;

 BEGIN
  PERFORM fn_usuario_crear('v2-invalid-role@example.invalid',repeat('x',60),'[2147483647]'::jsonb);
  RAISE EXCEPTION 'Se aceptó una referencia a un rol inexistente';
 EXCEPTION WHEN SQLSTATE '23503' THEN NULL;
 END;
 IF EXISTS (SELECT 1 FROM usuario WHERE correo='v2-invalid-role@example.invalid') THEN
  RAISE EXCEPTION 'La operación inválida dejó persistido el maestro';
 END IF;

 BEGIN
  INSERT INTO usuario_rol(usuario_id,rol_id) VALUES (9223372036854770000,v_admin_role);
  RAISE EXCEPTION 'Se aceptó una FK de usuario inexistente';
 EXCEPTION WHEN foreign_key_violation THEN NULL;
 END;

 v:=fn_usuario_bootstrap_admin('v2-bootstrap@example.invalid',repeat('x',60));
 IF (v->'roles'->0->>'nombre') IS DISTINCT FROM 'Administrador' THEN
  RAISE EXCEPTION 'Bootstrap no asignó el rol Administrador';
 END IF;
 BEGIN
  PERFORM fn_usuario_bootstrap_admin('v2-bootstrap-second@example.invalid',repeat('x',60));
  RAISE EXCEPTION 'El bootstrap permitió más de un primer administrador';
 EXCEPTION WHEN unique_violation THEN NULL;
 END;
END $$;
ROLLBACK;
