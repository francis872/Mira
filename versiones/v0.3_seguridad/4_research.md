# Investigación y Decisiones Arquitectónicas (ADRs) — MIRA v0.3 (`4_research.md`)

> **Registro de Decisiones de Diseño para la Capa de Seguridad (`v0.3`)**

---

## Tabla Comparativa de Decisiones Técnicas (ADRs)

| # | Decisión | Opción Elegida | Alternativa Descartada | Justificación Técnica y Consecuencias |
| :-: | :--- | :--- | :--- | :--- |
| **ADR-1** | Algoritmo de Hashing de Contraseñas | **`BCrypt.Net-Next` (`4.0.3`)** con factor de costo 11 (`$2a$11$...`) | `SHA-256` simple o `MD5` | `SHA-256` es vulnerable a ataques de diccionario y tablas rainbow por su velocidad. `BCrypt` incorpora sal criptográfica aleatoria por cada contraseña y factor de trabajo adaptativo. |
| **ADR-2** | Mecanismo de Autenticación en la API | **`JWT Bearer` (`Microsoft.AspNetCore.Authentication.JwtBearer`)** firmado con `HmacSha256` | Sesiones en memoria en el servidor .NET | Un token JWT *stateless* porta los claims de identidad (`sub`, `email`) y los roles (`ClaimTypes.Role`), permitiendo escalar contenedores sin perder sesión en la API. |
| **ADR-3** | Creación de Usuario + Asignación de Roles | **Transacción Explícita ADO.NET (`conn.BeginTransaction()`) en Dapper** | Dos llamadas independientes sin transacción | Si el `INSERT INTO usuario` tiene éxito pero el `INSERT INTO rol_usuario` falla, una ejecución sin transacción dejaría un usuario huérfano sin roles. `BeginTransaction()` garantiza atomicidad ACID. |
| **ADR-4** | Exposición de Datos de Usuario | **DTOs dedicados (`UsuarioCrear`, `UsuarioResponse`, `LoginResponse`) en `MIRA.Api/Peticiones/`** | Retornar directamente la clase `Modelos/Usuario.cs` | La entidad `Usuario` contiene la propiedad `Password` (hash BCrypt). Usar `UsuarioResponse` impide por contrato que el hash salga serializado en respuestas JSON. |
