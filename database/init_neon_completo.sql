-- =============================================================================
-- MIRA - SCRIPT MAESTRO DE INICIALIZACIÓN COMPLETA PARA NEON (POSTGRESQL 16)
-- Versiones: v0.1 (Catálogos) + v0.2 (Relaciones) + v0.3 (Seguridad RBAC) + v0.4
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1. TABLAS CATÁLOGOS BASE (v0.1)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS area_conocimiento (
    id SERIAL PRIMARY KEY,
    gran_area VARCHAR(150) NOT NULL,
    area VARCHAR(150) NOT NULL,
    disciplina VARCHAR(150) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS objetivo_desarrollo_sostenible (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    categoria VARCHAR(100) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS area_aplicacion (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS termino_clave (
    termino VARCHAR(150) PRIMARY KEY,
    termino_ingles VARCHAR(150),
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS universidad (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    tipo VARCHAR(100) NOT NULL,
    ciudad VARCHAR(100) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS linea_investigacion (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    descripcion TEXT,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

-- -----------------------------------------------------------------------------
-- 2. DATOS SEMILLA CATÁLOGOS BASE (v0.1)
-- -----------------------------------------------------------------------------
INSERT INTO area_conocimiento (gran_area, area, disciplina, activo) VALUES
('Ingeniería y Tecnología', 'Ingeniería de Sistemas', 'Ingeniería de Software', TRUE),
('Ciencias Naturales', 'Ciencias de la Computación', 'Inteligencia Artificial', TRUE),
('Ciencias Sociales', 'Economía y Negocios', 'Gestión de Proyectos', TRUE)
ON CONFLICT DO NOTHING;

INSERT INTO objetivo_desarrollo_sostenible (nombre, categoria, activo) VALUES
('Educación de Calidad', 'Social', TRUE),
('Industria, Innovación e Infraestructura', 'Económico', TRUE),
('Acción por el Clima', 'Ambiental', TRUE)
ON CONFLICT DO NOTHING;

INSERT INTO area_aplicacion (nombre, activo) VALUES
('Salud y Telemedicina', TRUE),
('Educación Virtual', TRUE),
('Sector Financiero y Fintech', TRUE)
ON CONFLICT DO NOTHING;

INSERT INTO termino_clave (termino, termino_ingles, activo) VALUES
('Inteligencia Artificial', 'Artificial Intelligence', TRUE),
('Computación en la Nube', 'Cloud Computing', TRUE),
('Arquitectura de Software', 'Software Architecture', TRUE)
ON CONFLICT (termino) DO NOTHING;

INSERT INTO universidad (nombre, tipo, ciudad, activo) VALUES
('Universidad de San Buenaventura Medellín', 'Privada', 'Medellín', TRUE),
('Universidad de Antioquia', 'Pública', 'Medellín', TRUE),
('Universidad Nacional de Colombia', 'Pública', 'Bogotá', TRUE)
ON CONFLICT DO NOTHING;

INSERT INTO linea_investigacion (nombre, descripcion, activo) VALUES
('Ingeniería de Software y Sistemas Distribuidos', 'Investigación orientada a arquitecturas modernas, microservicios y calidad de software.', TRUE),
('Inteligencia Artificial y Analítica de Datos', 'Enfoque en modelos de aprendizaje automático y visión por computadora.', TRUE),
('Ciberseguridad y Redes', 'Investigación en seguridad informática, protocolos criptográficos y protección de infraestructuras.', TRUE)
ON CONFLICT DO NOTHING;

-- -----------------------------------------------------------------------------
-- 3. ENTIDADES RELACIONALES Y MAESTRO-DETALLE (v0.2)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS docente (
    cedula INTEGER PRIMARY KEY,
    nombres VARCHAR(150) NOT NULL,
    apellidos VARCHAR(150) NOT NULL,
    correo VARCHAR(150) UNIQUE NOT NULL,
    enlace_cvlac VARCHAR(250),
    escalafon VARCHAR(100),
    categoria_minciencias VARCHAR(100),
    linea_investigacion_principal INTEGER REFERENCES linea_investigacion(id) ON DELETE RESTRICT,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS grupo_investigacion (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(50) UNIQUE NOT NULL,
    nombre VARCHAR(200) NOT NULL,
    enlace_gruplac VARCHAR(250),
    categoria_minciencias VARCHAR(50),
    universidad_id INTEGER NOT NULL REFERENCES universidad(id) ON DELETE RESTRICT,
    docente_lider_cedula INTEGER NOT NULL REFERENCES docente(cedula) ON DELETE RESTRICT,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS semillero (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    descripcion TEXT,
    grupo_investigacion_id INTEGER NOT NULL REFERENCES grupo_investigacion(id) ON DELETE RESTRICT,
    activo BOOLEAN NOT NULL DEFAULT TRUE
);

-- -----------------------------------------------------------------------------
-- 4. TABLAS PUENTE N:M (v0.2)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS docente_grupo (
    docente_cedula INTEGER NOT NULL REFERENCES docente(cedula) ON DELETE RESTRICT,
    grupo_id INTEGER NOT NULL REFERENCES grupo_investigacion(id) ON DELETE RESTRICT,
    rol VARCHAR(100) NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (docente_cedula, grupo_id, fecha_inicio)
);

CREATE TABLE IF NOT EXISTS docente_semillero (
    docente_cedula INTEGER NOT NULL REFERENCES docente(cedula) ON DELETE RESTRICT,
    semillero_id INTEGER NOT NULL REFERENCES semillero(id) ON DELETE RESTRICT,
    rol VARCHAR(100) NOT NULL,
    periodo_academico VARCHAR(20) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (docente_cedula, semillero_id, periodo_academico)
);

CREATE TABLE IF NOT EXISTS grupo_linea (
    grupo_id INTEGER NOT NULL REFERENCES grupo_investigacion(id) ON DELETE RESTRICT,
    linea_id INTEGER NOT NULL REFERENCES linea_investigacion(id) ON DELETE RESTRICT,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (grupo_id, linea_id)
);

CREATE TABLE IF NOT EXISTS semillero_linea (
    semillero_id INTEGER NOT NULL REFERENCES semillero(id) ON DELETE RESTRICT,
    linea_id INTEGER NOT NULL REFERENCES linea_investigacion(id) ON DELETE RESTRICT,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (semillero_id, linea_id)
);

CREATE TABLE IF NOT EXISTS linea_area (
    linea_id INTEGER NOT NULL REFERENCES linea_investigacion(id) ON DELETE RESTRICT,
    area_conocimiento_id INTEGER NOT NULL REFERENCES area_conocimiento(id) ON DELETE RESTRICT,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (linea_id, area_conocimiento_id)
);

CREATE TABLE IF NOT EXISTS linea_ods (
    linea_id INTEGER NOT NULL REFERENCES linea_investigacion(id) ON DELETE RESTRICT,
    ods_id INTEGER NOT NULL REFERENCES objetivo_desarrollo_sostenible(id) ON DELETE RESTRICT,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (linea_id, ods_id)
);

CREATE TABLE IF NOT EXISTS linea_area_aplicacion (
    linea_id INTEGER NOT NULL REFERENCES linea_investigacion(id) ON DELETE RESTRICT,
    area_aplicacion_id INTEGER NOT NULL REFERENCES area_aplicacion(id) ON DELETE RESTRICT,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (linea_id, area_aplicacion_id)
);

-- -----------------------------------------------------------------------------
-- 5. DATOS SEMILLA RELACIONALES (v0.2)
-- -----------------------------------------------------------------------------
INSERT INTO docente (cedula, nombres, apellidos, correo, enlace_cvlac, escalafon, categoria_minciencias, linea_investigacion_principal, activo)
VALUES (10102020, 'Carlos Arturo', 'Castro Castro', 'ccastro@usbmed.edu.co', 'https://scienti.minciencias.gov.co/cvlac/0001', 'Asociado', 'Senior', 1, TRUE)
ON CONFLICT (cedula) DO NOTHING;

INSERT INTO docente (cedula, nombres, apellidos, correo, enlace_cvlac, escalafon, categoria_minciencias, linea_investigacion_principal, activo)
VALUES (10203040, 'Francisco', 'Guisado López', 'fguisado@usbmed.edu.co', 'https://scienti.minciencias.gov.co/cvlac/0002', 'Asistente', 'Junior', 1, TRUE)
ON CONFLICT (cedula) DO NOTHING;

INSERT INTO grupo_investigacion (codigo, nombre, enlace_gruplac, categoria_minciencias, universidad_id, docente_lider_cedula, activo)
VALUES ('COL0012345', 'Grupo de Investigación en Tecnologías Emergentes (GITE)', 'https://scienti.minciencias.gov.co/gruplac/0001', 'A1', 1, 10102020, TRUE)
ON CONFLICT (codigo) DO NOTHING;

INSERT INTO semillero (nombre, descripcion, grupo_investigacion_id, activo)
VALUES ('Semillero de Arquitectura de Software y Cloud Computing', 'Investigación formativa en microservicios, DevOps y plataformas distribuidas.', 1, TRUE)
ON CONFLICT DO NOTHING;

INSERT INTO grupo_linea (grupo_id, linea_id, activo) VALUES (1, 1, TRUE) ON CONFLICT DO NOTHING;
INSERT INTO semillero_linea (semillero_id, linea_id, activo) VALUES (1, 1, TRUE) ON CONFLICT DO NOTHING;
INSERT INTO linea_area (linea_id, area_conocimiento_id, activo) VALUES (1, 1, TRUE) ON CONFLICT DO NOTHING;
INSERT INTO linea_ods (linea_id, ods_id, activo) VALUES (1, 2, TRUE) ON CONFLICT DO NOTHING;
INSERT INTO linea_area_aplicacion (linea_id, area_aplicacion_id, activo) VALUES (1, 2, TRUE) ON CONFLICT DO NOTHING;

-- -----------------------------------------------------------------------------
-- 6. MÓDULO DE SEGURIDAD RBAC (v0.3)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS rol (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE,
    descripcion VARCHAR(200),
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    fecha_creacion TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS usuario (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    nombre_completo VARCHAR(150),
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    fecha_creacion TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS rol_usuario (
    usuario_id INT NOT NULL REFERENCES usuario(id) ON DELETE RESTRICT,
    rol_id INT NOT NULL REFERENCES rol(id) ON DELETE RESTRICT,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (usuario_id, rol_id)
);

-- Roles predeterminados
INSERT INTO rol (nombre, descripcion, activo)
VALUES 
    ('Administrador', 'Acceso total a la gestión del sistema y administración de usuarios.', TRUE),
    ('Editor', 'Capacidad de formulación, edición y vinculación académica.', TRUE),
    ('Consulta', 'Acceso de solo lectura para reportes e indicadores.', TRUE)
ON CONFLICT (nombre) DO NOTHING;

-- Usuarios Iniciales con hash BCrypt
-- admin: admin123
INSERT INTO usuario (username, password, email, nombre_completo, activo)
VALUES (
    'admin',
    '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy',
    'admin@usbmed.edu.co',
    'Administrador del Sistema',
    TRUE
)
ON CONFLICT (username) DO NOTHING;

-- superadmin: admin123
INSERT INTO usuario (username, password, email, nombre_completo, activo)
VALUES (
    'superadmin',
    '$2a$11$H9Wo2u2ZNqMhyP.amhXVbOzq145ftNgdsnwa8FvKfwYMQ2oqoY3/C',
    'superadmin@usbmed.edu.co',
    'Super Administrador MIRA',
    TRUE
)
ON CONFLICT (username) DO NOTHING;

-- editor1: editor123
INSERT INTO usuario (username, password, email, nombre_completo, activo)
VALUES (
    'editor1',
    '$2a$11$RFKSqtO.nxSe34Q7uHyiielaUkE1AroMDuphlDzWFIEUWBELVPIay',
    'editor1@usbmed.edu.co',
    'Editor Académico',
    TRUE
)
ON CONFLICT (username) DO NOTHING;

-- lector1: lector123
INSERT INTO usuario (username, password, email, nombre_completo, activo)
VALUES (
    'lector1',
    '$2a$11$9yhU5ggzwnZkGkfbEsf3xu.I5ygdjgr3B37aqpoyxqTD49ClvsRDi',
    'lector1@usbmed.edu.co',
    'Auditor Lector Consulta',
    TRUE
)
ON CONFLICT (username) DO NOTHING;

-- Asignación de Roles RBAC
INSERT INTO rol_usuario (usuario_id, rol_id, activo)
VALUES 
    ((SELECT id FROM usuario WHERE username = 'admin'), (SELECT id FROM rol WHERE nombre = 'Administrador'), TRUE),
    ((SELECT id FROM usuario WHERE username = 'superadmin'), (SELECT id FROM rol WHERE nombre = 'Administrador'), TRUE),
    ((SELECT id FROM usuario WHERE username = 'editor1'), (SELECT id FROM rol WHERE nombre = 'Editor'), TRUE),
    ((SELECT id FROM usuario WHERE username = 'lector1'), (SELECT id FROM rol WHERE nombre = 'Consulta'), TRUE)
ON CONFLICT (usuario_id, rol_id) DO NOTHING;
