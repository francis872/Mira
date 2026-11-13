# Guía de Trabajo con IA — Versión 3 (`GUIA_IA3.md`)

> **Eslabón entre Especificación (`v0.3`) y Código Construido**  
> Registra los prompts utilizados para implementar la autenticación JWT, el hashing BCrypt y el control de acceso RBAC, así como las correcciones aplicadas.

---

## 1. Prompt Principal de Seguridad JWT, BCrypt y RBAC (v0.3)

```text
Implementa la Entrega 3 (v0.3) agregando seguridad completa al sistema MIRA según el Spec Kit en versiones/v0.3_seguridad/:
1. Crea el script database/init/04_seguridad_v0.3.sql con las tablas rol, usuario y rol_usuario, incluyendo borrado lógico (activo = TRUE) y semillas para los roles Administrador, Editor y Consultor, más un usuario administrador inicial con hash BCrypt.
2. En MIRA.Api, instala BCrypt.Net-Next y Microsoft.AspNetCore.Authentication.JwtBearer. Crea los DTOs en Peticiones/ (LoginRequest, LoginResponse, UsuarioCrear, UsuarioResponse) y las clases UsuarioRepository, UsuarioService, AuthService, AuthController y UsuarioController.
3. Protege todos los controladores con [Authorize]: lectura GET para cualquier usuario autenticado, creación/edición (POST/PUT) para Administrador y Editor, y eliminación (DELETE) y gestión de usuarios exclusivamente para Administrador.
4. En frontend/app.py, agrega /login, /logout, el módulo /usuarios y los decoradores @login_required y @roles_required inyectando Authorization: Bearer <token>.
```

## 2. Correcciones Realizadas a la IA durante la Versión 3

1. **Sincronización de Secuencias `SERIAL` en PostgreSQL tras Semillas con ID Explícito:** Al insertar el usuario `id = 1` y los roles `1, 2, 3` en `04_seguridad_v0.3.sql`, la secuencia `usuario_id_seq` quedaba en `1`, provocando colisión al crear un nuevo usuario desde la interfaz. Se corrigió añadiendo `SELECT setval('usuario_id_seq', (SELECT COALESCE(MAX(id), 1) FROM usuario));` al final del script SQL.
2. **Renovación de Sesión en Flask cuando el Token JWT Expira:** Cuando la cookie de sesión de Flask conservaba un token JWT ya expirado, la API respondía `401 Unauthorized` y las vistas mostraban listas vacías. Se corrigió en `frontend/app.py` para detectar `response.status_code == 401`, limpiar automáticamente `session.clear()` y redirigir a `/login`.
