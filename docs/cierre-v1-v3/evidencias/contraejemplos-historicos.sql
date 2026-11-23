BEGIN;
CREATE TEMP TABLE audit_findings (id text, observed jsonb);
DO $$
DECLARE s int;g int;p int;d int;t int;l int; v jsonb;
BEGIN
 INSERT INTO termino_clave(termino,termino_ingles) VALUES ('audit-historical-keyword',NULL);
 INSERT INTO objetivo_desarrollo_sostenible(nombre,categoria) VALUES ('ODS ficticio','categoria fuera del dominio');
 INSERT INTO audit_findings VALUES ('HU02-CA02',jsonb_build_object('arbitraryOdsCategoryAccepted',true));
 INSERT INTO audit_findings VALUES ('HU02-CA03',jsonb_build_object('keywordEnglishNullAccepted',true));
 v:=sp_crear_v2_sedes('{"nombre":"Sede de prueba historica","ciudad":"Prueba"}'::jsonb);s:=(v->>'id')::int;
 PERFORM sp_eliminar_v2_sedes(s);
 v:=sp_crear_v2_grupos_investigacion(jsonb_build_object('sede_id',s,'gruptac_url','https://example.invalid/grupo','categoria',NULL));g:=(v->>'id')::int;
 INSERT INTO audit_findings VALUES ('HU04-CA03',jsonb_build_object('groupAssignedToInactiveSede',g IS NOT NULL));
 INSERT INTO audit_findings VALUES ('HU05-CA02-zero',jsonb_build_object('groupCreatedWithoutLeader',NOT EXISTS(SELECT 1 FROM grupo_docentes WHERE grupo_id=g)));
 INSERT INTO usuarios(nombre,correo,password_hash) VALUES ('Persona ficticia','historial-prueba@example.invalid','fixture-not-access-account') RETURNING id INTO p;
 INSERT INTO docentes(usuario_id) VALUES(p) RETURNING id INTO d;
 INSERT INTO grupo_docentes(grupo_id,docente_id,rol,fecha_inicio,fecha_fin) VALUES(g,d,'Líder de Grupo','2026-01-01','2026-12-31'),(g,d,'Líder de Grupo','2026-02-01','2026-11-30');
 INSERT INTO audit_findings VALUES ('HU05-CA02-overlap',jsonb_build_object('overlappingDatedLeadersAccepted',true));
 v:=sp_crear_v2_semilleros(jsonb_build_object('grupo_id',g,'nombre','Semillero ficticio','descripcion',NULL));t:=(v->>'id')::int;
 INSERT INTO semillero_tutores(semillero_id,docente_id,rol,fecha_inicio,fecha_fin) VALUES(t,d,'Tutor','2026-01-01','2026-12-31'),(t,d,'Tutor','2026-02-01','2026-11-30');
 INSERT INTO audit_findings VALUES ('HU06-CA03-ambiguity',jsonb_build_object('overlappingTutorIntervalsAccepted',true));
 BEGIN
 INSERT INTO semillero_tutores(semillero_id,docente_id,rol,fecha_inicio,fecha_fin) VALUES(t,d,'Tutor','2026-01-01','2026-12-31');
 INSERT INTO audit_findings VALUES ('HU06-CA03-exact',jsonb_build_object('exactDuplicateRejected',false));
 EXCEPTION WHEN unique_violation THEN
 INSERT INTO audit_findings VALUES ('HU06-CA03-exact',jsonb_build_object('exactDuplicateRejected',true));
 END;
 v:=sp_crear_v2_lineas_investigacion('{"codigo":"AUDIT-HIST","nombre":"Linea ficticia","descripcion":null}'::jsonb);l:=(v->>'id')::int;
 INSERT INTO audit_findings VALUES ('HU07-CA01',jsonb_build_object('lineDescriptionNullAccepted',l IS NOT NULL));
 INSERT INTO audit_findings VALUES ('HU09-CA04',jsonb_build_object('lineCreatedWithoutOdsOrDiscipline',NOT EXISTS(SELECT 1 FROM linea_ods WHERE linea_id=l) AND NOT EXISTS(SELECT 1 FROM linea_disciplinas WHERE linea_id=l)));
 PERFORM sp_eliminar_v2_lineas_investigacion(l);
 PERFORM sp_crear_v2_grupo_lineas(jsonb_build_object('grupo_id',g,'linea_id',l));
 INSERT INTO audit_findings VALUES ('HU07-CA03',jsonb_build_object('inactiveLineNewAssociationAccepted',true));
END;
$$;
SELECT jsonb_agg(to_jsonb(audit_findings) ORDER BY id) FROM audit_findings;
ROLLBACK;
