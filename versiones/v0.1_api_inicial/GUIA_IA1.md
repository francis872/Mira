# Guía de Trabajo con IA — Versión 1 (`GUIA_IA1.md`)

> **Eslabón entre Especificación (`v0.1`) y Código Construido**  
> Documenta los prompts ejecutados para construir la Entrega 1 a partir del Spec Kit y las correcciones aplicadas sobre la salida de la IA.

---

## 1. Prompt Principal de Generación de Backend y Scripts SQL (v0.1)

```text
Actúa como desarrollador senior .NET y ejecuta la implementación completa de la Entrega 1 siguiendo estrictamente el Spec Kit de versiones/v0.1_api_inicial/ y la constitución docs/1_constitution.md:
1. Implementar y verificar el CRUD completo de las 6 tablas sin claves foráneas en MIRA.Api con borrado lógico (activo = true/false), documentación Swagger y scripts DDL/Semillas en database/init/.
2. Tablas: area_conocimiento, objetivo_desarrollo_sostenible, area_aplicacion, termino_clave (PK string), universidad, linea_investigacion.
3. Arquitectura obligatoria: Controllers -> Servicios -> Repositorios (Dapper + Npgsql). Prohibido usar Entity Framework o DELETE físico.
```

## 2. Prompt de Generación del Frontend Desacoplado en Flask (v0.1)

```text
Crea la aplicación Frontend para la Entrega 1 en el directorio aislado frontend/ usando Python (Flask) + Jinja2 + Bootstrap 5 + librería requests.
Regla estricta: El Frontend NO debe tener conexión directa a PostgreSQL; debe consumir únicamente los endpoints HTTP de http://mira-api:8080/api.
```

## 3. Correcciones Realizadas a la IA durante la Versión 1

1. **Mapeo `snake_case` a `PascalCase` en Dapper:** Inicialmente las propiedades compuestas como `gran_area` y `termino_ingles` llegaban vacías al serializar si no tenían alias SQL. Se corrigió habilitando `DefaultTypeMap.MatchNamesWithUnderscores = true` en `MIRA.Api/Program.cs` y añadiendo alias explícitos en `AreaConocimientoRepository` y `TerminoClaveRepository`.
2. **Codificación URL en `termino_clave`:** Dado que `termino` es una PK de texto que admite espacios y tildes (ej. `"Inteligencia Artificial"`), se corrigió en `frontend/app.py` el uso de `urllib.parse.quote(termino, safe="")` antes de enviar peticiones `GET`, `PUT` o `DELETE` a la API.
