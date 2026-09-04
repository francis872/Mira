# Modelo de Datos v0.1 — Módulo de Investigación

**Fuente:** Historias de Usuario (HU 1-10) + Plan de Desarrollo  
**Motor BD:** PostgreSQL 15+  
**ORM:** Entity Framework Core 8  
**Fecha:** Agosto 2026

---

## 1. Diagrama Entidad-Relación (ER)

```
┌─────────────────┐
│    usuarios     │
├─────────────────┤
│ id (PK)         │
│ email (UNIQUE)  │
│ password_hash   │
│ created_at      │
│ updated_at      │
└────────┬────────┘
         │ (1:N)
         ↓
┌──────────────────────┐     ┌──────────────────┐
│  usuario_roles (J)   │────→│     roles        │
├──────────────────────┤     ├──────────────────┤
│ usuario_id (FK)      │     │ id (PK)          │
│ rol_id (FK)          │     │ nombre (UNIQUE)  │
└──────────────────────┘     │ descripcion      │
                             └──────────────────┘

┌─────────────────────┐
│     docentes        │
├─────────────────────┤
│ id (PK)             │
│ usuario_id (FK)     │ → usuarios
│ cvlac_url           │
│ escalafon           │
│ minciencias_cat     │  (Senior, Asociado, Junior, Sin Categoría)
│ created_at          │
└────────┬────────────┘
         │ (N:N)
         ├─→ grupos_investigacion (via grupo_docentes)
         ├─→ semilleros (via semillero_tutores)
         └─→ lineas_investigacion (via linea_coordinadores)

┌──────────────────┐     ┌──────────────────┐
│     sedes        │     │ grupos_invest.   │
├──────────────────┤     ├──────────────────┤
│ id (PK)          │←────│ id (PK)          │
│ nombre           │     │ sede_id (FK)     │
│ ciudad           │     │ gruptac_url      │
│ codigo_inst      │     │ categoria        │
│ activo (default) │     │ activo (default) │
│ deleted_at (soft)│     │ deleted_at (soft)│
└──────────────────┘     └────────┬─────────┘
                                  │ (1:N)
                                  ↓
                         ┌──────────────────────┐
                         │  semilleros          │
                         ├──────────────────────┤
                         │ id (PK)              │
                         │ grupo_id (FK)        │ (NOT NULL)
                         │ nombre               │
                         │ descripcion          │
                         │ activo (default)     │
                         │ deleted_at (soft)    │
                         └──────────────────────┘

┌─────────────────────────────┐
│  lineas_investigacion       │
├─────────────────────────────┤
│ id (PK)                     │
│ codigo (UNIQUE)             │
│ nombre                      │
│ descripcion                 │
│ activo (default)            │
│ deleted_at (soft)           │
└─────────────────────────────┘
         │ (N:N)
         ├─→ grupos (via grupo_lineas)
         ├─→ semilleros (via semillero_lineas)
         ├─→ disciplinas (via linea_disciplinas)
         ├─→ ods (via linea_ods)
         └─→ aplicaciones (via linea_aplicacion)

┌────────────────────────┐
│  areas_conocimiento    │  (Catálogo)
├────────────────────────┤
│ id (PK)                │
│ tipo (ENUM)            │  (gran_area, area, disciplina)
│ nombre                 │
│ activo (default)       │
│ deleted_at (soft)      │
└────────────────────────┘

┌────────────────┐
│      ods       │  (Catálogo)
├────────────────┤
│ id (PK)        │
│ nombre         │
│ categoria      │  (social, economica, ambiental)
│ numero (1-17)  │
│ activo         │
└────────────────┘

┌───────────────────────┐
│  areas_aplicacion     │  (Catálogo)
├───────────────────────┤
│ id (PK)               │
│ nombre                │
│ activo (default)      │
│ deleted_at (soft)     │
└───────────────────────┘

┌───────────────────────┐
│   palabras_clave      │  (Catálogo)
├───────────────────────┤
│ id (PK)               │
│ termino_es (NOT NULL) │
│ termino_en (NOT NULL) │
│ activo (default)      │
│ deleted_at (soft)     │
└───────────────────────┘
```

---

## 2. Tablas Detalladas

### 2.1 `usuarios`
```sql
CREATE TABLE usuarios (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    activo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT email_valid CHECK (email ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$')
);
```

