-- Aditiva: no modifica personas, credenciales ni rutinas V1/V2.
CREATE TABLE sesion_acceso (
    jti TEXT PRIMARY KEY,
    usuario_id BIGINT NOT NULL REFERENCES usuario(id),
    expira_en TIMESTAMPTZ NOT NULL
);
CREATE INDEX ix_sesion_acceso_expiracion ON sesion_acceso(expira_en);
CREATE INDEX ix_sesion_acceso_usuario ON sesion_acceso(usuario_id);

CREATE PROCEDURE sp_v3_usuario_actualizar(p_id BIGINT,p_correo TEXT,p_roles JSONB,INOUT p_resultado JSONB DEFAULT NULL)
LANGUAGE plpgsql AS $$
BEGIN
    CALL sp_usuario_actualizar(p_id,p_correo,p_roles,p_resultado);
    DELETE FROM sesion_acceso WHERE usuario_id=p_id;
END $$;

CREATE PROCEDURE sp_v3_usuario_password(p_id BIGINT,p_hash TEXT)
LANGUAGE plpgsql AS $$
BEGIN
    IF p_hash IS NULL OR p_hash !~ '^\$2[aby]\$12\$[./A-Za-z0-9]{53}$' THEN
        RAISE EXCEPTION 'Hash no valido' USING ERRCODE='22023';
    END IF;
    UPDATE usuario SET password_hash=p_hash WHERE id=p_id AND activo;
    IF NOT FOUND THEN RAISE EXCEPTION 'Cuenta inexistente o inactiva' USING ERRCODE='P0002'; END IF;
    DELETE FROM sesion_acceso WHERE usuario_id=p_id;
END $$;
