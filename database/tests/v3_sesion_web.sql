\set ON_ERROR_STOP on
BEGIN;
DO $$
BEGIN
 IF EXISTS (SELECT 1 FROM pg_namespace n CROSS JOIN LATERAL aclexplode(coalesce(n.nspacl,acldefault('n',n.nspowner))) a
            WHERE n.nspname='mira_web' AND a.grantee=0 AND a.privilege_type IN ('USAGE','CREATE')) THEN
  RAISE EXCEPTION 'El esquema de sesiones no debe conceder CREATE a PUBLIC';
 END IF;
 BEGIN
  INSERT INTO mira_web.sesion VALUES('invalid',decode('00','hex'),decode(repeat('00',32),'hex'),now());
  RAISE EXCEPTION 'Se admitió clave de sesión inválida';
 EXCEPTION WHEN check_violation THEN NULL; END;
 BEGIN
  INSERT INTO mira_web.sesion VALUES(repeat('a',64),decode('00','hex'),decode('00','hex'),now());
  RAISE EXCEPTION 'Se admitió firma inválida';
 EXCEPTION WHEN check_violation THEN NULL; END;
END $$;
ROLLBACK;
