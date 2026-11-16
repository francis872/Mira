CREATE TABLE IF NOT EXISTS usuarios (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(120) NOT NULL,
    correo VARCHAR(160) NOT NULL UNIQUE,
    password_hash VARCHAR(200) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS roles (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS usuario_roles (
    usuario_id INT NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    rol_id INT NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    PRIMARY KEY (usuario_id, rol_id)
);

CREATE TABLE IF NOT EXISTS sedes (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(120) NOT NULL,
    ciudad VARCHAR(120) NOT NULL,
    activa BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS areas_conocimiento (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(160) NOT NULL UNIQUE,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS ods (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(160) NOT NULL UNIQUE,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS areas_aplicacion (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(160) NOT NULL UNIQUE,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS palabras_clave (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(160) NOT NULL UNIQUE,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

INSERT INTO sedes (nombre, ciudad, activa)
VALUES ('Sede Medellin', 'Medellin', TRUE)
ON CONFLICT DO NOTHING;

INSERT INTO areas_conocimiento (nombre) VALUES
('Ingenieria de Software'),
('Ciencias Sociales')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO ods (nombre) VALUES
('ODS 4 - Educacion de Calidad'),
('ODS 9 - Industria, Innovacion e Infraestructura')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO areas_aplicacion (nombre) VALUES
('Salud'),
('Educacion')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO palabras_clave (nombre) VALUES
('Investigacion Aplicada'),
('Innovacion')
ON CONFLICT (nombre) DO NOTHING;

CREATE TABLE IF NOT EXISTS docentes (
    id SERIAL PRIMARY KEY,
    usuario_id INTEGER UNIQUE NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    cvlac_url VARCHAR(500),
    escalafon VARCHAR(100),
    minciencias_categoria VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT docentes_minciencias_categoria_check CHECK (
        minciencias_categoria IS NULL OR minciencias_categoria IN ('Senior', 'Asociado', 'Junior', 'Sin Categoría')
    ),
    CONSTRAINT docentes_cvlac_url_check CHECK (
        cvlac_url IS NULL OR cvlac_url ~ '^https?://'
    )
);

CREATE TABLE IF NOT EXISTS grupos_investigacion (
    id SERIAL PRIMARY KEY,
    sede_id INTEGER NOT NULL REFERENCES sedes(id),
    gruptac_url VARCHAR(500) NOT NULL,
    categoria VARCHAR(100),
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    deleted_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT grupos_gruptac_url_check CHECK (gruptac_url ~ '^https?://')
);

CREATE TABLE IF NOT EXISTS grupo_docentes (
    id SERIAL PRIMARY KEY,
    grupo_id INTEGER NOT NULL REFERENCES grupos_investigacion(id) ON DELETE CASCADE,
    docente_id INTEGER NOT NULL REFERENCES docentes(id) ON DELETE CASCADE,
    rol VARCHAR(100) NOT NULL CHECK (rol IN ('Líder de Grupo', 'Coinvestigador', 'Colaborador')),
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (grupo_id, docente_id, rol, fecha_inicio, fecha_fin),
    CONSTRAINT grupo_docentes_period_check CHECK (fecha_fin IS NULL OR fecha_fin > fecha_inicio)
);

CREATE UNIQUE INDEX IF NOT EXISTS grupo_docentes_un_lider_activo_idx
    ON grupo_docentes (grupo_id)
    WHERE rol = 'Líder de Grupo' AND activo = TRUE AND fecha_fin IS NULL;

CREATE TABLE IF NOT EXISTS semilleros (
    id SERIAL PRIMARY KEY,
    grupo_id INTEGER NOT NULL REFERENCES grupos_investigacion(id),
    nombre VARCHAR(200) NOT NULL,
    descripcion TEXT,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    deleted_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE OR REPLACE FUNCTION mira_validar_grupo_activo_semillero()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM grupos_investigacion
        WHERE id = NEW.grupo_id AND activo = TRUE AND deleted_at IS NULL
    ) THEN
        RAISE EXCEPTION 'El semillero debe asociarse a un grupo de investigación activo';
    END IF;
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS semilleros_grupo_activo_trigger ON semilleros;
CREATE TRIGGER semilleros_grupo_activo_trigger
BEFORE INSERT OR UPDATE OF grupo_id ON semilleros
FOR EACH ROW
EXECUTE FUNCTION mira_validar_grupo_activo_semillero();

CREATE TABLE IF NOT EXISTS semillero_tutores (
    id SERIAL PRIMARY KEY,
    semillero_id INTEGER NOT NULL REFERENCES semilleros(id) ON DELETE CASCADE,
    docente_id INTEGER NOT NULL REFERENCES docentes(id) ON DELETE CASCADE,
    rol VARCHAR(100) NOT NULL CHECK (rol IN ('Director', 'Tutor', 'Coinvestigador')),
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT semillero_tutores_period_check CHECK (fecha_fin IS NULL OR fecha_fin > fecha_inicio)
);

CREATE UNIQUE INDEX IF NOT EXISTS semillero_tutores_persona_rol_periodo_idx
    ON semillero_tutores (
        semillero_id,
        docente_id,
        rol,
        fecha_inicio,
        COALESCE(fecha_fin, DATE '9999-12-31')
    );

CREATE TABLE IF NOT EXISTS lineas_investigacion (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(50) UNIQUE NOT NULL,
    nombre VARCHAR(200) NOT NULL,
    descripcion TEXT,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    deleted_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS grupo_lineas (
    grupo_id INTEGER NOT NULL REFERENCES grupos_investigacion(id) ON DELETE CASCADE,
    linea_id INTEGER NOT NULL REFERENCES lineas_investigacion(id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (grupo_id, linea_id)
);

CREATE TABLE IF NOT EXISTS semillero_lineas (
    semillero_id INTEGER NOT NULL REFERENCES semilleros(id) ON DELETE CASCADE,
    linea_id INTEGER NOT NULL REFERENCES lineas_investigacion(id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (semillero_id, linea_id)
);

CREATE TABLE IF NOT EXISTS linea_disciplinas (
    linea_id INTEGER NOT NULL REFERENCES lineas_investigacion(id) ON DELETE CASCADE,
    disciplina_id INTEGER NOT NULL REFERENCES areas_conocimiento(id),
    PRIMARY KEY (linea_id, disciplina_id)
);

CREATE TABLE IF NOT EXISTS linea_ods (
    linea_id INTEGER NOT NULL REFERENCES lineas_investigacion(id) ON DELETE CASCADE,
    ods_id INTEGER NOT NULL REFERENCES ods(id),
    PRIMARY KEY (linea_id, ods_id)
);

CREATE TABLE IF NOT EXISTS linea_aplicacion (
    linea_id INTEGER NOT NULL REFERENCES lineas_investigacion(id) ON DELETE CASCADE,
    aplicacion_id INTEGER NOT NULL REFERENCES areas_aplicacion(id),
    PRIMARY KEY (linea_id, aplicacion_id)
);

CREATE INDEX IF NOT EXISTS idx_docentes_usuario_id ON docentes(usuario_id);
CREATE INDEX IF NOT EXISTS idx_grupos_sede_id ON grupos_investigacion(sede_id);
CREATE INDEX IF NOT EXISTS idx_grupo_docentes_grupo_id ON grupo_docentes(grupo_id);
CREATE INDEX IF NOT EXISTS idx_grupo_docentes_docente_id ON grupo_docentes(docente_id);
CREATE INDEX IF NOT EXISTS idx_semilleros_grupo_id ON semilleros(grupo_id);
CREATE INDEX IF NOT EXISTS idx_semillero_tutores_semillero_id ON semillero_tutores(semillero_id);
CREATE INDEX IF NOT EXISTS idx_linea_disciplinas_linea_id ON linea_disciplinas(linea_id);
CREATE INDEX IF NOT EXISTS idx_linea_ods_linea_id ON linea_ods(linea_id);
CREATE INDEX IF NOT EXISTS idx_linea_aplicacion_aplicacion_id ON linea_aplicacion(aplicacion_id);
