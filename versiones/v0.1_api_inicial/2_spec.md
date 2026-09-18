# Especificación Funcional v0.1 — Módulo de Investigación

**Fuente:** Plan de desarrollo Módulo de Investigación (v1.0) + Historias de Usuario (v1.0)  
**Fecha:** Agosto 2026  
**Audiencia:** Equipos de desarrollo, arquitectura, QA, stakeholders

---

## 1. Propósito

Diseñar e implementar un **Módulo de Investigación centralizado** en la Universidad San Buenaventura Medellín que permita:
- Gestionar, estructurar y hacer seguimiento integral a proyectos académicos.
- Centralizar la información de investigadores, grupos, semilleros y líneas de investigación.
- Articular la investigación institucional con los Objetivos de Desarrollo Sostenible (ODS).
- Proporcionar visibilidad y trazabilidad investigativa mediante reportes.

---

## 2. Alcance v0.1

### ✅ **Incluido en v0.1**

#### 2.1 Gestión de Usuarios y Seguridad
- **Autenticación por login** con generación de token JWT seguro.
- **Cifrado de contraseñas** con bcrypt/argon2.
- **Sistema de Roles y Control de Acceso** (Administrador, Coordinador, Líder de Grupo, Tutor de Semillero, Docente, Investigador).
- **Asignación y revocación de roles** con validación de permisos en frontend y backend.

#### 2.2 Catálogos Institucionales
- **Gestión de Áreas del Conocimiento**: Gran Área, Área, Disciplina.
- **Gestión de ODS**: Mapeo de 17 Objetivos de Desarrollo Sostenible (Social, Económica, Ambiental).
- **Gestión de Áreas de Aplicación**: Sectores productivos/económicos.
- **Gestión de Palabras Clave**: Multiidioma (ES/EN).
- **Operaciones CRUD** con validaciones y borrado lógico.

#### 2.3 Perfiles Académicos
- **Gestión de Perfiles Docentes/Investigadores**: Registro de CvLAC, escalafón docente, categoría Minciencias.
- **Validación de URLs** (CvLAC, GrupLAC).
- **Historial de vinculación** sin pérdida de datos.

#### 2.4 Sedes y Seccionales
- **Registro y listado de sedes** nacionales de la universidad.
- **Asociación de grupos** según ubicación geográfica.
- **Desactivación con histórico** (los grupos existentes conservan registro pero no nuevas asignaciones).

#### 2.5 Grupos de Investigación
- **Creación y administración** de grupos enlazados a una sede.
- **Vinculación de docentes/investigadores** con roles (Director, Tutor, Coinvestigador) y períodos.
- **Control de un único Líder de Grupo** en período activo.
- **Exportación de integrantes** (Excel/PDF).

#### 2.6 Semilleros de Investigación
- **Creación de semilleros** vinculados obligatoriamente a un grupo activo.
- **Asignación de docentes tutores** con rol y vigencia.
- **Prevención de duplicidad** de persona+rol+período.
- **Filtrado** por grupo base o docente tutor.

#### 2.7 Líneas de Investigación
- **Creación, edición y desactivación** de líneas institucionales.
- **Código único, nombre y descripción**.
- **Estado Activa/Inactiva** (líneas inactivas no disponibles para nuevas articulaciones).
- **Vinculación con Disciplinas, ODS y Áreas de Aplicación**.
- **Validación obligatoria**: al menos 1 ODS + 1 Área del Conocimiento.

#### 2.8 Articulación Investigativa
- **Vinculación de Grupos y Semilleros con Líneas de Investigación**: selección múltiple.
- **Desvincular líneas** con confirmación del usuario.
- **Visualización clara** de líneas en perfiles públicos.

#### 2.9 Trazabilidad e Informes
- **Dashboard unificado**: correspondencia Grupo/Semillero ↔ Docentes ↔ Línea ↔ ODS.
- **Filtros**: por Sede, Facultad, Línea de Investigación.
- **Descargas** en formato Excel/CSV (con coincidencia exacta de filtros).

