-- =============================================================================
-- MIRA - Versión 0.3: Seguridad, Autenticación y Roles (RBAC)
-- =============================================================================

-- 1. Tabla de Roles (Permisos del Sistema)
CREATE TABLE IF NOT EXISTS rol (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE,
    descripcion VARCHAR(200),
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    fecha_creacion TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 2. Tabla de Usuarios (Credenciales con Hash BCrypt)
CREATE TABLE IF NOT EXISTS usuario (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL, -- Almacena Hash BCrypt (60 caracteres), NUNCA texto plano
    email VARCHAR(100) NOT NULL UNIQUE,
    nombre_completo VARCHAR(150),
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    fecha_creacion TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 3. Tabla Puente Rol - Usuario (Muchos a Muchos)
CREATE TABLE IF NOT EXISTS rol_usuario (
    usuario_id INT NOT NULL REFERENCES usuario(id) ON DELETE RESTRICT,
    rol_id INT NOT NULL REFERENCES rol(id) ON DELETE RESTRICT,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (usuario_id, rol_id)
);

-- =============================================================================
-- SEEDING INICIAL DE SEGURIDAD
-- =============================================================================

-- Roles predeterminados del sistema
INSERT INTO rol (nombre, descripcion, activo)
VALUES 
    ('Administrador', 'Acceso total a la gestión del sistema y administración de usuarios.', TRUE),
    ('Editor', 'Capacidad de formulación, edición y vinculación académica.', TRUE),
    ('Consulta', 'Acceso de solo lectura para reportes e indicadores.', TRUE)
ON CONFLICT (nombre) DO NOTHING;

-- Usuarios Iniciales del Sistema (Contraseñas con BCrypt workFactor = 11)
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

-- Asignación de Roles
INSERT INTO rol_usuario (usuario_id, rol_id, activo)
VALUES 
    ((SELECT id FROM usuario WHERE username = 'admin'), (SELECT id FROM rol WHERE nombre = 'Administrador'), TRUE),
    ((SELECT id FROM usuario WHERE username = 'superadmin'), (SELECT id FROM rol WHERE nombre = 'Administrador'), TRUE),
    ((SELECT id FROM usuario WHERE username = 'editor1'), (SELECT id FROM rol WHERE nombre = 'Editor'), TRUE),
    ((SELECT id FROM usuario WHERE username = 'lector1'), (SELECT id FROM rol WHERE nombre = 'Consulta'), TRUE)
ON CONFLICT (usuario_id, rol_id) DO NOTHING;
