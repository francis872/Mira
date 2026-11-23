-- Datos iniciales de V1 (Entrega 1). Idempotente: no duplica filas si se vuelve a aplicar.
INSERT INTO area_conocimiento (gran_area, area, disciplina, activo)
SELECT v.* FROM (VALUES
 ('Ingeniería y Tecnología', 'Ingeniería de Sistemas', 'Ingeniería de Software', TRUE),
 ('Ciencias Naturales', 'Ciencias de la Computación', 'Inteligencia Artificial', TRUE),
 ('Ciencias Sociales', 'Economía y Negocios', 'Gestión de Proyectos', TRUE)
) AS v(gran_area, area, disciplina, activo)
WHERE NOT EXISTS (SELECT 1 FROM area_conocimiento t WHERE t.disciplina = v.disciplina);

INSERT INTO objetivo_desarrollo_sostenible (nombre, categoria, activo)
SELECT v.* FROM (VALUES
 ('Educación de Calidad', 'Social', TRUE),
 ('Industria, Innovación e Infraestructura', 'Económico', TRUE),
 ('Acción por el Clima', 'Ambiental', TRUE)
) AS v(nombre, categoria, activo)
WHERE NOT EXISTS (SELECT 1 FROM objetivo_desarrollo_sostenible t WHERE t.nombre = v.nombre);

INSERT INTO area_aplicacion (nombre, activo)
SELECT v.* FROM (VALUES
 ('Salud y Telemedicina', TRUE),
 ('Educación Virtual', TRUE),
 ('Sector Financiero y Fintech', TRUE)
) AS v(nombre, activo)
WHERE NOT EXISTS (SELECT 1 FROM area_aplicacion t WHERE t.nombre = v.nombre);

INSERT INTO termino_clave (termino, termino_ingles, activo) VALUES
 ('Inteligencia Artificial', 'Artificial Intelligence', TRUE),
 ('Computación en la Nube', 'Cloud Computing', TRUE),
 ('Arquitectura de Software', 'Software Architecture', TRUE)
ON CONFLICT (termino) DO NOTHING;

INSERT INTO universidad (nombre, tipo, ciudad, activo)
SELECT v.* FROM (VALUES
 ('Universidad de San Buenaventura Medellín', 'Privada', 'Medellín', TRUE),
 ('Universidad de Antioquia', 'Pública', 'Medellín', TRUE),
 ('Universidad Nacional de Colombia', 'Pública', 'Bogotá', TRUE)
) AS v(nombre, tipo, ciudad, activo)
WHERE NOT EXISTS (SELECT 1 FROM universidad t WHERE t.nombre = v.nombre);

INSERT INTO linea_investigacion (nombre, descripcion, activo)
SELECT v.* FROM (VALUES
 ('Ingeniería de Software y Sistemas Distribuidos', 'Investigación orientada a arquitecturas modernas, microservicios y calidad de software.', TRUE),
 ('Inteligencia Artificial y Analítica de Datos', 'Enfoque en modelos de aprendizaje automático y visión por computadora.', TRUE),
 ('Ciberseguridad y Redes', 'Investigación en seguridad informática, protocolos criptográficos y protección de infraestructuras.', TRUE)
) AS v(nombre, descripcion, activo)
WHERE NOT EXISTS (SELECT 1 FROM linea_investigacion t WHERE t.nombre = v.nombre);
