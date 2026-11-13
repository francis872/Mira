# Plan Técnico de Implementación — MIRA v0.3 (`3_plan.md`)

> **Versión 0.3:** Seguridad Perimetral Stateless con JWT Bearer, Hashing BCrypt y Control de Acceso Basado en Roles (RBAC).

---

## 1. Diagrama Obligatorio 1: Arquitectura de Despliegue (Docker Compose)

```mermaid
flowchart LR
    subgraph Host[" Red Interna Docker: mira-network (bridge) "]
        Front["Contenedor: mira-frontend\nPython 3.11 + Flask\nPuerto Host 5000 -> 5000"]
        Api["Contenedor: mira-api\n.NET 7 + Dapper + JwtBearer + BCrypt\nPuerto Host 8081 -> 8080"]
        Db[("Contenedor: mira-postgres\nPostgreSQL 16\nPuerto Host 5544 -> 5432")]
    end

    Front -- "HTTP REST JSON + Bearer JWT\nAPI_URL=http://mira-api:8080/api" --> Api
    Api -- "TCP 5432 (Npgsql)\nVariables de entorno (.env)" --> Db
```

> **Guía de lectura:** Las tres cajas representan los contenedores orquestados por `docker-compose.yml` dentro de la red `mira-network`, indicando el mapeo de puertos hacia la máquina anfitriona (`5000`, `8081` y `5544`) y el protocolo que conecta cada servicio.

---

## 2. Diagrama Obligatorio 2: Diagrama de Clases de la Rebanada de Seguridad

```mermaid
classDiagram
    class AuthController {
        -IAuthService _authService
        +Login(LoginRequest peticion) Task~IActionResult~
    }
    class UsuarioController {
        -IUsuarioService _usuarioService
        +ObtenerTodos() Task~IActionResult~
        +ObtenerPorId(int id) Task~IActionResult~
        +Crear(UsuarioCrear peticion) Task~IActionResult~
        +Eliminar(int id) Task~IActionResult~
    }
    class IAuthService {
        <<interface>>
        +LoginAsync(LoginRequest request) Task~LoginResponse~
    }
    class AuthService {
        -IUsuarioRepository _usuarioRepo
        -IConfiguration _config
        +LoginAsync(LoginRequest request) Task~LoginResponse~
        -GenerarJwtToken(Usuario usuario) string
    }
    class IUsuarioService {
        <<interface>>
        +ObtenerTodosAsync() Task~IEnumerable~UsuarioResponse~~
        +ObtenerPorIdAsync(int id) Task~UsuarioResponse~
        +CrearAsync(UsuarioCrear peticion) Task~UsuarioResponse~
        +EliminarAsync(int id) Task
    }
    class UsuarioService {
        -IUsuarioRepository _usuarioRepo
    }
    class IUsuarioRepository {
        <<interface>>
        +ObtenerPorUsernameAsync(string username) Task~Usuario~
        +ObtenerTodosActivosAsync() Task~IEnumerable~Usuario~~
        +CrearAsync(Usuario usuario, List~int~ rolesIds) Task~int~
        +EliminarLogicoAsync(int id) Task~bool~
    }
    class UsuarioRepository {
        -IDbConnectionFactory _dbFactory
    }

    AuthController --> IAuthService
    UsuarioController --> IUsuarioService
    IAuthService <|.. AuthService
    IUsuarioService <|.. UsuarioService
    AuthService --> IUsuarioRepository
    UsuarioService --> IUsuarioRepository
    IUsuarioRepository <|.. UsuarioRepository
```

> **Guía de lectura:** El diagrama de clases muestra las clases reales de C# en `MIRA.Api` para autenticación y usuarios, donde los controladores dependen únicamente de las interfaces `IAuthService` e `IUsuarioService`, y los servicios dependen de `IUsuarioRepository`.

---

## 3. Diagrama Obligatorio 3: Secuencia del Camino Feliz (Login y Emisión de JWT)

```mermaid
sequenceDiagram
    autonumber
    actor Usuario as Usuario
    participant Flask as frontend/app.py
    participant Ctrl as AuthController
    participant Svc as AuthService
    participant Repo as UsuarioRepository
    participant DB as PostgreSQL 16

    Usuario->>Flask: Envía credenciales en POST /login
    Flask->>Ctrl: POST /api/Auth/login (LoginRequest)
    Ctrl->>Svc: LoginAsync(peticion)
    Svc->>Repo: ObtenerPorUsernameAsync(username)
    Repo->>DB: SELECT usuario activo + SELECT roles activos
    DB-->>Repo: Registro Usuario + Lista de Roles
    Repo-->>Svc: Usuario hidratado
    Svc->>Svc: BCrypt.Verify(password, usuario.Password) == true
    Svc->>Svc: GenerarJwtToken(usuario) con HmacSha256
    Svc-->>Ctrl: LoginResponse { Token, Usuario }
    Ctrl-->>Flask: 200 OK
    Flask-->>Usuario: Guarda JWT en sesión y redirige al Panel Principal
```

> **Guía de lectura:** Representa el camino feliz de inicio de sesión desde el formulario en Flask hasta la verificación del hash BCrypt y la firma del token JWT en `AuthService`.

---

## 4. Compuerta 2: Chequeo de Constitución

- [x] **Regla de Capas (`Controller` → `Service` → `Repository`):** `AuthController` y `UsuarioController` no contienen lógica criptográfica ni consultas SQL.
- [x] **SQL Parametrizado con Dapper:** `UsuarioRepository` utiliza parámetros `@Username`, `@Email`, `@UsuarioId`, `@RolId` y transacciones explícitas `BeginTransaction()`.
- [x] **Borrado Lógico:** `EliminarLogicoAsync` ejecuta `UPDATE usuario SET activo = FALSE, fecha_actualizacion = CURRENT_TIMESTAMP`.
- [x] **Cero Secretos Quemados:** La clave de firma JWT y la cadena de conexión se inyectan por variables de entorno desde `.env` / `docker-compose.yml`.
