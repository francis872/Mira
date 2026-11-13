# Plan Técnico de Implementación — MIRA v0.4 (`3_plan.md`)

> **Versión 0.4:** Consultas Analíticas Multitabla en Dapper, Dashboard con Chart.js, Identidad Corporativa y PWA.

---

## 1. Diagrama Obligatorio 1: Despliegue de Contenedores y Servicios (v0.4)

```mermaid
flowchart LR
    subgraph DockerNet["Red Docker: mira-network"]
        Front["mira-frontend (:5000)\nFlask + Jinja2 + Chart.js\nmarca.css + PWA manifest.json"]
        Api["mira-api (:8081 -> :8080)\n.NET 7 + ReportesController\n+ 12 Controladores v1-v3"]
        Db[("mira-postgres (:5544 -> :5432)\nPostgreSQL 16\n19 Tablas Activas")]
    end

    Front -- "GET /api/reportes/dashboard\nGET /api/reportes/consulta-1..10" --> Api
    Api -- "Dapper QueryAsync\n(JOINs de 4 a 7 tablas)" --> Db
```

> **Guía de lectura:** Ilustra la topología de contenedores en la `v0.4`, donde `mira-frontend` incorpora Chart.js, `marca.css` y soporte PWA, consumiendo los nuevos endpoints analíticos de `mira-api`.

---

## 2. Diagrama Obligatorio 2: Diagrama de Clases del Módulo de Reportes Multitabla

```mermaid
classDiagram
    class ReportesController {
        -IReporteService _service
        +GetDashboard() Task~IActionResult~
        +GetConsulta(int numero) Task~IActionResult~
    }
    class IReporteService {
        <<interface>>
        +ObtenerDashboardCompletoAsync() Task~DashboardEjecutivoDto~
        +EjecutarConsultaMultitablaAsync(int numero) Task~object~
    }
    class ReporteService {
        -IReporteRepository _repository
        +ObtenerDashboardCompletoAsync() Task~DashboardEjecutivoDto~
        +EjecutarConsultaMultitablaAsync(int numero) Task~object~
    }
    class IReporteRepository {
        <<interface>>
        +GetConsulta1GruposUniversidadLiderLineasAsync() Task~IEnumerable~
        +GetConsulta2SemillerosGruposTutoresAsync() Task~IEnumerable~
        +GetConsulta3ImpactoOdsGruposUniversidadAsync() Task~IEnumerable~
        +GetConsulta4CoberturaOcdeGruposLiderAsync() Task~IEnumerable~
        +GetConsulta5TransferenciaAplicacionSemillerosAsync() Task~IEnumerable~
        +GetConsulta6TrayectoriaDocentesGruposAsync() Task~IEnumerable~
        +GetConsulta7MatrizSemillerosLineasOcdeOdsAsync() Task~IEnumerable~
        +GetConsulta8CapacidadCiudadUniversidadAsync() Task~IEnumerable~
        +GetConsulta9MapaIntegralLineasAsync() Task~IEnumerable~
        +GetConsulta10AuditoriaSeguridadRbacDocentesAsync() Task~IEnumerable~
    }
    class ReporteRepository {
        -IDbConnectionFactory _connectionFactory
    }

    ReportesController --> IReporteService
    IReporteService <|.. ReporteService
    ReporteService --> IReporteRepository
    IReporteRepository <|.. ReporteRepository
```

> **Guía de lectura:** Muestra la arquitectura de clases de la rebanada analítica (`ReportesController` → `IReporteService` → `IReporteRepository`), manteniendo el desacoplamiento por interfaces exigido en la constitución.

---

## 3. Diagrama Obligatorio 3: Secuencia del Camino Feliz (Dashboard y 10 Consultas)

```mermaid
sequenceDiagram
    autonumber
    actor Usuario as Usuario Autenticado
    participant Flask as frontend/app.py
    participant Ctrl as ReportesController
    participant Svc as ReporteService
    participant Repo as ReporteRepository
    participant DB as PostgreSQL 16

    Usuario->>Flask: Abre GET /dashboard
    Flask->>Ctrl: GET /api/reportes/dashboard (Header Authorization: Bearer <JWT>)
    Ctrl->>Svc: ObtenerDashboardCompletoAsync()
    Svc->>Repo: Invoca las 10 consultas multitabla (GetConsulta1..GetConsulta10)
    Repo->>DB: Ejecuta sentencias SQL SELECT con INNER/LEFT JOIN de 4 a 7 tablas
    DB-->>Repo: Filas proyectadas de las 10 consultas
    Repo-->>Svc: Colecciones tipadas
    Svc-->>Ctrl: DashboardEjecutivoDto
    Ctrl-->>Flask: 200 OK (JSON)
    Flask-->>Usuario: Renderiza dashboard.html con 4 gráficos Chart.js y 10 pestañas de datos
```

> **Guía de lectura:** Describe cómo una petición a `/dashboard` viaja autenticada con JWT hasta `ReporteRepository`, hidrata las 10 consultas multitabla desde PostgreSQL y alimenta los gráficos de Chart.js en el cliente.

---

## 4. Compuerta 2: Chequeo de Constitución

- [x] **Respeto de las 4 capas:** `ReportesController` delega en `IReporteService` y este en `IReporteRepository`.
- [x] **SQL Explícito con Dapper:** Las 10 consultas multitabla están escritas en SQL puro con `INNER JOIN` y `LEFT JOIN`, filtrando registros activos (`activo = TRUE`).
- [x] **Identidad Visual Desacoplada:** Los estilos de `/dashboard` y de las 5 páginas corporativas consumen las variables de `frontend/static/css/marca.css` definidas en `MANUAL_DE_MARCA.md`.
