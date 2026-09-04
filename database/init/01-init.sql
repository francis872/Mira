CREATE TABLE IF NOT EXISTS usuarios (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(120) NOT NULL,
    correo VARCHAR(160) NOT NULL UNIQUE,
    password_hash VARCHAR(200) NOT NULL,
    rol VARCHAR(50) NOT NULL DEFAULT 'Docente',
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS sedes (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(120) NOT NULL,
    ciudad VARCHAR(120) NOT NULL,
    activa BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

INSERT INTO usuarios (nombre, correo, password_hash, rol)
VALUES ('Administrador MIRA', 'admin@mira.local', 'Admin123!', 'Administrador')
ON CONFLICT (correo) DO NOTHING;

INSERT INTO sedes (nombre, ciudad, activa)
VALUES ('Sede Medellin', 'Medellin', TRUE)
ON CONFLICT DO NOTHING;
