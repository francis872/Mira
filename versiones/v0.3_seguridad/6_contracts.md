# Contratos de la API REST — MIRA v0.3 (`6_contracts.md`)

> **Especificación de Endpoints de Autenticación (`/api/Auth`) y Administración de Usuarios (`/api/Usuario`)**

---

## 1. Endpoints del Subsistema de Seguridad (v0.3)

| Método | Ruta | Autorización Requerida | Cuerpo de Entrada (DTO) | Respuestas HTTP |
| :---: | :--- | :--- | :--- | :--- |
| `POST` | `/api/Auth/login` | `[AllowAnonymous]` | `LoginRequest { username, password }` | `200 OK` (`LoginResponse`) \| `401 Unauthorized` |
| `GET` | `/api/Usuario` | `[Authorize(Roles = "Administrador")]` | *Ninguno* | `200 OK` \| `401 Unauthorized` \| `403 Forbidden` |
| `GET` | `/api/Usuario/{id}` | `[Authorize(Roles = "Administrador")]` | *Ninguno* | `200 OK` \| `404 Not Found` |
| `POST` | `/api/Usuario` | `[Authorize(Roles = "Administrador")]` | `UsuarioCrear { username, password, email, nombreCompleto, rolesIds }` | `201 Created` \| `400 Bad Request` |
| `DELETE` | `/api/Usuario/{id}` | `[Authorize(Roles = "Administrador")]` | *Ninguno* | `200 OK` (Borrado lógico) \| `404 Not Found` |

---

## 2. Diagramas Obligatorios: Secuencias de Error (`404 Not Found` y `400 Bad Request`)

### 2.1. Secuencia de Error `404 Not Found` (Quién decide "no existe")

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Administrador
    participant Ctrl as UsuarioController
    participant Svc as UsuarioService
    participant Repo as UsuarioRepository
    participant DB as PostgreSQL 16

    Admin->>Ctrl: DELETE /api/Usuario/999 (Header Authorization: Bearer <JWT>)
    Ctrl->>Svc: EliminarAsync(999)
    Svc->>Repo: ObtenerPorIdAsync(999)
    Repo->>DB: SELECT * FROM usuario WHERE id = 999 AND activo = TRUE;
    DB-->>Repo: 0 filas (null)
    Repo-->>Svc: retorna null
    Note over Svc: UsuarioService decide que el recurso no existe<br/>y lanza KeyNotFoundException
    Svc-->>Ctrl: throw new KeyNotFoundException("No se encontró un usuario activo con ID 999.")
    Note over Ctrl: UsuarioController traduce la excepción<br/>al código HTTP 404 Not Found
    Ctrl-->>Admin: HTTP 404 Not Found {"exito": false, "mensaje": "No se encontró un usuario activo con ID 999."}
```

> **Guía de lectura:** El diagrama muestra cómo `UsuarioRepository` devuelve `null` al consultar un ID inexistente o inactivo, `UsuarioService` lanza `KeyNotFoundException` y `UsuarioController` la traduce a `404 Not Found`.

---

### 2.2. Secuencia de Error `400 Bad Request` (Dónde corta la validación de duplicados)

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Administrador
    participant Ctrl as UsuarioController
    participant Svc as UsuarioService
    participant Repo as UsuarioRepository
    participant DB as PostgreSQL 16

    Admin->>Ctrl: POST /api/Usuario {"username": "admin", "email": "admin@mira.edu.co", ...}
    Ctrl->>Svc: CrearAsync(peticion)
    Svc->>Repo: ExisteUsernameOEmailAsync("admin", "admin@mira.edu.co")
    Repo->>DB: SELECT COUNT(1) FROM usuario WHERE LOWER(username) = LOWER(@Username) OR LOWER(email) = LOWER(@Email);
    DB-->>Repo: count = 1
    Repo-->>Svc: retorna true
    Note over Svc,Repo: Cortocircuito en UsuarioService:<br/>No se inicia transacción ni se ejecuta INSERT
    Svc-->>Ctrl: throw new ArgumentException("El nombre de usuario o el correo electrónico ya están registrados.")
    Note over Ctrl: UsuarioController captura ArgumentException<br/>y responde HTTP 400 Bad Request
    Ctrl-->>Admin: HTTP 400 Bad Request {"exito": false, "mensaje": "El nombre de usuario o el correo electrónico ya están registrados."}
```

> **Guía de lectura:** El diagrama ilustra el cortocircuito en `UsuarioService` cuando `ExisteUsernameOEmailAsync` detecta un usuario o correo duplicado, evitando abrir la transacción de escritura y devolviendo `400 Bad Request`.
