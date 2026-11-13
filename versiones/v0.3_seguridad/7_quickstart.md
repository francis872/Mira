# Guía de Inicio Rápido y Validación — MIRA v0.3 (`7_quickstart.md`)

> **Comandos de Verificación de Autenticación JWT y Control de Roles RBAC**

---

## 1. Diagrama Obligatorio: Ciclo de Validación de la Versión 3

```mermaid
flowchart LR
    Start["1. Levantar Entorno\ndocker compose up -d --build"] --> Login["2. Probar POST /api/Auth/login\ny obtener Token JWT"]
    Login --> Check401["3. Probar ruta sin token (401)\ny con rol insuficiente (403)"]
    Check401 --> CheckAdmin["4. Probar CRUD /api/Usuario\ncon Token Administrador (200/201)"]
    CheckAdmin --> Decision{"¿Pasan todos los\ncriterios + regresión v1-v2?"}
    Decision -- "Sí (Verde)" --> Tag["5. Cerrar versión:\ngit tag v3"]
    Decision -- "No (Rojo)" --> Fix["Corregir en código/spec\ny repetir validación"]
    Fix --> Login
```

> **Guía de lectura:** El diagrama de flujo muestra la secuencia de pasos para arrancar los contenedores, ejecutar las pruebas de humo de seguridad (`200`, `401`, `403`) y decidir el cierre con `tag v3`.

---

## 2. Comandos de Prueba Rápida (`curl`)

### 2.1. Iniciar sesión y obtener Token JWT (`200 OK`)
```bash
curl -X POST http://localhost:8081/api/Auth/login \
  -H "Content-Type: application/json" \
  -d "{\"username\": \"<USUARIO_ADMIN>\", \"password\": \"<PASSWORD_ADMIN>\"}"
```

### 2.2. Verificar bloqueo `401 Unauthorized` sin Token
```bash
curl -i http://localhost:8081/api/docente
```

### 2.3. Consultar listado de usuarios con Token de Administrador (`200 OK`)
```bash
curl -H "Authorization: Bearer <TOKEN_JWT>" http://localhost:8081/api/Usuario
```
