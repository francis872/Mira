# Informe de ejecución del prompt definitivo — MIRA V2

23 de octubre de 2026. Rama: `feature/mira-v1-v2-integracion`. La implementación inicial quedó local, sin commits ni push, a partir de `4b339ca`. Después el usuario autorizó expresamente nuevos commits y push, junto con una mejora adicional del diseño. Esta actualización se conserva en la misma rama; no se fusiona con `main`.

## 1. Estado general

MIRA funciona localmente en `http://localhost:5000`, con API/Swagger en `http://localhost:8081/swagger`. V1 conserva sus seis catálogos. **V2 completa no está terminada**: seis de los doce criterios están verificados y seis bloqueados por el modelo académico ausente.

## 2. Archivos afectados

Se modificaron 40 archivos existentes y se añadieron 16 archivos de implementación, documentación y evidencia antes de este informe. Principales cambios: seis repositorios, `Program.cs`, servicio y formulario de área, base y seis listados Flask, scripts de verificación, pruebas y los siete documentos existentes del kit V2.

Nuevos: migraciones 004/005, prueba SQL de catálogos, middleware de excepciones, pruebas HTTP de errores y guardia arquitectónica, CSS/JS propios, toolbar compartida, verificador CDP, prompt auténtico, matriz de criterios, reglas de negocio y dos capturas con su README. Inventario y rutas detalladas en [auditoría](auditoria-v2.md).

## 3. Requisitos implementados

RF-V2-SP-01: cinco operaciones V1 exclusivamente por rutinas. RF-V2-TR-01: trigger real para una regla existente. RF-V2-HTTP-01: errores seguros y contratos inválidos en español. RF-V2-UI-01: navegación, búsqueda, accesibilidad básica y estados de envío. Se preservaron las validaciones, CSRF y contratos anteriores.

## 4. Cambios arquitectónicos

Controller → Service → Repository → PostgreSQL se conserva. Repository solo invoca funciones con parámetros Dapper/Npgsql; las consultas e instrucciones CRUD sobre tablas están en PostgreSQL. Se centralizó normalización de área en un trigger y traducción de errores técnicos en middleware. Logs a consola evitan la dependencia de permisos Event Log que falló en las pruebas.

## 5. Rutinas PostgreSQL

`database/migrations/004_v2_catalogos_rutinas.sql` define 30 FUNCTION, cinco por catálogo: `sp_listar_*`, `sp_consultar_*`, `sp_crear_*`, `sp_actualizar_*`, `sp_eliminar_*`. Se invocan con SELECT; no se confunden con PROCEDURE/CALL. La migración 003 conserva sin cambios sus rutinas legadas. Contratos completos en `versiones/v2_profesor/6_contracts.md`.

## 6. Trigger verificado

`005_v2_area_aplicacion_trigger.sql`: `tr_area_aplicacion_normalizar` llama a `fn_area_aplicacion_normalizar`. Recorta espacios extremos de `nombre` en INSERT/UPDATE y rechaza vacío. No añade columnas ni modifica retroactivamente datos. Se comprobó por SQL con disparador habilitado/deshabilitado, API, formulario HTTP y navegador real.

## 7. Maestro-detalle

La atomicidad legada usuario→roles pasa sus pruebas SQL; no se expone en pantallas ni acredita el criterio académico. Universidad→grupo, grupo→semillero y línea→docente no se implementaron sin claves/columnas/cardinalidades oficiales. No existen pantallas académicas de varios renglones que puedan declararse operativas.

## 8. Mejoras de interfaz

Se añadieron búsqueda local con estado de coincidencias, página activa, enlace para saltar al contenido, nombres accesibles en controles, estado de procesamiento y prevención de reenvío, estilos coherentes y adaptación móvil. Se conservan Bootstrap, los conteos reales y la identidad MIRA. No se añadió login ni analítica de otra versión. [Capturas reales](evidencias/README.md).

La mejora posterior autorizada incorpora navegación lateral, encabezado contextual, nuevas tarjetas, estilos de formularios y confirmaciones más claras. Se verificó nuevamente con 41/42/31 pruebas, diez comprobaciones Edge y 52 combinaciones de página/ancho. [Rediseño y actualización GitHub](diseno-v2.md).

## 9. Pruebas

Antes: 31 .NET, 40 Flask y 30 HTTP. Después: **41 .NET, 42 Flask/arquitectura, 31 HTTP**, dos archivos SQL con rollback y **7 comprobaciones Edge**. La suite final no tuvo fallos ni omisiones .NET. Se comprobó persistencia tras reiniciar los tres servicios y repetición del arranque sin reaplicar migraciones. [Registro, entorno y límites](pruebas-v2.md).

## 10. Doce criterios

Verificados: C01, C03, C04, C10, C11 y C12. Bloqueados: C02, C05, C06, C07, C08 y C09. C07 tiene traducción técnica probada, pero carece aún de una FK académica integrada y su prueba real. [Matriz con evidencia y bloqueos](criterios-v2.md).

## 11. Pendientes reales

Se recuperaron del historial el plan, historias y cronograma, preservados en `docs/fuentes/historicos/`. No se localizaron `VERSION_2.md`, metodología del curso, mapa oficial, módulos, proyecto completo ni esquema académico vigente. El usuario indicó utilizar únicamente el modelo oficial, descartando un modelo provisional. La estructura oficial de nueve documentos y prompts personales de integrantes tampoco se aportaron; no se inventaron nombres, autores ni evidencias.

La limitación inicial de PostgreSQL 16 quedó resuelta: se ejecutó una plataforma independiente sobre 16.15, con 41/42/31 pruebas y ambos SQL aprobados en dos ejecuciones; [detalle](verificacion-postgres16.md). Se conservó la plataforma habitual sobre 17.9. Docker no está disponible. No se certifica producción, carga, escaneo CVE o accesibilidad exhaustiva. [Pendientes completos](pendientes-v2.md).

## 12. Ejecución reproducible

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-local.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-local.ps1
```

Preparación de dependencias, configuración, parada y navegador: [scripts/README.md](../scripts/README.md). No se borraron bases existentes; las bases exclusivas de auditoría se conservaron. La plataforma quedó activa y el navegador de pruebas aislado se cerró.

## 13. Sustentación

[Guía con diecisiete respuestas concretas](sustentacion-v2.md), [trazabilidad completa](trazabilidad-v2.md), [reglas por entidad](reglas-negocio-v2.md) y [procedencia de prompts](prompts/README.md). El nuevo adjunto se conservó byte por byte. La demostración debe limitarse a las funcionalidades verificadas y exponer el bloqueo de la parte académica V2.
