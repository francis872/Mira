# Guía de sustentación de MIRA

## Objetivo y estado

MIRA apoya la gestión de información de investigación académica de la Universidad de San Buenaventura Medellín. La parte demostrable actualmente gestiona seis catálogos reutilizables; no constituye todavía un sistema completo de proyectos académicos. V2 añade infraestructura de migraciones y rutinas maestro-detalle usuario→roles en PostgreSQL. Su dominio académico está pendiente del modelo oficial.

La auditoría adaptó un prompt de Backstage al repositorio real con confirmación del usuario. `docs/PROMPT.md` aporta contexto de trabajo; los requisitos viven en `versiones/`. Un prompt no sustituye la especificación ni demuestra que una funcionalidad esté terminada.

## Arquitectura que debe explicarse

Flask recibe formularios, valida CSRF y consume HTTP JSON. ASP.NET Core recibe el contrato y aplica validación de modelos. Los servicios contienen reglas y normalización. Los repositorios ejecutan SQL parametrizado con Dapper/Npgsql. PostgreSQL conserva registros, restricciones y rutinas. Consultar el diagrama y la secuencia concreta en [arquitectura-v2.md](arquitectura-v2.md).

## Ejemplos trazables

| Elemento | Universidad | Conflicto de término | Maestro-detalle de base de datos |
|---|---|---|---|
| Requisito | RF-MIRA-CAT-05 | RF-MIRA-TC-01 | RF-V2-MD-01/02/03/04 |
| Documento | `versiones/v0.1_api_inicial/2_spec.md` | `versiones/v2_profesor/2_spec.md` | Mismo Spec Kit V2 |
| Entrada | nombre, tipo y ciudad | término existente activo o inactivo | correo, hash sintético y arreglo de IDs de roles |
| Interfaz | `/universidad/nuevo` | `/termino_clave/nuevo` | Solo SQL; no hay pantalla ni endpoint |
| Código | `frontend/app.py:universidad_create`, `UniversidadController.Create`, `UniversidadService.CreateAsync`, `UniversidadRepository.CreateAsync` | `TerminoClaveService.CreateAsync`, `TerminoClaveRepository.CreateAsync`, `TerminoClaveController.Create` | `database/migrations/003_v2_usuarios_roles.sql:sp_usuario_*`, `fn_usuario_*` |
| Proceso | CSRF → JSON → validación → INSERT parametrizado → listado | Detección previa y captura de UNIQUE para inactivos/carreras | Una rutina PostgreSQL valida roles y escribe maestro y detalles atómicamente |
| Salida | HTTP 201, registro con ID; Flask redirige al listado | HTTP 409 y mensaje; no sobrescribe ni reactiva | JSONB compuesto; fallo FK no deja cambios parciales |
| Prueba | `FullLifecycle_CreateReadListUpdateLogicalDelete`, E2E universidad | `InactiveAndConcurrentDuplicateKeyword_ReturnsConflict`, E2E duplicado | `database/tests/v2_usuarios_roles.sql` |

## Demostración reproducible

1. Ejecutar el arranque local de [scripts/README.md](../scripts/README.md) y abrir `http://localhost:5000`. No se requieren credenciales de aplicación.
2. Mostrar dashboard y conteos reales. Abrir Universidades y contrastar el número de activos.
3. Crear una universidad con nombre identificable, tipo y ciudad. Recargar el listado y abrirlo en otra pestaña: el dato procede de PostgreSQL.
4. Editarla, volver al listado y mostrar el cambio. Inactivarla mediante el control de eliminación; desaparece de los activos. La prueba .NET comprueba que la fila sigue en la base.
5. Crear un término clave y volver a enviarlo. Mostrar el mensaje de duplicado. Inactivarlo e intentar crearlo otra vez: conflicto sin reactivación.
6. Consultar Swagger en `http://localhost:8081/swagger`. Mostrar contrato JSON y un 400 al enviar un texto demasiado largo.
7. Ejecutar `scripts/verify-local.ps1` y explicar sus tres niveles de pruebas y la base exclusiva para SQL/.NET. No confundir clientes HTTP con automatización visual de navegador.
8. Mostrar la matriz de trazabilidad, las decisiones de arquitectura y los pendientes de la entrega.

## Pruebas y limitaciones para declarar

La auditoría ejecutó 31 pruebas .NET con PostgreSQL real, 40 Flask y 30 E2E HTTP; las rutinas SQL pasaron con rollback. Se comprobó persistencia después de reiniciar los tres servicios. Los detalles están en [pruebas-v2.md](pruebas-v2.md).

No declarar que están implementados maestro-detalle académicos, selectores FK o triggers del dominio. No presentar las tablas de usuarios como una autenticación disponible. La interfaz mantiene identidad MIRA; no incluye mapas, inteligencia geoespacial ni SOI. No afirmar despliegue, operación productiva, accesibilidad completa o ausencia de vulnerabilidades. Docker no está disponible en el entorno utilizado.

## Criterio de cierre

La sustentación puede demostrar los seis catálogos y las rutinas SQL existentes. La entrega V2 completa requiere incorporar el modelo oficial, actualizar contratos y especificaciones, implementar el dominio pendiente y volver a verificarlo. Operatividad local, preparación de despliegue y producción son estados distintos.
