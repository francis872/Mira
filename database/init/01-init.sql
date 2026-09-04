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

INSERT INTO usuarios (nombre, correo, password_hash)
VALUES ('Administrador MIRA', 'admin@mira.local', 'Admin123!')
ON CONFLICT (correo) DO NOTHING;

INSERT INTO roles (nombre)
VALUES ('Administrador'), ('Docente')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO usuario_roles (usuario_id, rol_id)
SELECT u.id, r.id
FROM usuarios u
CROSS JOIN roles r
WHERE u.correo = 'admin@mira.local'
  AND r.nombre = 'Administrador'
ON CONFLICT DO NOTHING;

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
