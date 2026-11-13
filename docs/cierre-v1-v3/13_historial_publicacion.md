# Control de cambios Fase 5 — sin ejecutar commits

HEAD 3105273af84e55a1e813e76f2acb92acc31caa94, rama feature/mira-v3 y remoto francis872/Mira conservados; índice sin preparar. Respaldo independiente previo 6fdeba6c2d67801a39ead866801b8b48ed7e4519e94ee08918a4bc29ab57baaa. [Inventario exacto antes/después](evidencias/fase5-20261107/control-final.json). No hubo acceso ni escritura en BACKSTAGE en esta fase.

La corrección LOGIN_FORWARDING_KEY de Fase 4 y D01 conservan hashes; 001–008 no cambiaron. 009–014 son nuevas. Los 111 archivos pendientes previos no se descartan: las modificaciones legítimas adicionales se comparan contra sus 323 hashes, no solo contra HEAD.

Propuesta revisable por responsabilidad, sin staging/commit/push:

1. Seguridad D01 y configuración previa, manteniendo separación del cambio CI Fase 4.
2. Corrección CI efímera y gates ya existentes de Fase 4.
3. V1: 009, inglés/categoría en modelo/servicios/formularios y pruebas V1.
4. V2 dominio: 010/012/013/014, metadatos, DTO, servicio/repositorio y pruebas de integridad/concurrencia/estados.
5. V2 interfaz: multiselección, filtros, resúmenes, confirmación con foco y opciones de sede vivas.
6. V2 reportes: 011, endpoints, Flask, plantillas, dependencias/lock/Dockerfile y pruebas de exportación.
7. Verificación y cierre académico: nuevos recorridos HTTP/navegador/zoom, matriz, informe y evidencia saneada.

Controller, servicio, repositorio e investigacion.py contienen responsabilidades compartidas: revisar hunks o agrupar 4–6 en un commit coherente; no stagear archivos completos automáticamente. Conservar originales y evidencia histórica. Las capturas/dumps/.env/configuración privada no son material publicable; solo capturas saneadas elegidas y reportes de metadatos.

La siguiente decisión debe resolver los pendientes académicos y aprobar el diff concreto. Publicar, cambiar Draft/merge o desplegar requiere autorización posterior; no se ejecuta en esta fase.

---

## Corte previo conservado como antecedente

# Historial, preservación y propuesta de publicación

Repositorio autorizado: https://github.com/francis872/Mira.git, branch feature/mira-v3. HEAD 3105273af84e55a1e813e76f2acb92acc31caa94 sin cambios durante este encargo. PR #7 abierto/Draft/no fusionado; base main 35640853dc4cbb4dd50f17c9208acb2feedaff1d; 51 commits/197 archivos en la comparación publicada. Incluye 43 commits anteriores V1/V2 y ocho V3; no describirlo como diff exclusivo de ocho commits. [Consulta actual de solo lectura](evidencias/pr.json).

| Referencia comprobable | Significado |
|---|---|
| 157f61f137ab13ea1f1535724fe532595f360588 | Preservación de historias, plan y cronograma originales; blobs y SHA256 verificados otra vez |
| 880ac758e322ff51d9e4f3cc54d6f7f8ee50c640 | SQL que el usuario confirmó el 23 de octubre de 2026; base del modelo006/007 |
| 3ba563a | Spec y auditoría inicial V3 |
| f9f29f0 | JWT revocable y autorización API |
| 4265eba | Sesiones privadas/cuentas Flask |
| 446a1a0 | UI institucional/accesibilidad |
| 96fcd08 | Dependencias Python fijadas |
| 42e6e55 | Docker/Gunicorn/ejemploRender |
| f333cee | Verificación aislada/C06 |
| 3105273 | Matriz/sustentación/evidencias publicadas, anterior a correcciones locales |

No se inventan fechas para commits abreviados ni se confunden fechas de archivos con fecha de implementación. Las fuentes binarias coinciden con sus blobs. No se crearon tags de versiones ni nuevos commits durante el cierre.

## Evolución de los cambios sin commit

