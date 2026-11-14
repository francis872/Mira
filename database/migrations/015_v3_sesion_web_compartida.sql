-- Almacenamiento compartido para Flask en Vercel. No altera personas, JWT ni FK.
-- DDL exclusivo del operador de migraciones; el frontend nunca crea tablas.
CREATE SCHEMA IF NOT EXISTS mira_web;
REVOKE ALL ON SCHEMA mira_web FROM PUBLIC;
CREATE TABLE mira_web.sesion (
    clave_hash char(64) PRIMARY KEY CHECK (clave_hash ~ '^[0-9a-f]{64}$'),
    datos bytea NOT NULL CHECK (octet_length(datos) <= 65536),
    firma bytea NOT NULL CHECK (octet_length(firma) = 32),
    expira_en timestamptz NOT NULL
);
CREATE INDEX ix_sesion_web_expiracion ON mira_web.sesion(expira_en);
REVOKE ALL ON mira_web.sesion FROM PUBLIC;
