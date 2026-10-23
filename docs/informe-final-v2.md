# Informe de integración del modelo oficial MIRA V2

23 de octubre de 2026. Rama de trabajo `feature/mira-v1-v2-integracion`. El usuario autorizó commits/push y confirmó que publicar significa actualizar el repositorio GitHub.

## 1. Estado

Plataforma operativa en `http://localhost:5000/investigacion`, además de los seis catálogos V1. Once criterios verificados; C06 está bloqueado porque el SQL confirmado no contiene FK nullable. No se declara aceptación académica completa.

## 2. Archivos

Migraciones 006/007, dos SQL de pruebas, originales recuperados en `docs/fuentes/sql`, metadatos/modelo, controller, service y repository de investigación, módulo Flask, cinco plantillas y macros, JavaScript de renglones, pruebas .NET/Flask/HTTP y verificador Edge ampliado. Documentación, siete archivos del kit y contexto de prompts actualizados. No se alteró el texto de los prompts auténticos.

## 3. Requisitos

CRUD de once recursos, cinco asociaciones, FK por selectores reales, campos opcionales null, trigger oficial, errores 409 españoles y ficha grupo→semilleros con creación/edición/retiro de varios renglones. Conservados contratos V1, CSRF y ausencia de login.

## 4. Arquitectura

Controller → Service → Repository → PostgreSQL. Metadatos cerrados definen campos y nombres permitidos; Service valida; Repository solo llama funciones con parámetros. No se conecta Flask a la base ni se acepta un identificador SQL arbitrario del cliente.

## 5. Rutinas

007 añade 78 funciones: 55 CRUD de once entidades, 20 de cinco puentes y tres de referencias/ficha. Las 30 de V1 permanecen. Todas son FUNCTION invocadas mediante SELECT; los PROCEDURE legados de 003 se conservan.

## 6. Triggers

Conservado trigger oficial de grupo activo para semilleros; su excepción ahora usa 23503 para traducirse a 409. Pruebas SQL y API verifican la restricción. El trigger V1 de normalización continúa demostrado desde navegador.

## 7. Maestro-detalle

`sp_guardar_v2_ficha_grupo` bloquea el maestro, guarda grupo y detalles y retira lógicamente renglones omitidos. Un error tardío revierte todas las escrituras. No permite editar semilleros ajenos ni repetir el mismo id. Se demostró con SQL, API, formularios HTTP y navegador.

## 8. Interfaz

Nuevo acceso Investigación, once módulos, cinco asociaciones y ficha con renglones agregables/editables/retirables. Mantiene el diseño académico, búsqueda, mensajes, navegación móvil y referencias comprensibles. [Capturas](evidencias/README.md).

## 9. Pruebas

PostgreSQL 16.15: **47 .NET, 63 Flask/arquitectura, 46 HTTP**, sin fallos ni omisiones; cuatro SQL aprobados con rollback. Comparación del esquema contra la fuente: 19 tablas, sin diferencias de columnas/nulabilidad/restricciones/índices. Edge sobre plataforma habitual: **14 controles, 72 combinaciones página/ancho, cero excepciones JS**. [Registro](pruebas-v2.md).

## 10. Criterios

Verificados C01–C05 y C07–C12. C06 bloqueado por ausencia de FK opcionales en el SQL; no se inventó ninguna. [Evidencia por criterio](criterios-v2.md).

## 11. Pendientes

Confirmar no aplicabilidad de C06 y obtener VERSION_2/rúbrica/nombres del kit de nueve documentos. Para crear docentes en la plataforma habitual hacen falta personas institucionales reales cargadas: el selector está vacío y no se generaron credenciales ficticias. Docker, Python 3.11 y producción sin verificar. [Lista concreta](pendientes-v2.md).

## 12. Operación

Antes de actualizar se creó `.local/before-official-model-20261023153312.dump`. Se aplicaron 006/007 con registro de migraciones y se reiniciaron procesos propios; no se eliminaron datos V1. Inicio: `scripts/start-local.ps1`; pruebas completas aisladas: `scripts/verify-postgres16.ps1`; Swagger: `http://localhost:8081/swagger`. Navegador de pruebas cerrado después del recorrido.

## 13. Sustentación

[Guía de 17 respuestas](sustentacion-v2.md), [trazabilidad](trazabilidad-v2.md), [reglas](reglas-negocio-v2.md), [fuentes](fuentes/README.md) y [prompts auténticos](prompts/README.md). El SQL existe en otra rama y fue confirmado por el usuario; la incertidumbre anterior queda corregida explícitamente.