---

### ❌ **Excluido de v0.1**

- Frontend web completo (solo especificación de contratos API).
- Sistema de Proyectos de Investigación y entregas.
- Módulo de Desempeño (Evaluación de Competencias, Factores de Éxito, Análisis Organizacional).
- Módulo de Desarrollo (Cursos, Podcasts, Videos, Capacitaciones).
- Reportes avanzados y cuadros de mando interactivos.
- Integración con sistemas externos (CvLAC, GrupLAC en tiempo real).
- Auditoría y trazas de cambios.
- Notificaciones por email o SMS.

---

## 3. Requisitos Funcionales por Historia

### HU-1: Gestión de Seguridad, Autenticación y Asignación de Roles  
**Puntos:** 5 | **Horas:** 20 | **Riesgo:** Alto | **Prioridad:** Alta

- RF-1.1: Login por usuario/email + contraseña → token JWT
- RF-1.2: Cifrado bcrypt/argon2 de contraseñas
- RF-1.3: Token JWT con expiración
- RF-1.4: Validación de credenciales y rechazo de acceso por rol
- RF-1.5: Admin puede asignar/revocar múltiples roles (cambios efectivos tras reauth)

### HU-2: Gestión de Catálogos (Áreas, ODS, Aplicación, Keywords)  
**Puntos:** 3 | **Horas:** 12 | **Riesgo:** Bajo | **Prioridad:** Alta

- RF-2.1: CRUD Áreas del Conocimiento (Gran Área, Área, Disciplina) + borrado lógico
- RF-2.2: CRUD ODS (validación: solo Social, Económica, Ambiental)
- RF-2.3: CRUD Áreas de Aplicación
- RF-2.4: CRUD Palabras Clave (ES/EN obligatorio)
- RF-2.5: Bloquear eliminación física si existen asociaciones

### HU-3: Gestión de Perfiles Docentes e Investigadores  
**Puntos:** 3 | **Horas:** 12 | **Riesgo:** Medio | **Prioridad:** Alta

- RF-3.1: Registro y edición de CvLAC, escalafón docente, categoría Minciencias
- RF-3.2: Validación URL del CvLAC
- RF-3.3: Categoría Minciencias seleccionable (Senior, Asociado, Junior, Sin Categoría)
- RF-3.4: Historial de vinculación sin pérdida
- RF-3.5: Retroalimentación visual (alerta éxito/error)

### HU-4: Gestión de Sedes y Seccionales Universitarias  
**Puntos:** 1 | **Horas:** 6 | **Riesgo:** Bajo | **Prioridad:** Alta

- RF-4.1: Registrar sedes (nombre, ciudad, código institucional)
- RF-4.2: Listar sedes (actualizados en tiempo real en otros módulos)
- RF-4.3: Desactivar sede (grupos históricos conservan, nuevas asignaciones bloqueadas)

### HU-5: Administración de Grupos de Investigación  
**Puntos:** 4 | **Horas:** 16 | **Riesgo:** Medio | **Prioridad:** Alta

- RF-5.1: Crear grupo (GrupLAC, categoría, sede) + validación URL
- RF-5.2: Vincular docentes (rol, fecha inicio, fin opcional)
- RF-5.3: Control: un único Líder de Grupo en período activo
- RF-5.4: Exportar integrantes (Excel/PDF)

### HU-6: Creación de Semilleros y Docentes Tutores  
**Puntos:** 3 | **Horas:** 12 | **Riesgo:** Medio | **Prioridad:** Alta

- RF-6.1: Crear semillero (vinculado obligatoriamente a grupo activo)
- RF-6.2: Asignar tutor (rol: Director, Tutor, Coinvestigador + vigencia)
- RF-6.3: Prevenir duplicidad persona+rol+período
- RF-6.4: Filtros por grupo base o docente tutor

