# Contratos API v0.1 — Módulo de Investigación

**Formato:** OpenAPI 3.0 (Swagger)  
**Base URL:** `http://localhost:5000/api`  
**Autenticación:** JWT Bearer Token  
**Fecha:** Agosto 2026

---

## 1. Autenticación (HU-1)

### POST /auth/login

**Descripción:** Autenticar usuario por email y contraseña, retornar token JWT.

#### Request
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

#### Response 200 OK
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 86400,
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "roles": ["Coordinador"]
  }
}
```

#### Status Codes
- `200 OK` - Autenticación exitosa
- `400 Bad Request` - Email o contraseña inválida
- `401 Unauthorized` - Credenciales rechazadas

---

## 2. Catálogos (HU-2)

### GET /catalogos/areas-conocimiento

**Descripción:** Listar todas las Áreas del Conocimiento activas.

#### Response 200 OK
```json
{
  "data": [
    {
      "id": 1,
      "tipo": "gran_area",
      "nombre": "Ingeniería y Tecnología",
      "activo": true
    },
    {
      "id": 2,
      "tipo": "area",
      "nombre": "Ingeniería de Sistemas",
      "activo": true
    }
  ],
  "total": 150
}
```

#### Query Parameters
- `tipo?` (string): Filtro por 'gran_area', 'area', 'disciplina'
- `page?` (int): Paginación (default: 1)
- `pageSize?` (int): Items por página (default: 20)

---

### POST /catalogos/areas-conocimiento

**Descripción:** Crear nueva Área de Conocimiento (solo Admin).

#### Request
```json
{
  "tipo": "disciplina",
  "nombre": "Machine Learning"
}
```

#### Response 201 Created
```json
{
  "id": 451,
  "tipo": "disciplina",
  "nombre": "Machine Learning",
  "activo": true,
  "createdAt": "2026-09-04T10:30:00Z"
}
```

#### Status Codes
- `201 Created` - Creado exitosamente
- `400 Bad Request` - Validación fallida
- `403 Forbidden` - No es Admin

---

### GET /catalogos/ods

**Descripción:** Listar los 17 ODS disponibles.

#### Response 200 OK
```json
{
  "data": [
    {
      "id": 1,
      "numero": 1,
      "nombre": "Fin de la Pobreza",
      "categoria": "social",
      "activo": true
    },
    {
      "id": 2,
      "numero": 2,
      "nombre": "Hambre Cero",
      "categoria": "social",
      "activo": true
    }
  ],
  "total": 17
}
```

---

## 3. Perfiles Docentes (HU-3)

### POST /docentes

**Descripción:** Registrar perfil de docente/investigador (solo usuario autenticado para sí mismo).

#### Request
```json
{
  "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
  "cvlacUrl": "https://scienti.colciencias.gov.co/cvlac/visualizador/...",
  "escalafon": "Profesor Asociado",
  "mincienciasCategoria": "Senior"
}
```

#### Response 200 OK
```json
{
  "id": 10,
  "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
  "cvlacUrl": "https://scienti.colciencias.gov.co/cvlac/visualizador/...",
  "escalafon": "Profesor Asociado",
  "mincienciasCategoria": "Senior",
  "createdAt": "2026-09-04T10:30:00Z",
  "updatedAt": "2026-09-04T10:30:00Z"
}
```

#### Status Codes
- `200 OK` - Guardado/actualizado
- `400 Bad Request` - URL CvLAC inválida
- `403 Forbidden` - No puedes editar perfil de otro usuario

---

## 4. Sedes (HU-4)

### GET /sedes

**Descripción:** Listar todas las sedes activas.

#### Response 200 OK
```json
{
  "data": [
    {
      "id": 1,
      "nombre": "Sede Medellín",
      "ciudad": "Medellín",
      "codigoInstitucional": "MED",
      "activo": true
    },
    {
      "id": 2,
      "nombre": "Sede Cali",
      "ciudad": "Cali",
      "codigoInstitucional": "CAL",
      "activo": true
    }
  ]
}
```

---

### POST /sedes

**Descripción:** Crear nueva sede (solo Admin).

#### Request
```json
{
  "nombre": "Sede Cartagena",
  "ciudad": "Cartagena",
  "codigoInstitucional": "CAR"
}
```

#### Response 201 Created
```json
{
  "id": 5,
  "nombre": "Sede Cartagena",
  "ciudad": "Cartagena",
  "codigoInstitucional": "CAR",
  "activo": true,
  "createdAt": "2026-09-04T10:30:00Z"
}
```

---

## 5. Grupos de Investigación (HU-5)

### POST /grupos

**Descripción:** Crear grupo de investigación (solo Administrador).

#### Request
```json
{
  "gruptacUrl": "https://colciencias.gov.co/grupos/COL123456",
  "categoria": "A",
  "sedeId": 1
}
```

#### Response 201 Created
```json
{
  "id": 100,
  "gruptacUrl": "https://colciencias.gov.co/grupos/COL123456",
  "categoria": "A",
  "sedeId": 1,
  "activo": true,
  "createdAt": "2026-09-04T10:30:00Z"
}
```

---

### POST /grupos/{grupoId}/docentes

**Descripción:** Vincular docente a grupo con rol y período.

#### Request
```json
{
  "docenteId": 10,
  "rol": "Líder de Grupo",
  "fechaInicio": "2026-01-01",
  "fechaFin": null
}
```

#### Response 201 Created
```json
{
  "id": 500,
  "grupoId": 100,
  "docenteId": 10,
  "rol": "Líder de Grupo",
  "fechaInicio": "2026-01-01",
  "fechaFin": null,
  "activo": true,
  "createdAt": "2026-09-04T10:30:00Z"
}
```

#### Status Codes
- `201 Created` - Vinculación exitosa
- `400 Bad Request` - Validaciones fallidas (ej. ya existe otro Líder activo)
- `409 Conflict` - Duplicidad de persona+rol+período

---

### GET /grupos/{grupoId}/exportar

**Descripción:** Exportar lista de integrantes en Excel o PDF.

#### Query Parameters
- `formato` (string): 'xlsx' o 'pdf'

#### Response 200 OK
Archivo binario descargable.

---

## 6. Semilleros (HU-6)

### POST /semilleros

**Descripción:** Crear semillero vinculado a grupo activo.

#### Request
```json
{
  "grupoId": 100,
  "nombre": "Semillero IA Aplicada",
  "descripcion": "Investigación en IA para procesos empresariales"
}
```

#### Response 201 Created
```json
{
  "id": 50,
  "grupoId": 100,
  "nombre": "Semillero IA Aplicada",
  "descripcion": "Investigación en IA para procesos empresariales",
  "activo": true,
  "createdAt": "2026-09-04T10:30:00Z"
}
```

#### Status Codes
- `201 Created` - Creado
- `400 Bad Request` - Grupo inactivo o no existe

---

### POST /semilleros/{semilleroId}/tutores

**Descripción:** Asignar docente tutor a semillero.

#### Request
```json
{
  "docenteId": 10,
  "rol": "Director",
  "fechaInicio": "2026-01-01",
  "fechaFin": null
}
```

#### Response 201 Created
```json
{
  "id": 200,
  "semilleroId": 50,
  "docenteId": 10,
  "rol": "Director",
  "fechaInicio": "2026-01-01",
  "fechaFin": null,
  "createdAt": "2026-09-04T10:30:00Z"
}
```

#### Status Codes
- `201 Created` - Asignación exitosa
- `409 Conflict` - Duplicidad persona+rol+período

---

## 7. Líneas de Investigación (HU-7, HU-9)

### POST /lineas

**Descripción:** Crear línea de investigación (solo Coordinador/Admin).

#### Request
```json
{
  "codigo": "LIN-001",
  "nombre": "Línea de IA y Automatización",
  "descripcion": "Investigación en inteligencia artificial para procesos"
}
```

#### Response 201 Created
```json
{
  "id": 1,
  "codigo": "LIN-001",
  "nombre": "Línea de IA y Automatización",
  "descripcion": "Investigación en inteligencia artificial para procesos",
  "activo": true,
  "createdAt": "2026-09-04T10:30:00Z"
}
```

---

### PUT /lineas/{lineaId}

**Descripción:** Editar línea y articular con Disciplinas, ODS, Áreas de Aplicación.

#### Request
```json
{
  "nombre": "Línea de IA y Automatización (v2)",
  "descripcion": "Actualizado",
  "activo": true,
  "disciplinas": [2, 5],
  "ods": [8, 9, 12],
  "aplicaciones": [1, 3]
}
```

#### Response 200 OK
```json
{
  "id": 1,
  "codigo": "LIN-001",
  "nombre": "Línea de IA y Automatización (v2)",
  "descripcion": "Actualizado",
  "activo": true,
  "disciplinas": [2, 5],
  "ods": [8, 9, 12],
  "aplicaciones": [1, 3],
  "updatedAt": "2026-09-04T11:00:00Z"
}
```

#### Status Codes
- `200 OK` - Actualizado
- `400 Bad Request` - Falta 1 ODS o 1 Área de Conocimiento
- `404 Not Found` - Línea no existe

---

### GET /lineas/activas

**Descripción:** Listar solo líneas activas (para selecciones múltiples).

#### Response 200 OK
```json
{
  "data": [
    {
      "id": 1,
      "codigo": "LIN-001",
      "nombre": "Línea de IA y Automatización"
    },
    {
      "id": 2,
      "codigo": "LIN-002",
      "nombre": "Línea de Sostenibilidad"
    }
  ]
}
```

---

## 8. Articulación (HU-8)

### POST /grupos/{grupoId}/lineas

**Descripción:** Vincular grupo a una o varias líneas de investigación.

#### Request
```json
{
  "lineasIds": [1, 2, 5]
}
```

#### Response 200 OK
```json
{
  "grupoId": 100,
  "lineasIds": [1, 2, 5],
  "lineas": [
    { "id": 1, "nombre": "Línea de IA y Automatización" },
    { "id": 2, "nombre": "Línea de Sostenibilidad" },
    { "id": 5, "nombre": "Línea de Gobernanza" }
  ],
  "updatedAt": "2026-09-04T11:00:00Z"
}
```

---

### DELETE /grupos/{grupoId}/lineas/{lineaId}

**Descripción:** Desvincular línea de grupo (con confirmación en frontend).

#### Response 200 OK
```json
{
  "message": "Línea desvinculada exitosamente",
  "grupoId": 100,
  "lineaId": 1
}
```

---

## 9. Reportes (HU-10)

### GET /reportes/trazabilidad

**Descripción:** Dashboard unificado de articulación investigativa.

#### Query Parameters
- `sedeId?` (int): Filtro por sede
- `lineaId?` (int): Filtro por línea
- `page?` (int): Paginación

#### Response 200 OK
```json
{
  "data": [
    {
      "grupoId": 100,
      "grupoNombre": "Grupo IA",
      "sedeId": 1,
      "sedeName": "Medellín",
      "semilleros": [
        {
          "id": 50,
          "nombre": "Semillero IA Aplicada",
          "tutores": [
            { "id": 10, "nombre": "Dr. Juan", "rol": "Director" }
          ]
        }
      ],
      "lineas": [1, 2],
      "ods": [8, 9, 12],
      "docentes": [
        { "id": 10, "nombre": "Dr. Juan", "rol": "Líder de Grupo" }
      ]
    }
  ],
  "total": 45
}
```

---

### GET /reportes/exportar

**Descripción:** Descargar matriz completa en Excel o CSV.

#### Query Parameters
- `formato` (string): 'xlsx' o 'csv'
- `sedeId?` (int): Aplica mismo filtro
- `lineaId?` (int): Aplica mismo filtro

#### Response 200 OK
Archivo binario descargable.

---

## 10. Errores Estándar

### Error 400 Bad Request
```json
{
  "errorCode": "VALIDATION_ERROR",
  "message": "Validación fallida",
  "errors": [
    {
      "field": "email",
      "message": "Email inválido"
    },
    {
      "field": "cvlacUrl",
      "message": "URL debe comenzar con https://"
    }
  ]
}
```

### Error 401 Unauthorized
```json
{
  "errorCode": "UNAUTHORIZED",
  "message": "Token inválido o expirado"
}
```

### Error 403 Forbidden
```json
{
  "errorCode": "INSUFFICIENT_PERMISSIONS",
  "message": "No tienes permiso para realizar esta acción",
  "requiredRoles": ["Admin", "Coordinador"]
}
```

### Error 404 Not Found
```json
{
  "errorCode": "RESOURCE_NOT_FOUND",
  "message": "Grupo de investigación no encontrado",
  "resourceId": 100
}
```

### Error 409 Conflict
```json
{
  "errorCode": "DUPLICATE_ENTRY",
  "message": "Ya existe un Líder de Grupo activo en este período",
  "existingId": 500
}
```

---

**Generado:** 2026-09-04  
**Versión:** 0.1  
**Estado:** Aprobado para Sprint 3