### 2.2 `roles`
```sql
CREATE TABLE roles (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(50) UNIQUE NOT NULL,
    descripcion TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO roles (nombre, descripcion) VALUES
('Administrador', 'Acceso total al sistema'),
('Coordinador', 'Gestión de líneas y coordinación'),
('Líder de Grupo', 'Administración de su grupo'),
('Tutor de Semillero', 'Gestión de semillero'),
('Docente', 'Participante en investigación'),
('Investigador', 'Participante en investigación');
```

### 2.3 `usuario_roles`
```sql
CREATE TABLE usuario_roles (
    usuario_id UUID NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    rol_id SERIAL NOT NULL REFERENCES roles(id),
    PRIMARY KEY (usuario_id, rol_id)
);
```

### 2.4 `sedes`
```sql
CREATE TABLE sedes (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    ciudad VARCHAR(100) NOT NULL,
    codigo_institucional VARCHAR(50) UNIQUE,
    activo BOOLEAN DEFAULT true,
    deleted_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT code_not_empty CHECK (codigo_institucional != '')
);
```

### 2.5 `docentes`
```sql
CREATE TABLE docentes (
    id SERIAL PRIMARY KEY,
    usuario_id UUID UNIQUE NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    cvlac_url VARCHAR(500),
    escalafon VARCHAR(100),
    minciencias_categoria VARCHAR(50) CHECK (minciencias_categoria IN ('Senior', 'Asociado', 'Junior', 'Sin Categoría')),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### 2.6 `grupos_investigacion`
```sql
CREATE TABLE grupos_investigacion (
    id SERIAL PRIMARY KEY,
    sede_id SERIAL NOT NULL REFERENCES sedes(id),
    gruptac_url VARCHAR(500) NOT NULL,
    categoria VARCHAR(100),
    activo BOOLEAN DEFAULT true,
    deleted_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT gruptac_valid CHECK (gruptac_url ~ '^https?://')
);
```

### 2.7 `grupo_docentes` (vinculación M:N con período)
```sql
CREATE TABLE grupo_docentes (
    id SERIAL PRIMARY KEY,
    grupo_id SERIAL NOT NULL REFERENCES grupos_investigacion(id) ON DELETE CASCADE,
    docente_id SERIAL NOT NULL REFERENCES docentes(id) ON DELETE CASCADE,
    rol VARCHAR(100) NOT NULL CHECK (rol IN ('Líder de Grupo', 'Coinvestigador', 'Colaborador')),
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE,
    activo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (grupo_id, docente_id, rol, fecha_inicio, fecha_fin),
    CONSTRAINT valid_period CHECK (fecha_fin IS NULL OR fecha_fin > fecha_inicio)
);
```

### 2.8 `semilleros`
```sql
CREATE TABLE semilleros (
    id SERIAL PRIMARY KEY,
    grupo_id SERIAL NOT NULL REFERENCES grupos_investigacion(id),
    nombre VARCHAR(200) NOT NULL,
    descripcion TEXT,
    activo BOOLEAN DEFAULT true,
    deleted_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT grupo_active CHECK (
        SELECT activo FROM grupos_investigacion WHERE id = grupo_id
    )
);
```

### 2.9 `semillero_tutores` (vinculación M:N con período)
```sql
CREATE TABLE semillero_tutores (
    id SERIAL PRIMARY KEY,
    semillero_id SERIAL NOT NULL REFERENCES semilleros(id) ON DELETE CASCADE,
    docente_id SERIAL NOT NULL REFERENCES docentes(id) ON DELETE CASCADE,
    rol VARCHAR(100) NOT NULL CHECK (rol IN ('Director', 'Tutor', 'Coinvestigador')),
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (semillero_id, docente_id, rol, fecha_inicio, fecha_fin),
    CONSTRAINT valid_period CHECK (fecha_fin IS NULL OR fecha_fin > fecha_inicio)
);
```

### 2.10 `lineas_investigacion`
```sql
CREATE TABLE lineas_investigacion (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(50) UNIQUE NOT NULL,
    nombre VARCHAR(200) NOT NULL,
    descripcion TEXT,
    activo BOOLEAN DEFAULT true,
    deleted_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### 2.11 `areas_conocimiento` (Catálogo)
```sql
CREATE TABLE areas_conocimiento (
    id SERIAL PRIMARY KEY,
    tipo VARCHAR(50) NOT NULL CHECK (tipo IN ('gran_area', 'area', 'disciplina')),
    nombre VARCHAR(200) NOT NULL,
    activo BOOLEAN DEFAULT true,
    deleted_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### 2.12 `ods` (Catálogo)
```sql
CREATE TABLE ods (
    id SERIAL PRIMARY KEY,
    numero INT UNIQUE NOT NULL CHECK (numero >= 1 AND numero <= 17),
    nombre VARCHAR(200) NOT NULL,
    categoria VARCHAR(50) NOT NULL CHECK (categoria IN ('social', 'economica', 'ambiental')),
    activo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### 2.13 `areas_aplicacion` (Catálogo)
```sql
CREATE TABLE areas_aplicacion (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    activo BOOLEAN DEFAULT true,
    deleted_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### 2.14 `palabras_clave` (Catálogo)
```sql
CREATE TABLE palabras_clave (
    id SERIAL PRIMARY KEY,
    termino_es VARCHAR(200) NOT NULL,
    termino_en VARCHAR(200) NOT NULL,
    activo BOOLEAN DEFAULT true,
    deleted_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### 2.15 Tablas de Articulación (M:N)

```sql
CREATE TABLE grupo_lineas (
    grupo_id SERIAL NOT NULL REFERENCES grupos_investigacion(id) ON DELETE CASCADE,
    linea_id SERIAL NOT NULL REFERENCES lineas_investigacion(id) ON DELETE CASCADE,
    PRIMARY KEY (grupo_id, linea_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE semillero_lineas (
    semillero_id SERIAL NOT NULL REFERENCES semilleros(id) ON DELETE CASCADE,
    linea_id SERIAL NOT NULL REFERENCES lineas_investigacion(id) ON DELETE CASCADE,
    PRIMARY KEY (semillero_id, linea_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE linea_disciplinas (
    linea_id SERIAL NOT NULL REFERENCES lineas_investigacion(id) ON DELETE CASCADE,
    disciplina_id SERIAL NOT NULL REFERENCES areas_conocimiento(id),
    PRIMARY KEY (linea_id, disciplina_id)
);

CREATE TABLE linea_ods (
    linea_id SERIAL NOT NULL REFERENCES lineas_investigacion(id) ON DELETE CASCADE,
    ods_id SERIAL NOT NULL REFERENCES ods(id),
    PRIMARY KEY (linea_id, ods_id)
);

CREATE TABLE linea_aplicacion (
    linea_id SERIAL NOT NULL REFERENCES lineas_investigacion(id) ON DELETE CASCADE,
    aplicacion_id SERIAL NOT NULL REFERENCES areas_aplicacion(id),
    PRIMARY KEY (linea_id, aplicacion_id)
);
```

---

## 3. Índices de Rendimiento

```sql
CREATE INDEX idx_usuarios_email ON usuarios(email);
CREATE INDEX idx_docentes_usuario_id ON docentes(usuario_id);
CREATE INDEX idx_grupos_sede_id ON grupos_investigacion(sede_id);
CREATE INDEX idx_grupo_docentes_grupo_id ON grupo_docentes(grupo_id);
CREATE INDEX idx_grupo_docentes_docente_id ON grupo_docentes(docente_id);
CREATE INDEX idx_semilleros_grupo_id ON semilleros(grupo_id);
CREATE INDEX idx_semillero_tutores_semillero_id ON semillero_tutores(semillero_id);
CREATE INDEX idx_linea_disciplinas_linea_id ON linea_disciplinas(linea_id);
CREATE INDEX idx_linea_ods_linea_id ON linea_ods(linea_id);
CREATE INDEX idx_linea_aplicacion_linea_id ON linea_aplicacion(linea_id);
```

---

## 4. Responsabilidades

### BD (PostgreSQL)
- ✅ Integrity: constraints, foreign keys, unique indexes
- ✅ Performance: índices estratégicos
- ✅ ACID: transacciones multi-tabla
- ✅ Audit: soft deletes, timestamps

### API (EF Core + Services)
- ✅ Lógica de negocio: validaciones complejas (ej. "un único Líder activo por grupo")
- ✅ Transformación: DTOs, mappings
- ✅ Autorización: validaciones por rol y ownership
- ✅ Logging: cambios significativos

---

## 5. Seed Data (Inicial)

Ver `database/init/seed.sql` para:
- Roles por defecto
- 17 ODS estándar
- Áreas del Conocimiento base (COLCIENCIAS)
- Usuario admin inicial (hash bcrypt)

---

**Generado:** 2026-09-04  
**Versión:** 0.1  
**Estado:** Aprobado para Sprint 3