El plan anterior conservaba49 candidatos (21 modificados/28 nuevos), agrupados en interfaz, verificadores, CI y documentación. Fase 3 modificó seis de ellos para D01, conservó 43 y añadió 15, dejando 64 (31 modificados/33 nuevos). Al iniciar este encargo los 64 coincidían con la huella esperada de Fase 3; ningún cambio ajeno fue descartado. El diff D01 previo comprende21 archivos (16 existentes/5 nuevos), distinto del inventario total 64.

En este encargo se añadieron solo documentos/evidencias en docs/cierre-v1-v3 y material privado de auditoría ignorado en .local. No se modificaron los 64 pendientes iniciales ni ningún archivo previamente versionado. README y guías históricas permanecen como antecedentes fechados; el informe nuevo es el corte consolidado. [Inventario final y responsabilidad por archivo](evidencias/inventario.json).

Respaldos verificados antes de escribir documentos nuevos:

| ZIP en carpeta hermana MIRA-respaldos | SHA256 |
|---|---|
| fase2-20261107-143626-3c61e13c.zip | 622b7e6611d6a270a43eba79710c49e620c9cd532c175027841bf4f7ef973a15 |
| fase3-pre-d01-20261107-162137-b9cfe7c9.zip | 377ec7f9ab94b1a73971503a8d06e194fbedfabd30b6e99b16d3c3a0e9a2c803 |
| cierre-pre-documentos-20261107-180231-b5ffa531.zip | 32636149c9d0b44c27b5458c1a57642f565a0b68c13c2a1cfb5cc2f32f430e1c |

Son respaldos privados, no archivos para subir al repositorio. CRC y hashes comprobados. No sustituyen una restauración real de producción.

## Commits propuestos, no ejecutados

[Lista exacta por grupo](evidencias/plan_commits.json). Los cuatro grupos antiguos se preservan como antecedente; se separa D01 por responsabilidad y se añade esta consolidación, evitando doble inclusión de archivos. La cuenta final se verifica automáticamente contra git status.

1. `fix(a11y): corregir ficha vacía y controles accesibles` — cinco archivos de UI/tests de regresión.
2. `test(verification): fechar evidencias y comprobar zoom real` — seis auditores/verificadores.
3. `fix(auth): autenticar origen Flask y conservar cuotas de login` —21 archivos de D01, tests y configuración compartida.
4. `ci(quality): preparar validación con PostgreSQL efímero` — cuatro archivos CI. **Bloqueado** hasta autorizar y aplicar la fixture LOGIN_FORWARDING_KEY; validar de nuevo antes de publicarlo.
5. `docs(academia): preservar cierre previo y evidencias V3` —28 archivos del cierre anterior.
6. `docs(academia): consolidar trazabilidad y dictámenes V1 a V3` — todos los archivos nuevos de docs/cierre-v1-v3.

Los grupos forman una propuesta del estado final. Si se necesita que cada commit intermedio compile y sea completamente autónomo, debe usarse staging por hunks en archivos compartidos y verificar cada commit aprobado; no se ejecutó ni se garantiza ahora bisectabilidad de estados intermedios. No usar git add indiscriminado. No incluir .local ni los respaldos ni el ZIP privado. Respetar los cambios locales previos y mantener hashes/inventario.

Después de autorización: resolver primero CI y alcance académico, validar cambios autorizados, revisar secretos/diff por grupo, crear únicamente commits aprobados, actualizar matriz y hashes si cambia la fuente, y solicitar/usar autorización específica para push al mismo PR. No cambiar main, branch, Draft o hacer merge/despliegue por consecuencia automática. Actions remoto y revisión humana deben evaluar el nuevo SHA; los tests locales actuales no certifican ese SHA futuro.

SAPIENS es portada institucional del producto actual. No se renombraron namespaces, endpoints, JWT, tablas o migraciones. **No iniciar V4** hasta resolver alcance original, C06, gate CI, aceptación de límites y entrega/publicación autorizada. No se atribuyen a una V4 inexistente las funcionalidades históricas todavía pendientes.
