# MIRA - Módulo de Investigación para la Gestión de Proyectos Académicos

**Universidad de San Buenaventura Medellín**  
**Proyecto de Aula — Diseño de Software (Metodología SDD / Spec Kit)**  
**Estudiante:** JUAN JOSE GRISALES SAÑUDO — ID: 30000129372  

---

## Estado del Proyecto: Versión v0.4 (100% Completa — 4 Iteraciones SDD)

El sistema **MIRA** integra la totalidad de las 4 entregas evolutivas bajo **Desarrollo Guiado por Especificaciones (SDD)**:
* **Versión 1 (`v1` — `versiones/v0.1_api_inicial/`):** 6 Catálogos maestros institucionales en arquitectura multicapa (.NET 7 + Dapper + PostgreSQL 16 + Flask).
* **Versión 2 (`v2` — `versiones/v0.2_relaciones/`):** Modelo relacional complejo (16 tablas), 7 tablas puente (N:M), borrado lógico (`activo = true/false`) y patrón Maestro-Detalle (**HU 3, 5, 6, 8 y 9**).
* **Versión 3 (`v3` — `versiones/v0.3_seguridad/`):** Seguridad perimetral stateless mediante **Tokens JWT (HMAC-SHA256)**, almacenamiento criptográfico con **BCrypt (`workFactor = 11`)** y **Control de Acceso Basado en Roles (RBAC)**.
* **Versión 4 (`v4` — `versiones/v0.4_dashboard/`):** **10 Consultas Multitabla (4+ tablas por consulta)** en Dapper (`/api/reportes/dashboard` y `/api/reportes/consulta/{1..10}`), **Dashboard Ejecutivo Interactivo (`/dashboard`)** con 4 gráficas `Chart.js`, **Manual de Identidad Corporativa (`MANUAL_DE_MARCA.md` + `static/css/marca.css`)**, **5 Páginas Corporativas (`/corporativo/*`)** y **PWA Responsiva (`manifest.json` + `sw.js`)**.

---

## Documentación Transversal SDD y Spec Kit

| Documento | Ruta | Descripción |
| :--- | :--- | :--- |
| **Constitución Arquitectónica** | `docs/1_constitution.md` | Regla de Dependencia de Capas, SQL explícito con Dapper, borrado lógico y cero secretos en código. |
| **Catálogo de Historias de Usuario** | `docs/0_historias_de_usuario.md` | Historias HU 1 a HU 12 con criterios de aceptación y diagramas `sequenceDiagram` obligatorios. |
| **Mapa de Versiones** | `versiones/0_mapa_versiones.md` | Evolución incremental `v1` → `v2` → `v3` → `v4`, tags de Git y principio de No Anticipación (YAGNI). |
| **Manual de Marca Institucional** | `MANUAL_DE_MARCA.md` | Paleta cromática USB Medellín, contraste WCAG AA, tipografía y variables CSS (`frontend/static/css/marca.css`). |
| **Manual Técnico Maestro** | `MANUAL_MIRA_ARQUITECTURA_Y_SPECS.md` | Radiografía de código real por capas, Spec Kits, secuencias y diccionario SQL de las 18 tablas. |

---

## Credenciales de Acceso y Matriz de Roles (RBAC)

Para ingresar a la plataforma web (`http://localhost:5000`) y comprobar los niveles de autorización, configure las variables en su archivo local `.env` (basado en `.env.example`):

| Usuario | Contraseña | Rol Asignado | Alcance y Permisos en el Sistema |
| :--- | :--- | :--- | :--- |
| `<USUARIO_ADMIN>` | `<PASSWORD_ADMIN>` | **Administrador** | Acceso total al sistema, creación/edición de catálogos, Dashboard Analítico (`/dashboard`) y gestión exclusiva del módulo administrativo de cuentas (`/usuarios`). |
| `<USUARIO_EDITOR>` | `<PASSWORD_EDITOR>` | **Editor** | Gestión operativa: creación y edición de docentes, grupos, semilleros, articulaciones (HU 3, 5, 6, 8, 9) y consulta del Dashboard Analítico (`/dashboard`). Sin acceso a gestión de usuarios. |
| `<USUARIO_LECTOR>` | `<PASSWORD_LECTOR>` | **Consulta** | Modo solo lectura y visualización ejecutiva del Dashboard Analítico (`/dashboard`). La interfaz oculta contextualmente botones de mutación y los decoradores bloquean escrituras. |

---

## Despliegue Rápido con Docker Compose

El proyecto está completamente contenerizado para garantizar su reproducibilidad inmediata en cualquier entorno.

### Requisitos
* Docker Desktop activo.

### Comandos de ejecución
1. Clonar el repositorio y ubicarse en la raíz del proyecto:
   ```bash
   git clone https://github.com/Griza2005/Mira.git
   cd Mira
   ```
2. Copiar la plantilla `.env.example` a `.env` (archivo excluido de Git) y levantar la pila de contenedores:
   ```bash
   cp .env.example .env
   docker compose up -d --build
   ```
3. Accesos locales:
   * **Portal Web & Dashboard Analítico (Flask):** `http://localhost:5000` (`http://localhost:5000/dashboard` y `http://localhost:5000/corporativo/inicio`)
   * **Documentación Interactiva OpenAPI / Swagger (.NET 7):** `http://localhost:8081/swagger`
   * **Motor PostgreSQL 16:** `localhost:5544` (`mira_db`)