### HU-7: Gestión de Líneas de Investigación Institucionales  
**Puntos:** 2 | **Horas:** 8 | **Riesgo:** Bajo | **Prioridad:** Alta

- RF-7.1: Crear línea (código único, nombre, descripción)
- RF-7.2: Cambiar estado (Activa/Inactiva)
- RF-7.3: Líneas inactivas no disponibles en selección
- RF-7.4: Historial de cambios intacto

### HU-8: Vinculación de Grupos y Semilleros con Líneas  
**Puntos:** 2 | **Horas:** 8 | **Riesgo:** Bajo | **Prioridad:** Alta

- RF-8.1: Selección múltiple de líneas para grupo/semillero
- RF-8.2: Desvincular con confirmación
- RF-8.3: Visualización clara en perfiles públicos

### HU-9: Relación de Líneas con Áreas, ODS, Sectores  
**Puntos:** 3 | **Horas:** 12 | **Riesgo:** Bajo | **Prioridad:** Alta

- RF-9.1: Asociar Disciplinas del catálogo de Áreas
- RF-9.2: Mapear 1+ de los 17 ODS
- RF-9.3: Asociar Áreas de Aplicación
- RF-9.4: Bloquear guardado sin 1 ODS + 1 Área del Conocimiento

### HU-10: Visualización y Reportes de Trazabilidad  
**Puntos:** 2 | **Horas:** 8 | **Riesgo:** Medio | **Prioridad:** Media

- RF-10.1: Dashboard unificado (Grupo ↔ Semillero ↔ Docentes ↔ Línea ↔ ODS)
- RF-10.2: Filtros por Sede, Facultad, Línea
- RF-10.3: Descargas Excel/CSV (con filtros coincidentes)

---

## 4. Requisitos No Funcionales

| ID | Categoría | Requisito | Criterio |
|----|-----------|-----------|----------|
| **RNF-1** | Seguridad | JWT con expiración | < 24 horas |
| **RNF-2** | Seguridad | Validación de entrada | Sin SQL injection, XSS |
| **RNF-3** | Rendimiento | Tiempo respuesta GET | < 500ms |
| **RNF-4** | Disponibilidad | BD replicada | 99.9% uptime |
| **RNF-5** | Escalabilidad | Soportar 1000 usuarios concurrentes | Load test |
| **RNF-6** | Mantenibilidad | C# + arquitectura limpia | Controllers → Services → Repos |
| **RNF-7** | Confiabilidad | Transacciones ACID | Rollback en errores |
| **RNF-8** | Auditabilidad | Log de cambios | Tabla `audit_log` |

---

## 5. Definición de Terminado (DoD v0.1)

Una característica se considera **terminada** cuando:

1. ✅ Código en rama feature
2. ✅ Pruebas unitarias ≥ 80% cobertura
3. ✅ Pruebas de integración en BD PostgreSQL local
4. ✅ Documentación API en Swagger
5. ✅ Validaciones de entrada (email, URLs, fechas)
6. ✅ Manejo de errores (400, 401, 403, 404, 500)
7. ✅ Revisión de código por pair programming
8. ✅ Merge a `develop` sin conflictos
9. ✅ Demo exitosa con stakeholder
10. ✅ PR documentada

---

## 6. Timeline v0.1

| Fase | Sprints | Período | Historias | Puntos | Horas |
|------|---------|---------|-----------|--------|-------|
| **1** | 3-4 | Jul-Ago | HU-1, HU-2, HU-4 | 9 | 38 |
| **2** | 5-6 | Ago-Sep | HU-3, HU-5, HU-7 | 9 | 36 |
| **3** | 7-8 | Sep-Oct | HU-6, HU-8, HU-9 | 7 | 28 |
| **4** | 9-12 | Oct-Nov | HU-10 + QA | 2 | 70 |
| **Total** | | | | **28** | **~112** |

---

**Generado:** 2026-09-04  
**Versión:** 0.1 Derivada de Fuentes Oficiales  
**Estado:** Aprobado para Sprint 3
