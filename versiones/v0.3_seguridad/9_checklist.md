# Lista de Chequeo de Requisitos (Compuerta 3) — MIRA v0.3 (`9_checklist.md`)

> **Estado de la Compuerta 3:** ✅ APROBADA (En Verde)  
> Incluye regresión completa de `v0.1` y `v0.2` más todos los criterios de seguridad JWT, BCrypt y RBAC de `v0.3`.

---

## 1. Verificación de las Tres Compuertas SDD

- [x] **Compuerta 1 (Clarificaciones en `2_spec.md`):** Resuelto el manejo de sesión JWT en Flask y el bloqueo de usuarios con `activo = FALSE`.
- [x] **Compuerta 2 (Chequeo de Constitución en `3_plan.md`):** Verificado el cumplimiento estricto de capas, transacciones ACID en `UsuarioRepository`, borrado lógico y cero secretos quemados en archivos versionados.
- [x] **Compuerta 3 (Requisitos Verificables):** Probados todos los escenarios de autenticación y autorización por roles.

---

## 2. Matriz de Verificación de Seguridad (v0.3)

- [x] Contraseñas almacenadas exclusivamente con hash `BCrypt` (`$2a$11$...`) en la tabla `usuario`.
- [x] `POST /api/Auth/login` retorna `200 OK` con token JWT firmado (`HmacSha256`) y lista de roles para credenciales válidas, y `401 Unauthorized` para credenciales inválidas o usuario inactivo.
- [x] Petición sin token a cualquier controlador académico o de usuarios retorna `401 Unauthorized`.
- [x] Petición con token de rol `Consultor` a `POST`, `PUT` o `DELETE` retorna `403 Forbidden`.
- [x] Petición con token de rol `Editor` a `DELETE` o a `/api/Usuario` retorna `403 Forbidden`.
- [x] Petición con token de rol `Administrador` permite crear y desactivar lógicamente usuarios en `/api/Usuario` y `/usuarios`.
